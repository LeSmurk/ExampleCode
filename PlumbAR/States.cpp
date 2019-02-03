#include "States.h"
#include <graphics/sprite_renderer.h>
#include <graphics/texture.h>
#include <graphics/mesh.h>
#include <graphics/primitive.h>
#include <assets/png_loader.h>
#include <graphics/image_data.h>
#include <graphics/font.h>
#include <input/touch_input_manager.h>
#include <maths/vector2.h>
#include <input/input_manager.h>
#include <input/sony_controller_input_manager.h>
#include <maths/math_utils.h>
#include <graphics/scene.h>
#include <graphics/renderer_3d.h>
#include <graphics/render_target.h>
#include <time.h>


#include <sony_sample_framework.h>
#include <sony_tracking.h>


States::States()
{
}


States::~States()
{
}

void States::LoadTextures(gef::Platform* platform_)
{
	//get textures
	gef::ImageData imgDat;
	gef::PNGLoader pngLoader;
	gef::Texture* newTex;

	//background menu
	pngLoader.Load("menuBackground.png", *platform_, imgDat);
	newTex = gef::Texture::Create(*platform_, imgDat);
	//place in the middle
	menuBackground.set_position(gef::Vector4(480, 350, 0)); //480, 272
	menuBackground.set_width(1080);
	menuBackground.set_height(720);
	menuBackground.set_texture(newTex);

	//exit to menu sprite
	pngLoader.Load("exitButton.png", *platform_, imgDat);
	newTex = gef::Texture::Create(*platform_, imgDat);
	exitSprite.set_position(gef::Vector4(90, 50, 0));
	exitSprite.set_width(180);//270
	exitSprite.set_height(120);//180
	exitSprite.set_texture(newTex);

	//finished sprite
	pngLoader.Load("finished.png", *platform_, imgDat);
	newTex = gef::Texture::Create(*platform_, imgDat);
	finishedSprite.set_position(gef::Vector4(480, 90, 0));
	finishedSprite.set_width(270);
	finishedSprite.set_height(180);
	finishedSprite.set_texture(newTex);

	//controls sprite
	pngLoader.Load("createControls.png", *platform_, imgDat);
	newTex = gef::Texture::Create(*platform_, imgDat);
	controlsSprite.set_position(gef::Vector4(800, 450, 0));
	controlsSprite.set_width(270);
	controlsSprite.set_height(180);
	controlsSprite.set_texture(newTex);

	//confirm save sprite
	pngLoader.Load("confirmSave.png", *platform_, imgDat);
	newTex = gef::Texture::Create(*platform_, imgDat);
	saveLevelSprite.set_position(gef::Vector4(480, 350, 0));
	saveLevelSprite.set_width(405);
	saveLevelSprite.set_height(270);
	saveLevelSprite.set_texture(newTex);

	//highlight sprite
	pngLoader.Load("highlighter.png", *platform_, imgDat);
	newTex = gef::Texture::Create(*platform_, imgDat);
	highlightSprite.set_position(gef::Vector4(130, 350, 0));
	highlightSprite.set_width(85.3);
	highlightSprite.set_height(85.3);
	highlightSprite.set_texture(newTex);

	//level icon sprites
	for (int i = 0; i < 8; i++)
	{
		//This is disgusting and I hate that I did this
		const char* fileName;
		switch (i)
		{
			case(0):
				fileName = "levelIcon0.png";
				break;
			case(1):
				fileName = "levelIcon1.png";
				break;
			case(2):
				fileName = "levelIcon2.png";
				break;
			case(3):
				fileName = "levelIcon3.png";
				break;
			case(4):
				fileName = "levelIcon4.png";
				break;
			case(5):
				fileName = "levelIcon5.png";
				break;
			case(6):
				fileName = "levelIcon6.png";
				break;
			case(7):
				fileName = "levelIcon7.png";
				break;
		}
		


		pngLoader.Load(fileName, *platform_, imgDat);
		newTex = gef::Texture::Create(*platform_, imgDat);
		//add to sprites
		gef::Sprite tempSprite;
		tempSprite.set_position(gef::Vector4(130 + (i * 100), 350, 0));
		tempSprite.set_width(85.3);
		tempSprite.set_height(85.3);
		tempSprite.set_texture(newTex);

		//add to storage
		levelIconSprites.push_back(tempSprite);

	}
}


void States::InitState(LevelManager* lvlManager, PrimitiveBuilder* primitive_builder_)
{
	switch (lvlManager->currentLevel)
	{
		//menu
	case(-1):
		InitMenu(lvlManager, primitive_builder_);
		break;
		//lvl create
	case(0):
		InitLvlCreate(lvlManager, primitive_builder_);
		break;

		//Game any other num
	default:
		InitGame(lvlManager, primitive_builder_);
		break;
	}
}

