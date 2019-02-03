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

bool Collisions::TestHookHit(int currentNum, vector<ClientHolder*>* clientList)
{
	//check that current client is hooking
	if (clientList->at(currentNum)->isHooking)
	{
		sf::Vector2f hookPos = clientList->at(currentNum)->hookHead.getPosition();

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
		for (int i = 0; i < clientList->size(); i++)
		{
			//only check colliding with other players and not already been hooked
			if (i != currentNum && !clientList->at(i)->gotHooked)
			{
				//check if current hook head hits another player
				if (hookPos.y <= clientList->at(i)->position.y + 20 && hookPos.y >= clientList->at(i)->position.y - 20 && hookPos.x <= clientList->at(i)->position.x + 20 && hookPos.x >= clientList->at(i)->position.x - 20)
				{
					cout << currentNum << " hooked player " << i << endl;

					sf::Packet colPack;

					//make a direction vector between the hooked player and the hooked player
					sf::Vector2f pullDir = sf::Vector2f(clientList->at(currentNum)->position.x - clientList->at(i)->position.x, clientList->at(currentNum)->position.y - clientList->at(i)->position.y);

					//normalize the hook direction vector
					float magni = sqrt(pullDir.x * pullDir.x + pullDir.y * pullDir.y);
					pullDir.x = pullDir.x / magni;
					pullDir.y = pullDir.y / magni;

					//tell the player it was hooked
					clientList->at(i)->gotHooked = true;
					//increase the speed to compensate for the hook
					clientList->at(i)->speed = 15;

					//stop the other player hooking if it is
					clientList->at(i)->ReleaseHook();

					//move the hooked player to where the hook head is
					clientList->at(i)->direction = pullDir;

					//new message
					clientList->at(i)->num++;

					//tell to move to direction of hook origin (other player)
					colPack << clientList->at(i)->ID << clientList->at(i)->num << true << pullDir.x << pullDir.y << -1;

					//send the message to the clients
					for (int z = 0; z < clientList->size(); z++)
					{
						clientList->at(z)->tcpSock.send(colPack);
					}
					

					return true;
				}
			}
		}
	}

	return false;
}