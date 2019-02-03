#include "LevelManager.h"

LevelManager::LevelManager()
{
	GenerateLevel();
}


LevelManager::~LevelManager()
{
}

void LevelManager::CreateLevel(gef::Vector4 separation, std::vector<GameObject::PipeType> pipes)
{
	//overwrite the last level
	if (levels.size() == 8)
	{
		levelSeparations.at(7) = separation;

		levels.at(7) = pipes;
	}

	else
	{
		levelSeparations.push_back(separation);

		levels.push_back(pipes);
	}

}

void LevelManager::GenerateLevel()
{
	//LEVEL 0
	std::vector<GameObject::PipeType> level;
	//first piece
	level.push_back(GameObject::PipeType::junction);
	//last piece
	level.push_back(GameObject::PipeType::junction);
	//puzzle pieces
	level.push_back(GameObject::PipeType::junction);
	level.push_back(GameObject::PipeType::junction);
	level.push_back(GameObject::PipeType::junction);
	level.push_back(GameObject::PipeType::junction);
	level.push_back(GameObject::PipeType::junction);

	CreateLevel(gef::Vector4(2, 0, 0), level);

	//LEVEL 1
	level.clear();
	//first piece
	level.push_back(GameObject::PipeType::junction);
	//last piece
	level.push_back(GameObject::PipeType::junction);
	//puzzle pieces
	level.push_back(GameObject::PipeType::junction);
	level.push_back(GameObject::PipeType::line);
	level.push_back(GameObject::PipeType::junction);
	level.push_back(GameObject::PipeType::line);
	level.push_back(GameObject::PipeType::junction);

	CreateLevel(gef::Vector4(4, -1, 0), level);

	//LEVEL 2
	level.clear();
	//first piece
	level.push_back(GameObject::PipeType::junction);
	//last piece
	level.push_back(GameObject::PipeType::junction);
	//puzzle pieces
	level.push_back(GameObject::PipeType::line);
	level.push_back(GameObject::PipeType::tetris);
	level.push_back(GameObject::PipeType::tetris);
	level.push_back(GameObject::PipeType::line);
	level.push_back(GameObject::PipeType::tetris);

	CreateLevel(gef::Vector4(2, 3, 0), level);

	//LEVEL 3
	level.clear();
	//first piece
	level.push_back(GameObject::PipeType::junction);
	//last piece
	level.push_back(GameObject::PipeType::junction);
	//puzzle pieces
	level.push_back(GameObject::PipeType::bend);
	level.push_back(GameObject::PipeType::tetris);
	level.push_back(GameObject::PipeType::tetris);
	level.push_back(GameObject::PipeType::junction);
	level.push_back(GameObject::PipeType::bend);

	CreateLevel(gef::Vector4(2, -2, 0), level);

	//LEVEL 4
	level.clear();
	//first piece
	level.push_back(GameObject::PipeType::junction);
	//last piece
	level.push_back(GameObject::PipeType::junction);
	//puzzle pieces
	level.push_back(GameObject::PipeType::line);
	level.push_back(GameObject::PipeType::bend);
	level.push_back(GameObject::PipeType::bend);
	level.push_back(GameObject::PipeType::tetris);
	level.push_back(GameObject::PipeType::bend);

	CreateLevel(gef::Vector4(0, -4, 0), level);

	//LEVEL 5
	level.clear();
	//first piece
	level.push_back(GameObject::PipeType::line);
	//last piece
	level.push_back(GameObject::PipeType::junction);
	//puzzle pieces
	level.push_back(GameObject::PipeType::bend);
	level.push_back(GameObject::PipeType::bend);
	level.push_back(GameObject::PipeType::tetris);
	level.push_back(GameObject::PipeType::bend);
	level.push_back(GameObject::PipeType::tetris);

	CreateLevel(gef::Vector4(2, -3, 0), level);

	//LEVEL 6
	level.clear();
	//first piece
	level.push_back(GameObject::PipeType::bend);
	//last piece
	level.push_back(GameObject::PipeType::junction);
	//puzzle pieces
	level.push_back(GameObject::PipeType::line);
	level.push_back(GameObject::PipeType::line);
	level.push_back(GameObject::PipeType::bend);
	level.push_back(GameObject::PipeType::tetris);
	level.push_back(GameObject::PipeType::bend);

	CreateLevel(gef::Vector4(4, 2, 0), level);
}

//GameObject::PipeType LevelManager::GetStartingPiece()
//{
//	//read from level file
//	return levels.at(currentLevel).at(0);
//}
//
//GameObject::PipeType LevelManager::GetEndingPiece()
//{
//	//read from level file
//	return levels.at(currentLevel).at(1);
//}

gef::Vector4 LevelManager::GetSeparationDistance()
{
	return levelSeparations.at(currentLevel);
}

GameObject::PipeType LevelManager::GetPieceNum(int num)
{
	//read from level file
	return levels.at(currentLevel).at(num);;
}

//Changing levels
void LevelManager::ChangeLevel(int newLvl)
{
	int wantedLvl = newLvl;

	//if not accessible level, make current level equal menu
	if (newLvl >= levels.size())
		wantedLvl = -1;

	//update current level to new one
	currentLevel = wantedLvl;
	
}

//get mesh wanted
gef::Mesh* LevelManager::GetLevelPipeMesh(int num)
{
	//pipe type
	gef::Mesh* wantedMesh;

	wantedMesh = GetMeshType(levels.at(currentLevel).at(num));

	return wantedMesh;
}

gef::Mesh* LevelManager::GetMeshType(GameObject::PipeType type)
{
	//pipe type
	gef::Mesh* wantedMesh;
	switch (type)
	{
		case(GameObject::PipeType::junction):
			wantedMesh = meshJunction;
			break;
		case(GameObject::PipeType::bend):
			wantedMesh = meshBend;
			break;
		case(GameObject::PipeType::line):
			wantedMesh = meshStraight;
			break;
		case(GameObject::PipeType::tetris):
			wantedMesh = meshTetris;
			break;
	}

	return wantedMesh;
}