void States::UpdateState(LevelManager* lvlManager, gef::InputManager* input_manager, PrimitiveBuilder* primitive_builder_)
{
	input_manager->Update();

	switch (lvlManager->currentLevel)
	{
		//menu
	case(-1):
		UpdateMenu(lvlManager, input_manager, primitive_builder_);
		break;
		//lvl create
	case(0):
		UpdateLvlCreate(lvlManager, input_manager, primitive_builder_);
		break;

		//Game any other num
	default:
		UpdateGame(lvlManager, input_manager, primitive_builder_);
		break;
	}
}

void States::Render2D(LevelManager* lvlManager, gef::SpriteRenderer* sprite_renderer_)
{
	switch (lvlManager->currentLevel)
	{
		//menu
	case(-1):
		RenderMenu2D(sprite_renderer_);
		break;
		//lvl create
	case(0):
		RenderLvlCreate2D(sprite_renderer_);
		break;

		//Game any other num
	default:
		RenderGame2D(sprite_renderer_);
		break;
	}
}

void States::Render3D(LevelManager* lvlManager, gef::Renderer3D* renderer_3d_, PrimitiveBuilder* primitive_builder_)
{
	switch (lvlManager->currentLevel)
	{
		//menu
	case(-1):
		RenderMenu3D(renderer_3d_, primitive_builder_);
		break;
		//lvl create
	case(0):
		RenderLvlCreate3D(renderer_3d_, primitive_builder_);
		break;

		//Game any other num
	default:
		RenderGame3D(renderer_3d_, primitive_builder_);
		break;
	}
}

//PRIVATE STATE RUNNING
void States::InitMenu(LevelManager* lvlManager, PrimitiveBuilder* primitive_builder_)
{
	timer = clock();
}

void States::UpdateMenu(LevelManager* lvlManager, gef::InputManager* input_manager, PrimitiveBuilder* primitive_builder_)
{
	//detect input
	if(input_manager->controller_input()->GetController(0)->buttons_pressed() & gef_SONY_CTRL_CROSS)
	{
		//start game to level selected
		lvlManager->ChangeLevel(currentlySelected);

		//init new state
		InitState(lvlManager, primitive_builder_);
	}

	//scrolling through selections
	if ((clock() - timer) * 10/CLOCKS_PER_SEC > scrollSpeed)
	{
		//scrolling through options
		if (input_manager->controller_input()->GetController(0)->left_stick_x_axis() > 0.5)
		{
			//scroll selection
			currentlySelected++;
			//prevent over scroll
			if (currentlySelected > maxSelection)
				currentlySelected = 0;

			//store current time
			timer = clock();
		}

		if (input_manager->controller_input()->GetController(0)->left_stick_x_axis() < -0.5)
		{
			//scroll selection
			currentlySelected--;
			//prevent under scroll
			if (currentlySelected < 0)
				currentlySelected = maxSelection;

			//store current time
			timer = clock();
		}
	}
	
}

void States::RenderMenu2D(gef::SpriteRenderer* sprite_renderer_)
{
	//background
	sprite_renderer_->DrawSprite(menuBackground);
	
	//items to highlight
	for (int i = 0; i < levelIconSprites.size(); i++)
	{
		////set to highlighted one
		//if (i == currentlySelected)
		//	levelIconSprites.at(i).set_colour(-10);
		//else
		//	levelIconSprites.at(i).set_colour(-1);

		//draw sprites
		sprite_renderer_->DrawSprite(levelIconSprites.at(i));
	}

	//draw highlighted sprite at position wanted
	highlightSprite.set_position(levelIconSprites.at(currentlySelected).position());
	sprite_renderer_->DrawSprite(highlightSprite);
	
}

void States::RenderMenu3D(gef::Renderer3D* renderer_3d_, PrimitiveBuilder* primitive_builder_)
{

}


