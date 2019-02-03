#include "TCPMode.h"

void TCPMode::ConnectToServer(Player* player1, Player* player2)
{
	//check if already sent first info
	if (!sendFirst)
	{
		//toggle sent first messae
		sendFirst = true;

		sf::Socket::Status status = socket.connect(address, 54000);
		if (status != sf::Socket::Done)
		{
			//error handling
			cout << "Retrying" << endl;
			sendFirst = false;
		}

		//send first packet
		sf::Packet firstPack;
		firstPack << player1->ID << player1->num << true << 0 << 0 << 3;

		SendToServer(firstPack);

		//set to tcp
		player1->TCPMode = true;
		player2->TCPMode = true;
	}

	//wait for first connection
	sf::Packet firstRecPack;
	socket.receive(firstRecPack);

	//if the packet isn't empty, something was received
	if (firstRecPack.getDataSize() != 0)
	{
		int ID;
		int num;
		bool dir;
		float posX;
		float posY;
		int lives;

		//num will be the server's global time
		firstRecPack >> ID >> num >> dir >> posX >> posY >> lives;

		//set this as player1 or 2
		if (player1->ID == 0)
		{
			//received first message
			msgReceived = true;

			if (ID == 1)
			{
				player1->ID = 1;
				player1->shape.setFillColor(sf::Color::Blue);
				player1->position = player1->startingPos1;

				player2->ID = 2;
				player2->shape.setFillColor(sf::Color::Green);
				player2->position = player2->startingPos2;
			}
			else
			{
				player1->ID = 2;
				player1->shape.setFillColor(sf::Color::Green);
				player1->position = player1->startingPos2;

				player2->ID = 1;
				player2->shape.setFillColor(sf::Color::Blue);
				player2->position = player2->startingPos1;
			}

			cout << "This is player: " << player1->ID << endl;
		}
		
		//this client is connected, so start
		player1->isConnected = true;

		//simulation time
		globalTime.restart();

		socket.setBlocking(false);
	}

}

void TCPMode::SendToServer(sf::Packet pack)
{
	if (socket.send(pack) != sf::Socket::Done)
	{
		cout << "sending error" << endl;
	}
}

void TCPMode::RecFromServer(Player* player1, Player* player2, sf::Packet pack)
{
	int ID;
	int num;
	bool dir;
	float posX;
	float posY;
	int lives;

	pack >> ID >> num >> dir >> posX >> posY >> lives;

	//identify which player the packet is for
	//packet is for player 1
	if (player1->ID == ID)
	{
		//check that this is the message sent
		if (num == player1->num && !msgReceived)
		{
			msgReceived = true;
			lostConnection = 0;
		}
		else if(num == player1->num && !msgReceived)
		{
			cout << "not right message number: " << num << endl;
		}

		//original message from server
		if (num != player1->num)
		{
			//determine what kind of message the server is sending
			//life lost
			if (lives > -1)
			{
				if (lives != player1->lives)
				{
					//decrease lives
					player1->lives = lives;

					//toggle off hooked
					player1->gotHooked = false;
					player1->speed = 5;
					player1->direction = sf::Vector2f(0, 0);

					//move to spawn position
					player1->position.x = posX;
					player1->position.y = posY;

					cout << "player " << player1->ID << " died and is now on " << lives << endl;
				}
			}

			//player was grabbed by hook
			else if (lives == -1)
			{
				cout << "got hooked" << endl;

				//move player towards the origin of the hook
				player1->direction.x = posX;
				player1->direction.y = posY;

				//cancel the hook if they were hooking
				player1->ReleaseHook();
				//player is hooked
				player1->speed = 15;
				player1->gotHooked = true;
			}		

			//readjust numbering
			player1->num = num;
		}

		//regular tcp direction packet
		else if (dir)
		{
			player1->direction.x = posX;
			player1->direction.y = posY;
		}

		//direct position update
		else
			player1->PosUpdate(sf::Vector2f(posX, posY));
	}

	//packet for player 2
	else if(player2->ID == ID)
	{
		if (lives > -1 && lives != player2->lives)
		{
			//decrease lives
			player2->lives = lives;

			//toggle off hooked
			player2->gotHooked = false;
			player2->speed = 5;
			player2->direction = sf::Vector2f(0, 0);

			//move to spawn position
			player2->position.x = posX;
			player2->position.y = posY;

			cout << "player " << player2->ID << " died and is now on " << lives << endl;
		}

		//player was grabbed by hook
		else if (lives == -1)
		{
			cout << "got hooked" << endl;

			//move player towards the origin of the hook
			player2->direction.x = posX;
			player2->direction.y = posY;

			//cancel the hook if they were hooking
			player2->ReleaseHook();
			//player is hooked
			player2->speed = 15;
			player2->gotHooked = true;
		}

		//regular direction tcp packet
		else if (dir)
		{
			player2->direction.x = posX;
			player2->direction.y = posY;
		}

		//direct position update
		else
			player2->PosUpdate(sf::Vector2f(posX, posY));
	}

	//hook for player 1
	else if (player1->ID + 2 == ID)
	{
		//check that this is the message sent
		if (num == player1->num && !msgReceived)
		{
			msgReceived = true;
		}

		//original message from server
		if (num != player1->num)
		{
			//if hook is 0, retract hook
			if (lives == 0)
				player1->retractHook = true;

			//readjust numbering
			player1->num++;
		}

		//tell the hook that it was thrown on the server
		else if (lives > 0)
			player1->hookThrow = false;
	}

	//hook for player 2
	else if (player2->ID + 2 == ID)
	{
		//if lives is 0, retract hook
		if (lives == 0)
			player2->retractHook = true;

		//start the hook at given location
		else if(lives > 0)
		{
			//start hook
			player2->InitHook(posX, posY);
		}
	}

}

