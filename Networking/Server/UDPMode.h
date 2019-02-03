#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Network.hpp>
#include <SFML/System.hpp>
#include <iostream>
#include <vector>
#include "ClientHolder.h"
#include "Collisions.h"
using namespace std;

class UDPMode
{
public:
	//functions
	UDPMode();
	sf::Packet ReadPacket(sf::Packet currentPacket, ClientHolder* currentClient);
	void InitUDP();
	void ConnectUDP();
	void RebindUDP(ClientHolder* newClient);
	void RunUDP();

	sf::Clock globalClock;

	bool connected = false;
	bool endGame = false;

	//variables
	vector<ClientHolder*> clientList;
	//gameplay
	sf::Vector2f startingPos1;
	sf::Vector2f startingPos2;

	//net
	sf::SocketSelector selector;

	Collisions collision;
};