//GAME
void States::InitGame(LevelManager* lvlManager, PrimitiveBuilder* primitive_builder_)
{
	//reset end level
	canEndLvl = false;
	savePrompt = false;

	//clear list of markers and cubes
	markers_list.clear();
	cube_object_list.clear();

	//create marker and cube objects
	for (int i = 0; i < 6; i++)
	{
		//create markers
		MarkerHandler* newMarker = new MarkerHandler;
		newMarker->InitMarker(i);
		markers_list.push_back(newMarker);

		//get first and last parts
		if (i == 0)
		{
			//START PART
			GameObject* newCube = new GameObject;
			//init object
			newCube->InitObject(lvlManager->GetLevelPipeMesh(0), primitive_builder_->GetDefaultSphereMesh(), true, false, lvlManager->GetPieceNum(0));

			//set relative positions
			gef::Vector4 relPos = gef::Vector4(0, 0, 0);
			newCube->SetLocalRelative(relPos);

			//add to list
			cube_object_list.push_back(newCube);

			////////////////////////////////////////////////////
			//END PART
			GameObject* new2Cube = new GameObject;
			//init object
			new2Cube->InitObject(lvlManager->GetLevelPipeMesh(1), primitive_builder_->GetDefaultSphereMesh(), false, true, lvlManager->GetPieceNum(1));

			//set relative positions
			gef::Vector4 rel2Pos = lvlManager->GetSeparationDistance();
			new2Cube->SetLocalRelative(rel2Pos);

			//add to list
			cube_object_list.push_back(new2Cube);

		}

		//Get rest of parts
		else
		{
			GameObject* newCube = new GameObject;
			//init object
			newCube->InitObject(lvlManager->GetLevelPipeMesh(i + 1), primitive_builder_->GetDefaultSphereMesh(), false, false, lvlManager->GetPieceNum(i + 1));

			//set relative positions
			gef::Vector4 relPos = gef::Vector4(0, 0, 0);
			newCube->SetLocalRelative(relPos);

			//add to list
			cube_object_list.push_back(newCube);
		}
	}
}

void States::UpdateGame(LevelManager* lvlManager, gef::InputManager* input_manager, PrimitiveBuilder* primitive_builder_)
{
	//Show finished level
	if (canEndLvl)
	{
		//detect button click to exit level
		if (input_manager->controller_input()->GetController(0)->buttons_pressed() & gef_SONY_CTRL_CROSS)
		{
			//if creating level, only change if saving (but stupid to check level again in here)
			if (lvlManager->currentLevel == 0 )
			{
				if (savePrompt)
				{
					//STORE IN LEVEL FILE
					std::vector<GameObject::PipeType> pipes;
					for (int i = 0; i < cube_object_list.size(); i++)
					{
						pipes.push_back(cube_object_list.at(i)->thisPipe);
					}
					lvlManager->CreateLevel(cube_object_list.at(1)->relPosition, pipes);

					//back to menu
					lvlManager->ChangeLevel(-1);

					//init new state
					InitState(lvlManager, primitive_builder_);
				}
			}

			//normal level
			else
			{
				//back to menu
				lvlManager->ChangeLevel(-1);
			
				//init new state
				InitState(lvlManager, primitive_builder_);
			}
		}
	}

	//Exit to menu
	if (input_manager->controller_input()->GetController(0)->buttons_pressed() & gef_SONY_CTRL_START)
	{
		//back to menu
		lvlManager->ChangeLevel(-1);

		//init new state
		InitState(lvlManager, primitive_builder_);
	}

	AppData* dat = sampleUpdateBegin();

	// use the tracking library to try and find markers
	smartUpdate(dat->currentImage);

	//loop through all the markers and place cubes at each
	for (int i = 0; i < 6; i++)
	{
		bool atMarker = false;

		//found the marker
		if (sampleIsMarkerFound(i))
		{
			//tell objects at marker
			atMarker = true;

			//set marker transform
			sampleGetTransform(i, &markers_list.at(i)->markerMatrix);

			//set to last known
			markers_list.at(i)->lastKnownMatrix = markers_list.at(i)->markerMatrix;

			////set the markers relative position to all the other seen markers
			//for (int x = 0; x < 6; x++)
			//{
			//	//seen another marker, making sure not using itself
			//	if (sampleIsMarkerFound(x) && x != i)
			//	{
			//		//markers_list.at(i)->SetRelativeMarkerPosition(x, markers_list.at(x)->markerMatrix.GetTranslation());
			//		markers_list.at(i)->SetRelative(x, markers_list.at(x)->markerMatrix);
			//	}
			//}

		}

		//if first marker, set first and final pieces
		if (i == 0)
		{
			cube_object_list.at(i)->SetTransform(markers_list.at(i)->markerMatrix, atMarker);
			cube_object_list.at(i + 1)->SetTransform(markers_list.at(i)->markerMatrix, atMarker);
		}
		//otherwise, only one at each marker
		else
		{
			cube_object_list.at(i + 1)->SetTransform(markers_list.at(i)->markerMatrix, atMarker);
		}

	}

	//test collisions
	for (int z = 0; z < cube_object_list.size(); z++)
	{
		//only compare active items
		if (cube_object_list.at(z)->activeInScene)
		{
			//determine if connected to a green
			int gotConnection = -1;

			//compare with every other collider
			for (int c = 0; c < cube_object_list.size(); c++)
			{
				//don't compare with itself and make sure active in scene
				if (z != c && cube_object_list.at(c)->activeInScene)
				{
					//check collision
					if (BoxCollision(cube_object_list.at(z), cube_object_list.at(c)))
					{
						//collision detected with a green and it isn't already connected to this piece (prevents circular connections)
						if (cube_object_list.at(c)->connected != -1 && cube_object_list.at(c)->connected != z)
							gotConnection = c;
					}
				}
			}

			//set to gren or red connection and test if end piece
			if (cube_object_list.at(z)->SetConnectedCollider(gotConnection))
			{
				//end level
				if (gotConnection != -1)
					canEndLvl = true;
				//end piece disconnected
				else
					canEndLvl = false;
			}
		}
	}

	sampleUpdateEnd(dat);
}

