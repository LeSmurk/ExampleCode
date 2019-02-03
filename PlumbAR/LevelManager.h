#pragma once
#include <maths/vector2.h>
#include <maths/vector4.h>
#include <maths/matrix44.h>
#include <vector>
#include "GameObject.h"
#include <fstream>
#include <iostream>
#include <string>

class LevelManager
{
public:
	LevelManager();
	~LevelManager();

	gef::Vector4 GetSeparationDistance();

	GameObject::PipeType GetPieceNum(int);

	gef::Mesh* GetLevelPipeMesh(int);

	gef::Mesh* GetMeshType(GameObject::PipeType);

	//Level changing
	void ChangeLevel(int newLevel);


	////number of levels stored
	//int totalLevels = 1;

	//current level on
	int currentLevel = 0;

	//HANDLE MODELS TOO
	class gef::Mesh* meshJunction;
	class gef::Mesh* meshBend;
	class gef::Mesh* meshStraight;
	class gef::Mesh* meshTetris;

	void CreateLevel(gef::Vector4, std::vector<GameObject::PipeType>);


private:

	void GenerateLevel();


	//levels
	std::vector<std::vector<GameObject::PipeType>> levels;
	std::vector<gef::Vector4> levelSeparations;


};

