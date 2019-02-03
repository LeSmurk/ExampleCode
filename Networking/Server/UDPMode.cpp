#include "UDPMode.h"

UDPMode::UDPMode()
{
	//initialise gameplay
	startingPos1 = sf::Vector2f(520, 20);
	startingPos2 = sf::Vector2f(520, 680);
}

sf::Packet UDPMode::ReadPacket(sf::Packet currentPacket, ClientHolder* currentClient)
{
	//sever and client still connected
	currentClient->startConnectionTimer = false;

	//recieve msg
	int ID;
	int num;
	bool dir;
	float posX;
	float posY;
	int lives;
	currentPacket >> ID >> num >> dir >> posX >> posY >> lives;
	
	//repack
	sf::Packet returnPack;
	
	//player position update
	if(ID < 3)
	{
		//update the server client position
		currentClient->CheckPrediction(posX, posY);

		returnPack << ID << num << dir << posX << posY << lives;
	}

	//hook
	else
	{
		//hook needs to be thrown
		if (!currentClient->isHooking)
			currentClient->InitHook(posX, posY);

		//hook is already out
		else if (currentClient->isHooking)
		{
			//retracting (direction in this case is true for retracting)
			currentClient->retractHook = dir;

			currentClient->hookHead.setPosition(posX, posY);
		}

		returnPack << ID << num << dir << posX << posY << lives;
	}

	//send back to clients
	return returnPack;
}

//used to allow players to join using one port and then changes once connected
void UDPMode::InitUDP()
{
	//set up fake client for first port
	ClientHolder* client = new ClientHolder;

	//sets up first connection at port 53000
	if (client->udpSock.bind(53000) != sf::Socket::Done)
	{
		cout << "binding error" << endl;
	}

	//add fake client to the selector
	selector.add(client->udpSock);

	//add fake client to the list
	clientList.push_back(client);
}

void UDPMode::ConnectUDP()
{
	//keep checking until all clients are connected
	if (!connected)
	{
		if (selector.wait(sf::milliseconds(10)))
		{
			sf::Packet pack;

			//check for clients first message (SENDING TO THE FAKE CLIENT PORT)
			if (selector.isReady(clientList.at(clientList.size() - 1)->udpSock))
			{
				unsigned short portRec;
				sf::IpAddress addRec;

				//recieve msg and cancel if disconnect mid way
				if (clientList.at(clientList.size() - 1)->udpSock.receive(pack, addRec, portRec) != sf::Socket::Done)
				{
					//Disconnected
					cout << "Message rec failed" << endl;
					
				}

				//store address
				clientList.at(clientList.size() - 1)->address = addRec;
				//set to udp
				clientList.at(clientList.size() - 1)->TCPMode = false;

				//rebind to new port
				RebindUDP(clientList.at(clientList.size() - 1));

				if (clientList.size() - 1 >= 2)
					connected = true;
			}
		}
	}

	//once all players are connected, tell them to start
	if (connected)
	{
		//remove the fake client from the list
		clientList.pop_back();

		cout << "launch game" << endl;

		sf::Packet pack;

		for (int i = 0; i < clientList.size(); i++)
		{
			//block to wait for offset time
			clientList.at(i)->udpSock.setBlocking(true);

			//clear the packet
			pack.clear();

			//tell the client which player it is
			pack << clientList.at(i)->ID << 0 << false << 0 << 0 << 3;

			//start a timer
			globalClock.restart();

			clientList.at(i)->udpSock.send(pack, clientList.at(i)->address, clientList.at(i)->sendPort);

			pack.clear();

			unsigned short portRec;
			sf::IpAddress addRec;
			//wait for message back
			clientList.at(i)->udpSock.receive(pack, addRec, portRec);

			//determine how long offset is
			sf::Time timer = globalClock.getElapsedTime();
			clientList.at(i)->offsetTime = timer.asMilliseconds();
			//half the number as it is a round trip
			clientList.at(i)->offsetTime = clientList.at(i)->offsetTime / 2;

			pack.clear();
			//tell the client this info
			pack << clientList.at(i)->ID << clientList.at(i)->offsetTime;
			clientList.at(i)->udpSock.send(pack, clientList.at(i)->address, clientList.at(i)->sendPort);

			//unblock socket
			clientList.at(i)->udpSock.setBlocking(false);

			cout << "offset time: " << clientList.at(i)->offsetTime << endl;
		}

		//simulation time
		globalClock.restart();
	}
}

void UDPMode::RebindUDP(ClientHolder* newClient)
{
	//unbind the client from the connection port
	newClient->udpSock.unbind();

	//rebind the actual client to a port based on how many players are connected
	newClient->udpSock.bind(53000 + clientList.size());

	//set the port that the server sends to
	newClient->sendPort = 54000 + clientList.size();
	cout << "sending to: " << newClient->sendPort << endl;

	//make a new fake client for new players to send to port
	ClientHolder* client = new ClientHolder;
	if (client->udpSock.bind(53000) != sf::Socket::Done)
	{
		cout << "connection error" << endl;
	}

	//puts the fake client port to the back of the list
	clientList.push_back(client);

	//add client to the selector
	selector.add(client->udpSock);

	//set the starting positions from the server
	if (clientList.size() - 1 == 1)
	{
		newClient->spawnPos = startingPos1;
		newClient->position = startingPos1;
	}

	else if (clientList.size() - 1 == 2)
	{
		newClient->spawnPos = startingPos2;
		newClient->position = startingPos2;
	}

	//keep track of which player this is
	newClient->ID = clientList.size() - 1;
	cout << "Player connected: " << newClient->ID << endl;

	cout << "rebinding to: " << newClient->udpSock.getLocalPort() << endl;
}

void UDPMode::RunUDP()
{
	//wait for selector to find a client or recieve message
	if (selector.wait(sf::milliseconds(1)))
	{
		//loop through all the clients
		for (int i = 0; i < clientList.size(); i++)
		{
			sf::Packet pack;

			//check for clients ready to send
			if (selector.isReady(clientList.at(i)->udpSock))
			{
				unsigned short portRec;
				sf::IpAddress addRec;

				//recieve msg
				clientList.at(i)->udpSock.receive(pack, addRec, portRec);

				sf::Packet returnPack;
				//unpack the packet and make new packet to return
				if(pack.getDataSize() != 0)
					returnPack = ReadPacket(pack, clientList.at(i));

				//send the updated players to the clients
				for (int z = 0; z < clientList.size(); z++)
				{
					//send message out to all clients. if the send fails, start a timer
					clientList.at(z)->udpSock.send(returnPack, clientList.at(z)->address, clientList.at(z)->sendPort);
				}
			}
		}
	}

		/////////////////////////////////////////
		//update server side
	for (int i = 0; i < clientList.size(); i++)
	{
		clientList.at(i)->UDPUpdate();

		//check for endgame
		if (clientList.at(i)->lives <= 0)
			endGame = true;

		//start a timer for how long messages have failed to send
		if (!clientList.at(i)->startConnectionTimer)
		{
			clientList.at(i)->lostConnection.restart();
			clientList.at(i)->startConnectionTimer = true;
		}

		//if too many failed messages, end the game
		sf::Time timer = clientList.at(i)->lostConnection.getElapsedTime();
		if (timer.asSeconds() > 2)
		{
			cout << "client DC" << endl;
			endGame = true;
		}
			
	}
}