#include "UDPMode.h"

void UDPMode::ConnectToServer(Player* player1, Player* player2, int thisPort)
{
	//check if already sent first info
	if (!sendFirst)
	{
		//toggle sent first message
		sendFirst = true;

		if (socket.bind(thisPort) != sf::Socket::Done)
		{
			cout << "Port already used on this internet" << endl;
			socket.bind(thisPort + 1) != sf::Socket::Done;
		}

		socket.setBlocking(true);

		//send the first packet to port 53000 and get a new port based on which player
		sf::Packet pack;
		pack << player1->ID << player1->num << false << 0 << 0 << 3;

		if (socket.send(pack, address, sendPort) != sf::Socket::Done)
		{
			cout << "Resending" << endl;
			sendFirst = false;
		}

		//set to udp
		player1->TCPMode = false;
		player2->TCPMode = false;
	}

	//wait for first connection
	sf::Packet recPack;
	unsigned short portRec;
	sf::IpAddress addRec;
	socket.receive(recPack, addRec, portRec);

	//if the packet isn't empty, something was received
	if (recPack.getDataSize() != 0)
	{
		int ID;
		int num;
		bool dir;
		float posX;
		float posY;
		int lives;

		recPack >> ID >> num >> dir >> posX >> posY >> lives;

		//set this as player1 or 2 and set new port
		if (player1->ID == 0)
		{
			//this is player 1
			if (ID == 1)
			{
				player1->ID = 1;
				player1->shape.setFillColor(sf::Color::Blue);
				player1->position = player1->startingPos1;
				player1->spawnPos = player2->startingPos1;

				player2->ID = 2;
				player2->shape.setFillColor(sf::Color::Green);
				player2->position = player2->startingPos2;
				player2->spawnPos = player2->startingPos2;

			}
			//this is player 2
			else
			{
				player1->ID = 2;
				player1->shape.setFillColor(sf::Color::Green);
				player1->position = player1->startingPos2;
				player1->spawnPos = player2->startingPos2;

				player2->ID = 1;
				player2->shape.setFillColor(sf::Color::Blue);
				player2->position = player2->startingPos1;
				player2->spawnPos = player2->startingPos1;
			}

			//update the first positions
			player1->shape.setPosition(player1->position);
			player2->shape.setPosition(player2->position);

			//make the new send port based on which player it is
			sendPort += ID;
			cout << "This player is:  " << ID << endl;

			//ping back to tell offset time
			socket.setBlocking(true);

			sf::Packet pack;
			pack << player1->ID << player1->num << true << 0 << 0 << 3;

			SendToServer(pack);

			socket.receive(recPack, addRec, portRec);

			//determine offset time
			recPack >> ID >> offsetTime;

			cout << "offset time: " << offsetTime << endl;

			//this client is connected, so start
			player1->isConnected = true;

			socket.setBlocking(false);
		}

		//simulation time
		globalTime.restart();
	}
}

void UDPMode::SendToServer(sf::Packet pack)
{
	if (socket.send(pack, address, sendPort) != sf::Socket::Done)
	{
		cout << "sending error" << endl;	
	}
}

void UDPMode::RecFromServer(Player* player1, Player* player2, sf::Packet pack)
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
		//determine what kind of message the server is sending

		//update position
		//player1->UDPUpdate(sf::Vector2f(posX, posY));
	}

	//packet for player 2
	else if(player2->ID == ID)
	{
		if (lives > -1 && lives > player2->lives)
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

		//update positon
		player2->CheckPrediction(posX, posY);
	}

	//hook for player 1
	else if (player1->ID + 2 == ID)
	{

	}

	//hook for player 2
	else if (player2->ID + 2 == ID)
	{

		//move the hook
		//start hook
		if (!player2->isHooking)
			player2->InitHook(posX, posY);

		//hook already out
		else if(player2->isHooking)
		{
			//retract hook if the direction is retraction
			player2->retractHook = dir;

			player2->hookHead.setPosition(posX, posY);

		}
		
	}
}

//checking collsioons client side
void UDPMode::UDPCheckCollisions(Player* player1, Player* player2)
{
	//check collisions
	sf::Packet colPack;

	//HOLE COLLISIONS
	if (collision.TestFall(player1->position))
	{
		//decrement lives
		player1->lives--;

		//toggle off hooked
		player1->gotHooked = false;
		player1->speed = 5;
		player1->direction = sf::Vector2f(0, 0);

		//reset at spawn
		player1->position = player1->spawnPos;
		cout << "player " << player1->ID << " died and is now on " << player1->lives << endl;
	}


	//HOOK COLLISIONS
	if (collision.TestHookHit(player1, player2))
	{
		//make sure hook is not already retracting
		if (!player1->retractHook)
		{
			//hook hit something so draw it back
			player1->retractHook = true;
		}
	}	
}

//MAIN FOR UDP
void UDPMode::UDPRun(Player* player1, Player* player2, sf::Vector2f moveDirection, bool hookThrow)
{
	//store players
	playerStored1 = player1;
	playerStored2 = player2;


	player2->offsetTime = offsetTime;
	//any input to player1
	if (!hookThrow && !player1->gotHooked)
	{
		player1->direction = moveDirection;
	}

	//update client side
	player1->UDPUpdate();
	player2->UDPUpdate();
	UDPCheckCollisions(player1, player2);
	UDPCheckCollisions(player2, player1);

	//setup packet
	sf::Packet sendPack;

	sf::Time timerHook = hookClock.getElapsedTime();

	//constantly throw the hook
	if (player1->isHooking && timerHook.asMilliseconds() >= 60)
	{
		//say message has been sent to start hook
		player1->hookThrow = false;

		sendPack.clear();

		//send whether retracting or not
		//Store message into the packet
		sendPack << player1->ID + 2 << player1->num << player1->retractHook << player1->hookHead.getPosition().x << player1->hookHead.getPosition().y << player1->lives;

		//send the message to server
		SendToServer(sendPack);
	}

	sf::Time timer = clockSFML.getElapsedTime();

	//Update the player constantly every 60ms
	if (timer.asMilliseconds() >= 60 && !player1->isHooking)
	{
		clockSFML.restart();

		sendPack.clear();

		//packet number doesnt matter in udp mode
		sendPack << player1->ID << player1->num << false << player1->position.x << player1->position.y << player1->lives;
		SendToServer(sendPack);
	}

}

void UDPMode::Receive()
{
	while (true)
	{
		sf::Packet recPack;

		unsigned short portRec;
		sf::IpAddress addRec;
		//Update the other client's player
		socket.receive(recPack, addRec, portRec);

		//if the packet isn't empty, set the new packet info
		if (recPack.getDataSize() != 0)
		{
			RecFromServer(playerStored1, playerStored2, recPack);

			//sever and client still connected
			startConnectionTimer = false;
		}

		//start a timer for how long messages have failed to receive
		if (!startConnectionTimer)
		{
			lostConnection.restart();
			startConnectionTimer = true;
		}

		//if too many failed messages, end the game
		sf::Time timer = lostConnection.getElapsedTime();
		if (timer.asSeconds() > 2)
			endGame = true;
	}
}
