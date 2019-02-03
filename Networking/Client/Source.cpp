#include <SFML/Graphics.hpp>
#include <SFML/Network.hpp>
#include <iostream>
#include "Player.h"
#include "TCPMode.h"
#include "UDPMode.h"
using namespace std;

//globals
//font
sf::Font font;

//window
sf::Vector2f winSize(1080, 720);
sf::RenderWindow window(sf::VideoMode(winSize.x, winSize.y), "Client's Game");


//player object
Player player1(1);
Player player2(2);

//tcp or udp
TCPMode TCP;
UDPMode UDP;
bool TCPEnabled;
int port;

bool recThreadLaunched = false;
sf::Thread threadRec(&UDPMode::Receive, &UDP);

//gamestates
enum GameState
{
	Menu, Connecting, Game
};
//switch between gamestates
GameState state = Menu;

//ID - Which client the packet belongs to
//num - Which packet number it is
//dir - Whether the following floats are positions or directions (FALSE FOR POSITION)
struct PlayerPacket
{
	int ID;
	int num;
	bool dir;
	float posX;
	float posY;
	int lives;
};

sf::Packet& operator << (sf::Packet& packet, const PlayerPacket& character)
{
	return packet << character.ID << character.num << character.dir << character.posX << character.posY << character.lives;
}

sf::Packet& operator >> (sf::Packet& packet, PlayerPacket& character)
{
	return packet >> character.ID >> character.num >> character.dir >> character.posX >> character.posY >> character.lives;
}


sf::Vector2f Input()
{
	//local input pos
	sf::Vector2f moveVec(0, 0);

	//prevent any change if player is hooking or got hooked
	if (!player1.isHooking && !player1.gotHooked)
	{
		//change control based on player
		if (player1.ID == 1)
		{
			//Hooking
			if (sf::Mouse::isButtonPressed(sf::Mouse::Left))
				return player1.InitHook(sf::Mouse::getPosition(window).x, sf::Mouse::getPosition(window).y);

			//basic movement
			if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left))
				moveVec.x = -1;

			else if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right))
				moveVec.x = 1;

			if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up))
				moveVec.y = -1;

			else if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down))
				moveVec.y = 1;
		}

		else if (player1.ID == 2)
		{
			//Hooking
			if (sf::Mouse::isButtonPressed(sf::Mouse::Right))
				return player1.InitHook(sf::Mouse::getPosition(window).x, sf::Mouse::getPosition(window).y);

			//basic movement
			if (sf::Keyboard::isKeyPressed(sf::Keyboard::A))
				moveVec.x = -1;

			else if (sf::Keyboard::isKeyPressed(sf::Keyboard::D))
				moveVec.x = 1;

			if (sf::Keyboard::isKeyPressed(sf::Keyboard::W))
				moveVec.y = -1;

			else if (sf::Keyboard::isKeyPressed(sf::Keyboard::S))
				moveVec.y = 1;
		}
	}

	//check if valid movement and return position changes
	return moveVec;

}

//drawing
void Render()
{
	//other objects on the screen
	sf::RectangleShape middleLine;
	middleLine.setFillColor(sf::Color::Red);
	middleLine.setSize(sf::Vector2f(winSize.x, 1));
	middleLine.setPosition(0, (winSize.y / 2));

	sf::Vector2f holeTopLeft = sf::Vector2f(460, 280);
	sf::Vector2f holeBottomRight = sf::Vector2f(620, 440);

	sf::RectangleShape holeShape;
	holeShape.setPosition(holeTopLeft);
	holeShape.setSize(sf::Vector2f(holeBottomRight.x - holeTopLeft.x, holeBottomRight.y - holeTopLeft.y));
	holeShape.setOutlineColor(sf::Color::Red);
	holeShape.setOutlineThickness(-2);
	holeShape.setFillColor(sf::Color::White);

	//lives
	sf::String lives1Msg;
	sf::String lives2Msg;

	if (player1.ID == 1)
	{
		lives1Msg = std::to_string(player1.lives);
		lives2Msg = std::to_string(player2.lives);
	}
	else 
	{
		lives1Msg = std::to_string(player2.lives);
		lives2Msg = std::to_string(player1.lives);
	}

	sf::Text lives1Text(lives1Msg, font, 30);
	lives1Text.setOrigin(lives1Text.getLocalBounds().width / 2, lives1Text.getLocalBounds().height / 2);
	lives1Text.setPosition(winSize.x / 2, winSize.y / 2 - 45);
	lives1Text.setFillColor(sf::Color::Black);
	lives1Text.setOutlineThickness(2);
	lives1Text.setOutlineColor(sf::Color::Blue);

	sf::Text lives2Text(lives2Msg, font, 30);
	lives2Text.setOrigin(lives2Text.getLocalBounds().width / 2, lives2Text.getLocalBounds().height / 2);
	lives2Text.setPosition(winSize.x / 2, winSize.y / 2 + 40);
	lives2Text.setFillColor(sf::Color::Black);
	lives2Text.setOutlineThickness(2);
	lives2Text.setOutlineColor(sf::Color::Green);

	//clear the window
	window.clear();

	//draw player
	window.draw(player1.shape);
	window.draw(player2.shape);

	//draw hooks
	window.draw(player1.hookShape);
	window.draw(player1.hookHead);
	window.draw(player2.hookShape);
	window.draw(player2.hookHead);

	//draw middle line
	window.draw(middleLine);

	//draw hole
	window.draw(holeShape);

	//display lives
	window.draw(lives1Text);
	window.draw(lives2Text);

	//display to window
	window.display();
}

