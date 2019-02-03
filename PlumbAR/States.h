#pragma once
#include <system/application.h>
#include <graphics/sprite.h>
#include <maths/vector2.h>
#include <vector>
#include <graphics/mesh_instance.h>
#include <platform/vita/graphics/texture_vita.h>
#include "primitive_builder.h"
#include "GameObject.h"
#include "MarkerHandler.h"
#include "LevelManager.h"
#include "graphics\sprite_renderer.h"
#include "graphics\renderer_3d.h"
#include "input\input_manager.h"
#include "system\platform.h"
#include <iostream>
#include <ctime>
#include <time.h>
#include <chrono>

class States
{
public:
	States();
	~States();

	void LoadTextures(gef::Platform* platform);

	void InitState(LevelManager*, PrimitiveBuilder*);

	void UpdateState(LevelManager*, gef::InputManager*, PrimitiveBuilder*);

	void Render2D(LevelManager*, gef::SpriteRenderer*);

	void Render3D(LevelManager*, gef::Renderer3D*, PrimitiveBuilder*);

private:

	//for detecting what is currently selected
	int currentlySelected = 1;
	int maxSelection = 7;
	//selection scrolling
	float scrollSpeed = 0.5f;
	clock_t timer;

	//Since not many states that differ, doing them all here

	//MENU
	void InitMenu(LevelManager*, PrimitiveBuilder*);
	void UpdateMenu(LevelManager* lvlManager, gef::InputManager*, PrimitiveBuilder*);
	void RenderMenu2D(gef::SpriteRenderer*);
	void RenderMenu3D(gef::Renderer3D*, PrimitiveBuilder*);

	//main menu sprites
	gef::Sprite menuBackground;

	std::vector<gef::Sprite> levelIconSprites;
	gef::Sprite highlightSprite;


	////////////////////////////////////////////////////
	//GAME
	void InitGame(LevelManager*, PrimitiveBuilder*);
	void UpdateGame(LevelManager* lvlManager, gef::InputManager*, PrimitiveBuilder*);
	void RenderGame2D(gef::SpriteRenderer*);
	void RenderGame3D(gef::Renderer3D*, PrimitiveBuilder*);

	bool canEndLvl = false;

	//Sprites
	gef::Sprite finishedSprite;
	gef::Sprite exitSprite;

	//markers
	std::vector<MarkerHandler*> markers_list;
	//objects
	std::vector<GameObject*> cube_object_list;


	////////////////////////////////////////////////////
	//LVL CREATION
	void InitLvlCreate(LevelManager*, PrimitiveBuilder*);
	void UpdateLvlCreate(LevelManager* lvlManager, gef::InputManager*, PrimitiveBuilder*);
	void RenderLvlCreate2D(gef::SpriteRenderer*);
	void RenderLvlCreate3D(gef::Renderer3D*, PrimitiveBuilder*);

	//highlighted puzzle piece
	int highlightedBlock = 2;

	//Sprites
	gef::Sprite controlsSprite;
	gef::Sprite saveLevelSprite;

	bool savePrompt = false;


	///////////////////////////////////////////////////
	//COLLISION
	bool BoxCollision(GameObject*, GameObject*);
};

