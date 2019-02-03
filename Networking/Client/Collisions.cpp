#include "Collisions.h"

Collisions::Collisions()
{
	holeTopLeft = sf::Vector2f(460, 280);
	holeBottomRight = sf::Vector2f(620, 440);

	holeShape.setPosition(holeTopLeft);
	holeShape.setSize(sf::Vector2f(holeBottomRight.x - holeTopLeft.x, holeBottomRight.y - holeTopLeft.y));
	holeShape.setOutlineColor(sf::Color::Red);
	holeShape.setOutlineThickness(-2);
	holeShape.setFillColor(sf::Color::White);

}

bool Collisions::TestFall(sf::Vector2f playerPos)
{
	//check if player has fallen into the hole
	if (playerPos.x >= holeTopLeft.x && playerPos.x <= holeBottomRight.x)
	{
		//check Y of fallen in
		if (playerPos.y >= holeTopLeft.y && playerPos.y <= holeBottomRight.y)
		{
			return true;
		}
	}

	//not fallen into the hole
	return false;
}

bool Collisions::TestHookHit(Player* player1, Player* player2)
{
	//check that current client is hooking
	if (player1->isHooking)
	{
		sf::Vector2f hookPos = player1->hookHead.getPosition();

		//hit right wall
		if (hookPos.x > boundaryX)
			return true;

		//hit left wall
		else if (hookPos.x < 0)
			return true;

		//VERTICAL
		//hit bottom wall
		if (hookPos.y > boundaryY)
			return true;

		//hit top wall
		else if (hookPos.y < 0)
			return true;

		//hitting another player
		//only check colliding with other players and not already been hooked
		if (!player2->gotHooked)
		{
			//check if current hook head hits another player
			if (hookPos.y <= player2->position.y + 20 && hookPos.y >= player2->position.y - 20 && hookPos.x <= player2->position.x + 20 && hookPos.x >= player2->position.x - 20)
			{
				cout << "Player " << player1->ID << " hooked "<< player2->ID << endl;

				sf::Packet colPack;

				//make a direction vector between the hooked player and the hooked player
				sf::Vector2f pullDir = sf::Vector2f(player1->position.x - player2->position.x, player1->position.y - player2->position.y);

				//normalize the hook direction vector
				float magni = sqrt(pullDir.x * pullDir.x + pullDir.y * pullDir.y);
				pullDir.x = pullDir.x / magni;
				pullDir.y = pullDir.y / magni;

				//tell the player it was hooked
				player2->gotHooked = true;
				//increase the speed to compensate for the hook
				player2->speed = 15;

				//stop the other player hooking if it is
				player2->ReleaseHook();

				//move the hooked player to where the hook head is
				player2->direction = pullDir;			

				return true;
			}
		}
		
	}

	return false;
}