void RunMenu()
{
	//draw buttons
	//tcp
	sf::String tcpMsg = "TCP";
	sf::Text tcpText(tcpMsg, font, 30);
	tcpText.setOrigin(tcpText.getLocalBounds().width / 2, tcpText.getLocalBounds().height / 2);
	tcpText.setPosition(winSize.x / 2, (winSize.y / 2) - 50);
	tcpText.setFillColor(sf::Color::Blue);
	tcpText.setOutlineThickness(1);
	tcpText.setOutlineColor(sf::Color::White);
	tcpText.setStyle(sf::Text::Bold);

	//udp
	sf::String udpMsg = "UDP";
	sf::Text udpText(udpMsg, font, 30);
	udpText.setOrigin(udpText.getLocalBounds().width / 2, udpText.getLocalBounds().height / 2);
	udpText.setPosition(winSize.x / 2, (winSize.y / 2) + 50);
	udpText.setFillColor(sf::Color::Green);
	udpText.setOutlineThickness(1);
	udpText.setOutlineColor(sf::Color::White);
	udpText.setStyle(sf::Text::Bold);

	//clear the window
	window.clear();

	window.draw(tcpText);
	window.draw(udpText);

	window.display();

	//check button click TCP
	if (sf::Mouse::isButtonPressed(sf::Mouse::Left))
	{
		if (sf::Mouse::getPosition(window).x >= tcpText.getPosition().x - 20 && sf::Mouse::getPosition(window).x <= tcpText.getPosition().x + 20)
		{
			if (sf::Mouse::getPosition(window).y >= tcpText.getPosition().y - 20 && sf::Mouse::getPosition(window).y <= tcpText.getPosition().y + 20)
			{
				cout << "Start TCP" << endl;
				TCPEnabled = true;
				state = Connecting;
			}
		}
	}

	//check button click UDP
	if (sf::Mouse::isButtonPressed(sf::Mouse::Left))
	{
		if (sf::Mouse::getPosition(window).x >= udpText.getPosition().x - 20 && sf::Mouse::getPosition(window).x <= udpText.getPosition().x + 20)
		{
			if (sf::Mouse::getPosition(window).y >= udpText.getPosition().y - 20 && sf::Mouse::getPosition(window).y <= udpText.getPosition().y + 20)
			{
				cout << "Start UDP P1" << endl;
				TCPEnabled = false;
				port = 54001;
				state = Connecting;
			}
		}
	}
}

void RunConnecting()
{
	//display waiting text
	//waiting
	sf::String connectingMsg = "Connecting..";
	sf::Text connectingText(connectingMsg, font, 50);
	connectingText.setOrigin(connectingText.getLocalBounds().width / 2, connectingText.getLocalBounds().height / 2);
	connectingText.setPosition(winSize.x / 2, (winSize.y / 2));
	connectingText.setFillColor(sf::Color::Red);
	connectingText.setOutlineThickness(1);
	connectingText.setOutlineColor(sf::Color::White);
	connectingText.setStyle(sf::Text::Bold);

	//clear the window
	window.clear();

	window.draw(connectingText);

	window.display();

	//waiting on all players
	if (!player1.isConnected)
	{
		if (TCPEnabled)
			TCP.ConnectToServer(&player1, &player2);

		else
			UDP.ConnectToServer(&player1, &player2, port);
	}
	
	//both players are connected, launch the game
	else
	{
		state = Game;
	}
}

void RunGame()
{
	//check endgame
	if (!player1.lives <= 0 && !player2.lives <= 0 && !TCP.endGame && !UDP.endGame)
	{
		//TCP
		if (TCPEnabled)
		{
			TCP.TCPRun(&player1, &player2, Input(), player1.hookThrow);
		}

		//UDP
		else
		{
			UDP.UDPRun(&player1, &player2, Input(), player1.hookThrow);

			//thread to constantly receive
			if (!recThreadLaunched)
			{
				threadRec.launch();
				recThreadLaunched = true;
			}
		}


		//Render
		Render();
	}

	//end the game
	else
	{
		//game over screen
		sf::String endMsg = "Game Over";

		sf::Text endText(endMsg, font, 30);
		endText.setOrigin(endText.getLocalBounds().width / 2, endText.getLocalBounds().height / 2);
		endText.setPosition(winSize.x / 2, winSize.y / 2);
		endText.setFillColor(sf::Color::Red);
		endText.setOutlineThickness(5);
		endText.setOutlineColor(sf::Color::White);
		endText.setStyle(sf::Text::Bold);

		//clear the window
		window.clear();

		window.draw(endText);

		window.display();
	}

}

//event main loop
int main()
{
	if (!font.loadFromFile("arial.ttf"))
	{
		cout << "text load error" << endl;
	}

	window.setFramerateLimit(60);

	while (window.isOpen())
	{
		sf::Event gameEvent;
		while (window.pollEvent(gameEvent))
		{
			switch (gameEvent.type)
			{
				//close the game
				case(sf::Event::Closed):
					window.close();
					break;
			}
		}

		switch (state)
		{
		case(Menu):
			RunMenu();
			break;

		case(Connecting):
			RunConnecting();
			break;

		case(Game):
			RunGame();
			break;
		}

	}

	return 0;
}