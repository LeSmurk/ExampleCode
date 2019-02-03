#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Network.hpp>
#include <SFML/System.hpp>
#include <iostream>
#include <vector>
#include "ClientHolder.h"
#include "Collisions.h"
using namespace std;

class TCPMode
{
public:
	//functions
	TCPMode();
	sf::Packet ReadPacket(sf::Packet currentPacket, ClientHolder* currentClient);
	void RunTCP();
	void ConnectTCP();
	void UpdateTCP();

	//offset latency time
	sf::Clock globalClock;

	//determine if all the clients are connected or not
	bool connected = false;

	//endgame
	bool endGame = false;

	//variables
	vector<ClientHolder*> clientList;
	//gameplay
	sf::Vector2f startingPos1;
	sf::Vector2f startingPos2;

	//net
	sf::SocketSelector selector;
	sf::TcpListener listener;

	Collisions collision;

};