void States::RenderGame2D(gef::SpriteRenderer* sprite_renderer_)
{
	//show menu bnutton
	sprite_renderer_->DrawSprite(exitSprite);

	//can end level
	if (canEndLvl)
		sprite_renderer_->DrawSprite(finishedSprite);
}

void States::RenderGame3D(gef::Renderer3D* renderer_3d_, PrimitiveBuilder* primitive_builder_)
{
	for (int i = 0; i < cube_object_list.size(); i++)
	{
		//only draw if active object
		if (cube_object_list.at(i)->activeInScene)
		{
			//draw colour
			if (cube_object_list.at(i)->connected != -1)
				renderer_3d_->set_override_material(&primitive_builder_->green_material());
			else
				renderer_3d_->set_override_material(&primitive_builder_->red_material());

			//draw object
			renderer_3d_->DrawMesh(*cube_object_list.at(i));

			//collider green colour
			renderer_3d_->set_override_material(&primitive_builder_->blue_material());
			//rendering colliders
			for (int x = 0; x < cube_object_list.at(i)->colliders.size(); x++)
				renderer_3d_->DrawMesh(*cube_object_list.at(i)->colliders.at(x)->colliderMesh);
		}
	}
}

//LVL CREATOR
void States::InitLvlCreate(LevelManager* lvlManager, PrimitiveBuilder* primitive_builder_)
{
	//start at first highlighted block
	highlightedBlock = 2;

	timer = clock();

	//do regular game init
	InitGame(lvlManager, primitive_builder_);
}

void States::UpdateLvlCreate(LevelManager* lvlManager, gef::InputManager* input_manager, PrimitiveBuilder* primitive_builder_)
{
	//Changing level only allowed if not trying to save
	if (!savePrompt)
	{
		//detect button to change highlighted piece
		if (input_manager->controller_input()->GetController(0)->buttons_pressed() & gef_SONY_CTRL_SQUARE)
		{
			//cancelling save level
			if (savePrompt)
				savePrompt = false;

			//swapping highlighted piece
			else
			{
				highlightedBlock++;
				//loop round highlighted block
				if (highlightedBlock >= cube_object_list.size())
					highlightedBlock = 2;
			}

		}

		//Ref to highlighted block
		GameObject* selectedBlock = cube_object_list.at(highlightedBlock);

		//CHANGE PIECE TYPE
		if (input_manager->controller_input()->GetController(0)->buttons_pressed() & gef_SONY_CTRL_CIRCLE)
		{
			//next type of pipe
			GameObject::PipeType nextType = selectedBlock->thisPipe;
			switch (nextType)
			{
				case(GameObject::PipeType::junction):
					nextType = GameObject::PipeType::bend;
					break;
				case(GameObject::PipeType::bend):
					nextType = GameObject::PipeType::line;
					break;
				case(GameObject::PipeType::line):
					nextType = GameObject::PipeType::tetris;
					break;
				case(GameObject::PipeType::tetris):
					nextType = GameObject::PipeType::junction;
					break;
			}

			//reinitialise the objects to new types
			selectedBlock->InitObject(lvlManager->GetMeshType(nextType), primitive_builder_->GetDefaultSphereMesh(), false, false, nextType);
		}

		/////////////////////////////////////////////////////////////////////////////
		//CHANGE SEPARATION DIST
		GameObject * finalBlock = cube_object_list.at(1);

		if ((clock() - timer) * 10 / CLOCKS_PER_SEC > scrollSpeed)
		{
			if (input_manager->controller_input()->GetController(0)->right_stick_x_axis() > 0.5)
			{
				//set relative positions
				gef::Vector4 rel2Pos = finalBlock->relPosition + gef::Vector4(1, 0, 0);
				finalBlock->SetLocalRelative(rel2Pos);
				timer = clock();
			}
			//detect sticks to move separation distance
			else if (input_manager->controller_input()->GetController(0)->right_stick_x_axis() < -0.5)
			{
				//set relative positions
				gef::Vector4 rel2Pos = finalBlock->relPosition + gef::Vector4(-1, 0, 0);
				finalBlock->SetLocalRelative(rel2Pos);
				timer = clock();

			}
			//detect sticks to move separation distance
			else if (input_manager->controller_input()->GetController(0)->right_stick_y_axis() > 0.5)
			{
				//set relative positions
				gef::Vector4 rel2Pos = finalBlock->relPosition + gef::Vector4(0, -1, 0);
				finalBlock->SetLocalRelative(rel2Pos);
				timer = clock();

			}
			//detect sticks to move separation distance
			else if (input_manager->controller_input()->GetController(0)->right_stick_y_axis() < -0.5)
			{
				//set relative positions
				gef::Vector4 rel2Pos = finalBlock->relPosition + gef::Vector4(0, 1, 0);
				finalBlock->SetLocalRelative(rel2Pos);
				timer = clock();

			}
		}
	}

	
	/////////////////////////////////////////////////////////////////
	//select button to save level (if all green)
	if (input_manager->controller_input()->GetController(0)->buttons_pressed() & gef_SONY_CTRL_SELECT && canEndLvl)
	{
		//confirm save level
		savePrompt = !savePrompt;
		//store all part ty		//store separation dist
	}

	//do regular game update now
	UpdateGame(lvlManager, input_manager, primitive_builder_);
}

