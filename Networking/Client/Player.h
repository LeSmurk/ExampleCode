#pragma once
#include <SFML/Graphics.hpp>
#include <iostream>
using namespace std;

class Player
{
public:
	//Functions
	Player(int);
	void TCPUpdate();
	void PosUpdate(sf::Vector2f);

	void UDPUpdate();
	//prediction and interpolation
	sf::Vector2f prevPos;
	sf::Vector2f lerpPos;
	void CheckPrediction(float x, float y);
	sf::Clock lerpTimer;

	sf::Vector2f TestValid(sf::Vector2f);

	sf::Vector2f InitHook(float , float);
	void UpdateHook();
	void ReleaseHook();

	//net
	bool isConnected = false;
	int offsetTime;
	bool TCPMode;

	//Variables
	sf::CircleShape shape;
	sf::Vector2f position;
	sf::Vector2f direction;

	int player1 = 0;
	float speed = 5;
	float playerSize = 10;
	int ID = 0;
	int lives = 3;
	
	//packet info
	int num = 0;

	//general
	float boundaryX = 1080;
	float boundaryY = 720;

	//hook
	bool hookThrow = false;

	sf::Clock hookTimer;
	bool isHooking = false;
	bool retractHook = false;
	bool gotHooked = false;

	sf::Vector2f hookDir;
	sf::RectangleShape hookShape;
	sf::CircleShape hookHead;
	float hookSpeed = 20;

	sf::Vector2f startingPos1 = sf::Vector2f(520, 20);
	sf::Vector2f startingPos2 = sf::Vector2f(520, 680);
	sf::Vector2f spawnPos;

};

