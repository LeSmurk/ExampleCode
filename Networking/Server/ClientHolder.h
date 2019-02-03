#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Network.hpp>
#include <iostream>
using namespace std;

class ClientHolder
{
public:
	//functions
	ClientHolder();

	void TCPUpdate();
	void PosUpdate(float x, float y);

	void UDPUpdate();
	//prediction and interpolation
	sf::Vector2f prevPos;
	sf::Vector2f lerpPos;
	void CheckPrediction(float x, float y);
	sf::Clock lerpTimer;

	sf::Vector2f TestValid(sf::Vector2f);

	void InitHook(float, float);
	void UpdateHook();
	void ReleaseHook();

	//net
	sf::Clock lostConnection;
	bool startConnectionTimer;
	//latency in MS
	int offsetTime;
	//timer for udp updates
	sf::Clock updateTimer;
	bool TCPMode;

	//Player
	sf::CircleShape shape;
	sf::Vector2f position;
	sf::Vector2f direction = sf::Vector2f(0.0, 0.0);
	float playerSize = 10;
	float speed = 5;

	int ID = 0;
	int num = 0;
	int lives = 3;
	sf::Vector2f spawnPos;
	bool gotHooked = false;

	//hook
	sf::Clock hookTimer;
	bool isHooking = false;
	bool retractHook = false;
	sf::Vector2f hookDir;
	sf::RectangleShape hookShape;
	sf::CircleShape hookHead;
	float hookSpeed = 20;

	//network side
	sf::TcpSocket tcpSock;
	sf::UdpSocket udpSock;
	sf::IpAddress address = "192.168.0.72";
	unsigned short sendPort = 54000;

	//general
	float boundaryX = 1080;
	float boundaryY = 720;

};

