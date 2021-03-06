#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Network.hpp>
#include <SFML/System.hpp>
#include <iostream>
#include <vector>
#include "Player.h"
using namespace std;

class Collisions
{
public:
	Collisions();
	bool TestFall(sf::Vector2f);
	bool TestHookHit(Player*, Player*);

	//hole
	sf::Vector2f holeTopLeft;
	sf::Vector2f holeBottomRight;
	sf::RectangleShape holeShape;

	//general
	float boundaryX = 1080;
	float boundaryY = 720;
};