void States::RenderLvlCreate2D(gef::SpriteRenderer* sprite_renderer_)
{
	//show menu bnutton
	sprite_renderer_->DrawSprite(exitSprite);

	//show controls
	sprite_renderer_->DrawSprite(controlsSprite);

	//confirm save level
	if(savePrompt)
		sprite_renderer_->DrawSprite(saveLevelSprite);
}

void States::RenderLvlCreate3D(gef::Renderer3D* renderer_3d_, PrimitiveBuilder* primitive_builder_)
{
	//render the same as the game, with blue being the highlighted block
	for (int i = 0; i < cube_object_list.size(); i++)
	{
		//only draw if active object
		if (cube_object_list.at(i)->activeInScene)
		{
			//draw colour
			if (cube_object_list.at(i)->connected != -1)
				renderer_3d_->set_override_material(&primitive_builder_->green_material());
			else
				renderer_3d_->set_override_material(&primitive_builder_->red_material());

			//override this if highlighted for editing
			if(i == highlightedBlock)
				renderer_3d_->set_override_material(&primitive_builder_->blue_material());

			//draw object
			renderer_3d_->DrawMesh(*cube_object_list.at(i));

			//collider green colour
			renderer_3d_->set_override_material(&primitive_builder_->blue_material());
			//rendering colliders
			for (int x = 0; x < cube_object_list.at(i)->colliders.size(); x++)
				renderer_3d_->DrawMesh(*cube_object_list.at(i)->colliders.at(x)->colliderMesh);
		}
	}
}

//COLLISIONS
bool States::BoxCollision(GameObject* obj1, GameObject* obj2)
{
	//compare all the colliders of the two objects
	for (int i = 0; i < obj1->colliders.size(); i++)
	{
		for (int z = 0; z < obj2->colliders.size(); z++)
		{
			//change aabb by transform matrix of instance
			gef::Aabb col1;
			gef::Aabb col2;
			col1 = obj1->colliders.at(i)->colliderMesh->mesh()->aabb().Transform(obj1->colliders.at(i)->colliderMesh->transform());
			col2 = obj2->colliders.at(z)->colliderMesh->mesh()->aabb().Transform(obj2->colliders.at(z)->colliderMesh->transform());

			//check x within bounds
			if (col1.max_vtx().x() >= col2.min_vtx().x() && col1.min_vtx().x() <= col2.max_vtx().x())
			{
				//check y within
				if (col1.max_vtx().y() >= col2.min_vtx().y() && col1.min_vtx().y() <= col2.max_vtx().y())
				{
					//check z
					if (col1.max_vtx().z() >= col2.min_vtx().z() && col1.min_vtx().z() <= col2.max_vtx().z())
					{
						return true;
					}
				}
			}
		}
	}

	return false;
}