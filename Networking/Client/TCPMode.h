#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Network.hpp>
#include <iostream>
#include "Player.h"
using namespace std;

class TCPMode
{
public:
	void ConnectToServer(Player*, Player*);
	void SendToServer(sf::Packet);
	void RecFromServer(Player*, Player*, sf::Packet);
	void TCPRun(Player*, Player*, sf::Vector2f, bool);

	sf::Clock globalTime;
	//offset between this and the sever
	int offsetTime;

	bool sendFirst = false;

	sf::TcpSocket socket;
	sf::Vector2f prevDirection;
	bool msgReceived = true;

	int lostConnection = 0;
	bool endGame = false;
	sf::IpAddress address = ("192.168.0.72");

	//sending packet
	sf::Packet sendPack;

	sf::Clock packetLossClock;

	sf::Clock posClock;
};

