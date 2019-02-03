#include "TCPMode.h"

TCPMode::TCPMode()
{
	//assign listener a port
	listener.listen(54000);
	selector.add(listener);

	//initialise gameplay
	startingPos1 = sf::Vector2f(520, 20);
	startingPos2 = sf::Vector2f(520, 680);
}

sf::Packet TCPMode::ReadPacket(sf::Packet currentPacket, ClientHolder* currentClient)
{
	//recieve msg
	int ID;
	int num;
	bool dir;
	float posX;
	float posY;
	int lives;
	currentPacket >> ID >> num >> dir >> posX >> posY >> lives;

	//store which packet number we are on
	currentClient->num = num;

	//repack
	sf::Packet returnPack;
	
	//player movement
	if (ID < 3)
	{
		//if a regular direction update
		if (dir)
		{
			currentClient->direction.x = posX;
			currentClient->direction.y = posY;

			//return directions
			returnPack << ID << num << dir << posX << posY << lives;
		}

		//direct position update
		else
		{
			currentClient->PosUpdate(posX, posY);
			returnPack << ID << num << dir << currentClient->position.x << currentClient->position.y << lives;
		}
	}

	//hook
	else
	{
		//intiialise the hook and tell the other client to init too
		currentClient->InitHook(posX, posY);
		returnPack << ID << num << dir << posX << posY << lives;
	}

	//send back to clients
	return returnPack;
}


void TCPMode::ConnectTCP()
{
	//keep checking until all clients are connected
	if(!connected)
	{
		if (selector.wait(sf::milliseconds(10)))
		{
			//listener test for new client
			if (selector.isReady(listener))
			{
				// accept a new connection
				ClientHolder* client = new ClientHolder;
				if (listener.accept(client->tcpSock) == sf::Socket::Done)
				{
					//add client to the selector
					selector.add(client->tcpSock);

					//put client into list
					clientList.push_back(client);

					//says connected
					cout << "connected to client " << clientList.back()->tcpSock.getRemoteAddress() << endl;

					//set the starting positions from the server
					if (clientList.size() == 1)
					{
						client->spawnPos = startingPos1;
						client->position = startingPos1;
					}

					else if (clientList.size() == 2)
					{
						client->spawnPos = startingPos2;
						client->position = startingPos2;
					}

					client->TCPMode = true;
					//keep track of which player this is
					client->ID = clientList.size();
					cout << "Player connected: " << client->ID << endl;

					if (clientList.size() >= 2)
						connected = true;
				}
			}
		}
	}

	//once all players are connected, tell them to start
	if(connected)
	{
		cout << "launch game" << endl;

		sf::Packet pack;

		for (int i = 0; i < clientList.size(); i++)
		{
			//clear the packet
			pack.clear();

			//tell the player which one it is
			pack << clientList.at(i)->ID << 0 << true << 0 << 0 << 3;

			clientList.at(i)->tcpSock.send(pack);

			//unblock socket
			clientList.at(i)->tcpSock.setBlocking(false);
		}
	}
}

void TCPMode::RunTCP()
{
	//wait for selector to recieve message
	if (selector.wait(sf::milliseconds(10)))
	{
		//recieve and send messages
		sf::Packet pack;

		//check which client is ready to send
		for (int i = 0; i < clientList.size(); i++)
		{
			if (selector.isReady(clientList.at(i)->tcpSock))
			{
				pack.clear();
				//recieve msg and cancel if disconnect mid way
				if (clientList.at(i)->tcpSock.receive(pack) != sf::Socket::Done)
				{
					//Disconnected
					cout << "client disconnected" << endl;
					clientList.at(i)->tcpSock.disconnect();
					endGame = true;
					break;
				}

				//unpack the packet and make new packet to return
				sf::Packet returnPack = ReadPacket(pack, clientList.at(i));

				//send the updated players to the clients
				for (int z = 0; z < clientList.size(); z++)
				{
					clientList.at(z)->tcpSock.send(returnPack);
				}
			}
		}
	}
}

void TCPMode::UpdateTCP()
{
	//update player every frame for tcp
	for (int i = 0; i < clientList.size(); i++)
	{
		//check collisions
		sf::Packet colPack;

		//HOLE COLLISIONS
		if (collision.TestFall(clientList.at(i)->position))
		{
			//decrement lives
			clientList.at(i)->lives--;

			//toggle off hooked
			clientList.at(i)->gotHooked = false;
			clientList.at(i)->speed = 5;
			clientList.at(i)->direction = sf::Vector2f(0, 0);

			//reset at spawn
			clientList.at(i)->position = clientList.at(i)->spawnPos;
			cout << "player " << clientList.at(i)->ID << " died and is now on " << clientList.at(i)->lives << endl;

			//new message
			clientList.at(i)->num++;

			colPack.clear();
			colPack << clientList.at(i)->ID << clientList.at(i)->num << true << clientList.at(i)->position.x << clientList.at(i)->position.y << clientList.at(i)->lives;

			//send the message to the clients
			for (int z = 0; z < clientList.size(); z++)
			{
				clientList.at(z)->tcpSock.send(colPack);
			}
		}


		//HOOK COLLISIONS
		if (collision.TestHookHit(i, &clientList))
		{
			//hook hit something so draw it back
			clientList.at(i)->retractHook = true;

			//new message
			clientList.at(i)->num++;

			//tell the clients to draw it back
			colPack.clear();
			colPack << clientList.at(i)->ID + 2 << clientList.at(i)->num << false << clientList.at(i)->hookDir.x << clientList.at(i)->hookDir.y << 0;

			//send the message to the clients
			for (int z = 0; z < clientList.size(); z++)
			{
				clientList.at(z)->tcpSock.send(colPack);
			}
		}
		
		//update server side
		clientList.at(i)->TCPUpdate();

		//check for endgame
		if (clientList.at(i)->lives <= 0)
			endGame = true;
	}
	
}