//MAIN FOR TCP
void TCPMode::TCPRun(Player* player1, Player* player2, sf::Vector2f moveDirection, bool hookThrow)
{
	//message for hook init
	if (hookThrow && msgReceived)
	{
		sendPack.clear();

		//increment which message this is
		player1->num++;

		//Store message into the packet
		sendPack << player1->ID + 2 << player1->num << false << moveDirection.x << moveDirection.y << player1->lives;

		//send the message to server
		SendToServer(sendPack);

		//start a timer and bool to test msg arrives
		packetLossClock.restart();
		msgReceived = false;
	}

	//occassionally send a position to verify with the server
	sf::Time timer = posClock.getElapsedTime();
	//every 3 seconds (as long as the player has just moved obviously)
	if (timer.asSeconds() >= 3 && msgReceived)
	{
		sendPack.clear();

		//increment which message this is
		player1->num++;

		//send the message with client position
		//Store message into the packet
		sendPack << player1->ID << player1->num << false << player1->position.x << player1->position.y << player1->lives;

		//send the message to server
		SendToServer(sendPack);

		//start a timer and bool to test msg arrives
		packetLossClock.restart();
		msgReceived = false;

		//reset the position update timer
		posClock.restart();
	}

	//Update the player only if the player inputs a move command and last message arrived
	else if (moveDirection != prevDirection && msgReceived)
	{
		sendPack.clear();

		//increment which message this is
		player1->num++;

		//Store message into the packet
		sendPack << player1->ID << player1->num << true << moveDirection.x << moveDirection.y << player1->lives;

		//send the message to server
		SendToServer(sendPack);

		//update the previous direction
		prevDirection = moveDirection;

		//start a timer and bool to test msg arrives
		packetLossClock.restart();
		msgReceived = false;
	}

	//receive packet
	sf::Packet recPack;

	//Receive message from server
	socket.receive(recPack);

	//if the packet isn't empty, something was received
	if (recPack.getDataSize() != 0)
	{
		RecFromServer(player1, player2, recPack);
	}

	//if the messaged was not received back for 30 milliseconds, resend
	if (!msgReceived)
	{
		sf::Time timer = packetLossClock.getElapsedTime();

		if (timer.asMilliseconds() >= 200)
		{
			cout << "resending messge" << endl;
			SendToServer(sendPack);
			packetLossClock.restart();
			//if too many times attempted to resend, just cancel the connection
			lostConnection++;
			if (lostConnection > 4)
				endGame = true;
		}
	}

	//move the players
	player1->TCPUpdate();
	player2->TCPUpdate();
}
