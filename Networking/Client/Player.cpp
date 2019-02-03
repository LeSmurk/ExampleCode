#include "Player.h"


Player::Player(int p)
{
	//says if this is player the client controls
	player1 = p;

	//setup position
	position = sf::Vector2f(0.0f, 0.0f);
	//setup render stuff
	shape = sf::CircleShape(playerSize);

	shape.setPosition(position);
	
}

void Player::TCPUpdate()
{
	//only move if not hooking
	if (!isHooking)
	{
		//update pos if valid
		position = TestValid(sf::Vector2f(position.x += direction.x * speed, position.y += direction.y* speed));

		//set the shape to the new position
		shape.setPosition(position);
	}
	//hooking
	else
		UpdateHook();
}


void Player::PosUpdate(sf::Vector2f newPos)
{
	//update position
	position = TestValid(newPos);

	//set the shape to the new position
	shape.setPosition(position);
}

//lerp between the predicted position and the actual position
sf::Vector2f Lerp(sf::Vector2f start, sf::Vector2f end, float percentage)
{
	return (start + (percentage / 100)*(end - start));
}

void Player::UDPUpdate()
{
	//only move if not hooking
	if (!isHooking)
	{
		if (player1 == 1)
		{
			position = TestValid(sf::Vector2f(position.x += direction.x * speed, position.y += direction.y* speed));

			//set the shape to the new position
			shape.setPosition(position);
		}

		//not this client's player
		else
		{
			//timer to lerp properly
			sf::Time timer = lerpTimer.getElapsedTime();
			int time = (100 * timer.asMilliseconds() / 30); + offsetTime;
			if (time > 100)
				time = 100;

			//update direction based on if pos is valid and the values of lerp wanted
			position = Lerp(TestValid(sf::Vector2f(position.x += direction.x * speed, position.y += direction.y* speed)), lerpPos, time);

			//set the shape to the new position
			shape.setPosition(position);
		}
	
	}
	//hooking
	else
		UpdateHook();
}

//prediction and interpolation
void Player::CheckPrediction(float x, float y)
{
	//check if the new positions and interpolate then predict
	lerpPos.x = x;
	lerpPos.y = y;

	//linear prediction
	//sets the direction based on what the last position was and this new one
	//only if new pos isn't same as the old one
	if (prevPos != sf::Vector2f(x, y))
	{
		direction = sf::Vector2f(x - prevPos.x, y - prevPos.y);

		//normalize
		float magni = sqrt(direction.x * direction.x + direction.y * direction.y);
		direction.x = direction.x / magni;
		direction.y = direction.y / magni;
	}

	else
		direction = sf::Vector2f(0, 0);

	//store the given position
	prevPos = sf::Vector2f(x, y);

	//timer to move between start adn final of lerp
	lerpTimer.restart();
}

sf::Vector2f Player::InitHook(float x, float y)
{
	//make sure not already throwing (for any UDP issues)
	if (!isHooking)
	{
		//toggle the throw on server
		hookThrow = true;

		//make a direction vector between the player and the mouse pos
		hookDir = sf::Vector2f(x - position.x, y - position.y);

		//normalize the hook direction vector
		float magni = sqrt(hookDir.x * hookDir.x + hookDir.y * hookDir.y);
		hookDir.x = hookDir.x / magni;
		hookDir.y = hookDir.y / magni;

		//set the base position to the player
		hookShape.setPosition(position);

		//toggle hooking
		isHooking = true;

		//initialise the hook head
		hookHead = sf::CircleShape(10);
		hookHead.setOrigin(hookHead.getRadius(), hookHead.getRadius());
		hookHead.setFillColor(sf::Color::Magenta);
		hookHead.setPosition(position + direction);

		//convert unit vector to angle
		float angle = (atan2(hookDir.y, hookDir.x)) * (180 / 3.141592);
		//rotate the hook to the correct point
		hookShape.setRotation(angle);

		//start hook timer
		hookTimer.restart();
	}

	return sf::Vector2f(x, y);
}

void Player::UpdateHook()
{
	//stop the hook if it has been alive for too long
	sf::Time timer = hookTimer.getElapsedTime();
	if (timer.asSeconds() >= 2)
		ReleaseHook();


	//check if retracting or sending
	if (!retractHook)
	{
		//only recalculate angels if not this player and using tcp
		if (player1 != 1 && !TCPMode)
		{
			//re calculate angles
			//make a direction vector between the player and the mouse pos
			hookDir = sf::Vector2f(hookHead.getPosition().x - position.x, hookHead.getPosition().y - position.y);

			//normalize the hook direction vector
			float magni = sqrt(hookDir.x * hookDir.x + hookDir.y * hookDir.y);
			hookDir.x = hookDir.x / magni;
			hookDir.y = hookDir.y / magni;

			//convert unit vector to angle
			float angle = (atan2(hookDir.y, hookDir.x)) * (180 / 3.141592);
			//rotate the hook to the correct point
			hookShape.setRotation(angle);
		}

		//make shape expand to point
		hookShape.setSize(sf::Vector2f(hookShape.getSize().x + hookSpeed, 2));

		//head position
		hookHead.setPosition(hookHead.getPosition().x + hookDir.x * hookSpeed, hookHead.getPosition().y + hookDir.y * hookSpeed);

	}

	//retracting
	else
	{
		//make shape expand to point
		hookShape.setSize(sf::Vector2f(hookShape.getSize().x - hookSpeed, 2));

		//head position
		hookHead.setPosition(hookHead.getPosition().x - hookDir.x * hookSpeed, hookHead.getPosition().y - hookDir.y * hookSpeed);

		//stop it at the end of the retract
		if (hookHead.getPosition().y <= position.y + 5 && hookHead.getPosition().y >= position.y - 5)
			ReleaseHook();
	}
}

void Player::ReleaseHook()
{
	isHooking = false;
	retractHook = false;

	hookShape.setSize(sf::Vector2f(0, 0));
	hookHead.setRadius(0);
}

sf::Vector2f Player::TestValid(sf::Vector2f pos)
{
	//return position
	sf::Vector2f returnPos = pos;

	//HORIZONTAl
	if (pos.x > boundaryX - playerSize * 2)
	{
		//too far right
		returnPos.x = boundaryX - playerSize * 2;
	}

	else if (pos.x < 0)
	{
		//too far left
		returnPos.x = 0;
	}

	//VERTICAL
	if (pos.y > boundaryY - playerSize * 2)
	{
		//too far down
		returnPos.y = boundaryY - playerSize * 2;
	}

	else if (pos.y < 0)
	{
		//too far up
		returnPos.y = 0;
	}

	//set boundaries for player number
	if (ID == 1)
	{
		if (pos.y > (boundaryY / 2) - playerSize * 2)
		{
			returnPos.y = boundaryY / 2 - playerSize * 2;

			//toggle off hooked
			gotHooked = false;
			speed = 5;
			direction = sf::Vector2f(0, 0);
		}
	}

	else if (ID == 2)
	{
		if (pos.y < (boundaryY / 2))
		{
			returnPos.y = boundaryY / 2;

			//toggle off hooked
			gotHooked = false;
			speed = 5;
			direction = sf::Vector2f(0, 0);
		}
	}

	//return the fixed position
	return returnPos;
}
