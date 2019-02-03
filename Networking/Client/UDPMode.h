#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Network.hpp>
#include <SFML/System.hpp>
#include <iostream>
#include "Player.h"
#include "Collisions.h"
using namespace std;
class UDPMode
{
public:
	void ConnectToServer(Player*, Player*, int);
	void SendToServer(sf::Packet);
	void RecFromServer(Player*, Player*, sf::Packet);
	void UDPRun(Player*, Player*, sf::Vector2f, bool);
	void UDPCheckCollisions(Player*, Player*);
	void Receive();

	sf::Clock globalTime;
	int offsetTime;
	bool sendFirst = false;

	sf::UdpSocket socket;
	sf::IpAddress address = "192.168.0.72";
	unsigned short sendPort = 53000;

	sf::Clock lostConnection;
	bool startConnectionTimer;
	bool endGame = false;

	sf::Clock clockSFML;
	sf::Clock hookClock;

	Player* tempPlayer;
	sf::Vector2f tempDirection;
	bool tempDir;

	//collision object
	Collisions collision;

	//player tracking
	Player* playerStored1;
	Player* playerStored2;
};

