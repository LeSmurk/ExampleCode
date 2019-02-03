#include "scene_app.h"
#include <system/platform.h>
#include <graphics/sprite_renderer.h>
#include <graphics/texture.h>
#include <graphics/mesh.h>
#include <graphics/primitive.h>
#include <assets/png_loader.h>
#include <graphics/image_data.h>
#include <graphics/font.h>
#include <system/debug_log.h>
#include <input/input_manager.h>
#include <input/touch_input_manager.h>
#include <maths/vector2.h>
#include <input/sony_controller_input_manager.h>
#include <graphics/renderer_3d.h>
#include <maths/math_utils.h>


SceneApp::SceneApp(gef::Platform& platform) :
	Application(platform),
	sprite_renderer_(NULL),
	input_manager_(NULL),
	renderer_3d_(NULL),
	primitive_builder_(NULL),
	world_(NULL),
	font_(NULL),
	audio_manager_(NULL)
{
}

void SceneApp::Init()
{
	//start at menu
	MenuInit();
}

void SceneApp::CleanUp()
{
	CleanUpFont();

	delete world_;
	world_ = NULL;

	//input manager
	delete input_manager_;
	input_manager_ = NULL;

	delete primitive_builder_;
	primitive_builder_ = NULL;

	delete renderer_3d_;
	renderer_3d_ = NULL;

	delete sprite_renderer_;
	sprite_renderer_ = NULL;

	delete audio_manager_;
	audio_manager_ = NULL;

	//unload ALL audio samples
}

bool SceneApp::Update(float frame_time)
{
	fps_ = 1.0f / frame_time;
		
	switch (gameState)
	{
	case(MENU) :
		MenuUpdate();
		break;

	case(GAME):
		GameUpdate();
		break;
	}

	return true;
}

void SceneApp::Render()
{
	//projections
	gef::Matrix44 projection_matrix;
	float fov = gef::DegToRad(45.0f);
	float aspect_ratio = (float)platform_.width() / (float)platform_.height();
	projection_matrix = platform_.PerspectiveProjectionFov(fov, aspect_ratio, 0.1f, 100.0f);
	renderer_3d_->set_projection_matrix(projection_matrix);

	// Camera View ///////////////////////////////////////////////
	//view
	gef::Matrix44 view_matrix;
	gef::Vector4 camera_eye(0.0f, 0.0f, 5.0f);
	gef::Vector4 camera_lookat(0.0f, 0.0f, 0.0f);
	gef::Vector4 camera_up(0.0f, 1.0f, 0.0f);
	view_matrix.LookAt(camera_eye, camera_lookat, camera_up);
	renderer_3d_->set_view_matrix(view_matrix);

	switch (gameState)
	{
	case(MENU) :
		MenuRender();
		break;

	case(GAME) :
		GameRender();
		break;
	}
}

void SceneApp::InitFont()
{
	font_ = new gef::Font(platform_);
	font_->Load("comic_sans");
}

void SceneApp::CleanUpFont()
{
	delete font_;
	font_ = NULL;
}

void SceneApp::DrawHUD()
{
	if(font_)
	{
		// display frame rate
		font_->RenderText(
			sprite_renderer_,						// sprite renderer to draw the letters
			gef::Vector4(500.0f, 500.0f, -0.9f),	// position on screen
			1.0f,									// scale
			0xFF9E00F3,								// colour ABGR
			gef::TJ_CENTRE,							// justification
			"Score: %.1i Time: %.1i",		// string of text to render
			main_cone.score,
			timer
			);
	}
}

void SceneApp::SetupLights()
{
	// grab the data for the default shader used for rendering 3D geometry
	gef::Default3DShaderData& default_shader_data = renderer_3d_->default_shader_data();

	// set the ambient light
	default_shader_data.set_ambient_light_colour(gef::Colour(0.25f, 0.25f, 0.25f, 1.0f));

	// add a point light that is almost white, but with a blue tinge
	// the position of the light is set far away so it acts light a directional light
	gef::PointLight default_point_light;
	default_point_light.set_colour(gef::Colour(0.7f, 0.7f, 1.0f, 1.0f));
	default_point_light.set_position(gef::Vector4(-500.0f, 400.0f, 700.0f));
	default_shader_data.AddPointLight(default_point_light);
}


void SceneApp::MenuInit()
{
	//sprite rend
	sprite_renderer_ = gef::SpriteRenderer::Create(platform_);

	// create the renderer for draw 3D geometry
	renderer_3d_ = gef::Renderer3D::Create(platform_);

	input_manager_ = gef::InputManager::Create(platform_);

	// initialise primitive builder to make create some 3D geometry easier
	primitive_builder_ = new PrimitiveBuilder(platform_);

	// initialise audio manager
	audio_manager_ = gef::AudioManager::Create();

	// initialise the physics world
	b2Vec2 gravity(0.0f, -9.81f);
	world_ = new b2World(gravity);

	InitFont();
	SetupLights();


	//load in audio assets
	if (audio_manager_)
	{
		scoop_sfx = audio_manager_->LoadSample("assets/Squelch sound.wav", platform_);
		point_sfx = audio_manager_->LoadSample("assets/box_collected.wav", platform_);
		gag_sfx = audio_manager_->LoadSample("assets/Gagging sound.wav", platform_);
		music = audio_manager_->LoadMusic("assets/van music.wav", platform_);
	}

	//play music
	//audio_manager_->PlayMusic();

	//menu graphic
	menu_texture_ = CreateTextureFromPNG("assets/vita menu screen.png", platform_);
}

void SceneApp::MenuUpdate()
{
	//controls
	if (input_manager_)
	{
		input_manager_->Update();

		gef::SonyControllerInputManager* controller_input = input_manager_->controller_input();

		//get controller input data for first controller
		if (controller_input)
		{
			const gef::SonyController* controller = controller_input->GetController(0);
			if (controller)
			{
				if (controller->buttons_pressed() & gef_SONY_CTRL_SELECT)
				{
					switch (difficulty)
					{
					case(60) :
						difficulty = 30;
						break;
					case(30) :
						difficulty = 60;
						break;
					}
				}

				if (controller->buttons_pressed() & gef_SONY_CTRL_START)
				{
					gameState = GAME;
					GameInit();
				}

				//trigger sound effect
				if (audio_manager_)
				{
					if (controller->buttons_pressed() & gef_SONY_CTRL_CROSS)
						audio_manager_->PlaySample(scoop_sfx);
				}
			}
		}
	}

}

void SceneApp::MenuRender()
{
	// sprites
	sprite_renderer_->Begin();

	//draw menu sprite
	menu_sprite_.set_texture(menu_texture_);
	menu_sprite_.set_position(platform_.width() * 0.5f, platform_.height() * 0.5f, 0.0f);
	menu_sprite_.set_width(platform_.width());
	menu_sprite_.set_height(platform_.height());
	menu_sprite_.set_rotation(0.0f);
	sprite_renderer_->DrawSprite(menu_sprite_);

	//menu text
	if (font_)
	{
		// Press start
		font_->RenderText(
			sprite_renderer_,						// sprite renderer to draw the letters
			gef::Vector4(platform_.width() * 0.5f, platform_.height() * 0.5f - 40, 0.0f),			// position on screen
			2.0f,									// scale
			0xFF9E00F3,								// colour ABGR
			gef::TJ_CENTRE,							// justification
			"Press Start"							// string of text to render
			);

		//change difficulty
		font_->RenderText(
			sprite_renderer_,						// sprite renderer to draw the letters
			gef::Vector4(platform_.width() * 0.5f, (platform_.height() * 0.5f) + 100, 0.0f),			// position on screen
			1.0f,									// scale
			0xFF9E00F3,								// colour ABGR
			gef::TJ_CENTRE,							// justification
			"Select to change the difficulty"							// string of text to render
			);

		//difficulty text, easy
		if (difficulty == 60)
		{
			// Press start
			font_->RenderText(
				sprite_renderer_,						// sprite renderer to draw the letters
				gef::Vector4(platform_.width() * 0.5f, (platform_.height() * 0.5f) + 150, 0.0f),			// position on screen
				1.0f,									// scale
				0xFF9E00F3,								// colour ABGR
				gef::TJ_CENTRE,							// justification
				"Difficulty: Easy"// string of text to render
				);
		}

		else if(difficulty == 30)
		{
			// Press start
			font_->RenderText(
				sprite_renderer_,						// sprite renderer to draw the letters
				gef::Vector4(platform_.width() * 0.5f, (platform_.height() * 0.5f) + 150, 0.0f),			// position on screen
				1.0f,									// scale
				0xFF9E00F3,								// colour ABGR
				gef::TJ_CENTRE,							// justification
				"Difficulty: Hard"// string of text to render
				);
		}


		//show highscore
		//change difficulty
		font_->RenderText(
			sprite_renderer_,						// sprite renderer to draw the letters
			gef::Vector4(platform_.width() * 0.5f, (platform_.height() * 0.5f - 30) + 100, 0.0f),			// position on screen
			1.0f,									// scale
			0xFF9E00F3,								// colour ABGR
			gef::TJ_CENTRE,							// justification
			"High Score: %.1i",
			highscore
			);
	}
	sprite_renderer_->End();
}

void SceneApp::MenuRelease()
{
	delete menu_texture_;
	menu_texture_ = NULL;
}


void SceneApp::GameInit()
{

	//Sprite Stuff
	//background
	background_texture_ = CreateTextureFromPNG("assets/Vita background.png", platform_);

	background_sprite_.set_texture(background_texture_);
	background_sprite_.set_position(platform_.width() * 0.5f, platform_.height() * 0.5f, 1.0f);
	background_sprite_.set_width(platform_.width());
	background_sprite_.set_height(platform_.height());
	background_sprite_.set_rotation(0.0f);

	//order
	//create all the textures
	choc_texture_ = CreateTextureFromPNG("assets/Choc Cone.png", platform_);
	stra_texture_ = CreateTextureFromPNG("assets/Strawberry Cone.png", platform_);
	mint_texture_ = CreateTextureFromPNG("assets/Mint Cone.png", platform_);
	vani_texture_ = CreateTextureFromPNG("assets/Vanilla Cone.png", platform_);

	//set default
	order_texture_ = choc_texture_;

	order_sprite_.set_texture(order_texture_);
	order_sprite_.set_position((platform_.width() * 0.5f) + 6.0f, 48.0f, 0.0f);
	order_sprite_.set_width(50);
	order_sprite_.set_height(95);
	order_sprite_.set_rotation(0.0f);


	// init hands
	//left
	model_scene_ = new gef::Scene();
	left_hand.player_hand_init(false, primitive_builder_, world_, platform_, renderer_3d_, model_scene_);
	//right
	model_scene_ = new gef::Scene();
	right_hand.player_hand_init(true, primitive_builder_, world_, platform_, renderer_3d_, model_scene_);

	//init ice cream
	model_scene_ = new gef::Scene();
	first_cream.ice_cream_init(primitive_builder_, world_, Vector4(-2.9f, 1.8f, 0.0f), CHOC, platform_, renderer_3d_, model_scene_);
	model_scene_ = new gef::Scene();
	second_cream.ice_cream_init(primitive_builder_, world_, Vector4(2.9f, 1.8f, 0.0f), STRA, platform_, renderer_3d_, model_scene_);
	model_scene_ = new gef::Scene();
	third_cream.ice_cream_init(primitive_builder_, world_, Vector4(-2.9f, -1.8f, 0.0f), MINT, platform_, renderer_3d_, model_scene_);
	model_scene_ = new gef::Scene();
	fourth_cream.ice_cream_init(primitive_builder_, world_, Vector4(2.9f, -1.8f, 0.0f), VANI, platform_, renderer_3d_, model_scene_);

	//init cone
	model_scene_ = new gef::Scene();
	main_cone.cone_init(primitive_builder_, world_, Vector4(0.0f, 0.0f, 0.0f), platform_, renderer_3d_, model_scene_);

	//init scoops
	for (int i = 0; i < 10; i++)
	{
		scoops.push_back(new scoop());
		model_scene_ = new gef::Scene();
		scoops.at(i)->scoop_init(primitive_builder_, world_, Vector4(5, i, 0), platform_, renderer_3d_, model_scene_);
	}

	//init first sequence for cone
	main_cone.Generate();

	//start timer
	//timerStart = clock() / CLOCKS_PER_SEC;
}

void SceneApp::GameUpdate()
{

	//update game time
	//timer = timerStart + difficulty - std::clock() / CLOCKS_PER_SEC;
	timer = 60;

	//player controls
	if (input_manager_)
	{
		input_manager_->Update();

		gef::SonyControllerInputManager* controller_input = input_manager_->controller_input();

		//get controller input data for first controller
		if (controller_input)
		{
			const gef::SonyController* controller = controller_input->GetController(0);
			if (controller)
			{
				//analog init
				left_hand.movement = b2Vec2(controller->left_stick_x_axis() * 10, -controller->left_stick_y_axis() * 10);
				right_hand.movement = b2Vec2(controller->right_stick_x_axis() * 10, -controller->right_stick_y_axis() * 10);

				//say triggers pressed (also check not already holding something)
				if (controller->buttons_down() & gef_SONY_CTRL_R2 && right_hand.GetHolding() != HELD)
					right_hand.SetHolding(GRABBING);

				else if (controller->buttons_released() & gef_SONY_CTRL_R2)
					right_hand.SetHolding(EMPTY);

				//left
				if (controller->buttons_down() & gef_SONY_CTRL_L2 && left_hand.GetHolding() != HELD)
					left_hand.SetHolding(GRABBING);

				else if (controller->buttons_released() & gef_SONY_CTRL_L2)
					left_hand.SetHolding(EMPTY);

				//press x to clear current cone
				if (controller->buttons_pressed() & gef_SONY_CTRL_CROSS)
					main_cone.Clear(false);

				//exit program
				if (controller->buttons_down() & gef_SONY_CTRL_START && controller->buttons_down() & gef_SONY_CTRL_SELECT)
				{
					gef::DebugOut("EXIT\n");
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////
	//update scoops
	for (int i = 0; i < scoops.size(); i++)
	{
		if (scoops.at(i)->GetHolding() == HELD)
			scoops.at(i)->MoveScoop();

		scoops.at(i)->UpdateFromSimulation(scoops.at(i)->scoop_body_);
	}

	//////////////////////////////////////////////////////////////
	//cone
	//check if correct
	main_cone.Detect(audio_manager_, point_sfx, gag_sfx);

	//update order sprite
	switch (main_cone.sequence)
	{
	case(CHOC):
		order_sprite_.set_texture(choc_texture_);
		break;

	case(STRA) :
		order_sprite_.set_texture(stra_texture_);
		break;

	case(MINT) :
		order_sprite_.set_texture(mint_texture_);
		break;

	case(VANI) :
		order_sprite_.set_texture(vani_texture_);
		break;
	}

	//collisions
	//////////////////////////////////////////////////////////////

	left_hand.UpdateMatrices();
	right_hand.UpdateMatrices();

	// update physics world
	float32 timeStep = 1.0f / 60.0f;

	int32 velocityIterations = 6;
	int32 positionIterations = 2;

	world_->Step(timeStep, velocityIterations, positionIterations);

	// collision detection
	// get the head of the contact list
	b2Contact* contact = world_->GetContactList();
	// get contact count
	int contact_count = world_->GetContactCount();

	for (int contact_num = 0; contact_num<contact_count; ++contact_num)
	{
		if (contact->IsTouching())
		{
			// get the colliding bodies
			b2Body* bodyA = contact->GetFixtureA()->GetBody();
			b2Body* bodyB = contact->GetFixtureB()->GetBody();

			// DO COLLISION RESPONSE HERE
			DetectCollisions(bodyA, bodyB, contact);
		}

		// Get next contact point
		contact = contact->GetNext();
	}

	//////////////////////////////////////////////////////////////

	//game over
	if (timer <= 0)
	{
		GameRelease();
		gameState = MENU;
		MenuInit();		
	}

}

void SceneApp::GameRender()
{
	// 2D Sprites ///////////////////////////////////////////////
	// start drawing sprites, but don't clear the frame buffer
	sprite_renderer_->Begin();

	//Load Background
	sprite_renderer_->DrawSprite(background_sprite_);

	sprite_renderer_->End();

	// 3D objects //////////////////////////////////////////////
	// draw 3d geometry
	renderer_3d_->Begin(false);

	//draw player hand meshes
	left_hand.Draw(renderer_3d_);
	right_hand.Draw(renderer_3d_);

	//draw ice creams
	first_cream.Draw(renderer_3d_);

	second_cream.Draw(renderer_3d_);
	third_cream.Draw(renderer_3d_);
	fourth_cream.Draw(renderer_3d_);

	//render cone
	main_cone.Draw(renderer_3d_);

	//render scoops
	for (int i = 0; i < scoops.size(); i++)
		scoops.at(i)->Draw(renderer_3d_);


	renderer_3d_->End();

	//render 2D HUD
	// start drawing sprites, but don't clear the frame buffer
	sprite_renderer_->Begin(false);

	//Load order
	sprite_renderer_->DrawSprite(order_sprite_);

	//HUD
	DrawHUD();

	sprite_renderer_->End();
}

void SceneApp::GameRelease()
{
	//store score
	if (highscore < main_cone.score)
		highscore = main_cone.score;

	//reset score
	main_cone.score = 0;

	//stop music
	audio_manager_->StopMusic();

	CleanUp();

	scoops.clear();
}

void SceneApp::DetectCollisions(b2Body* bodyA, b2Body* bodyB, b2Contact* contact)
{	
	GameObject* gameobject_A = NULL;
	GameObject* gameobject_B = NULL;
	ice_cream* ice_cream_tub = NULL;
	scoop* current_scoop = NULL;
	cone* current_cone = NULL;

 	gameobject_A = (GameObject*)bodyA->GetUserData();
	gameobject_B = (GameObject*)bodyB->GetUserData();

	//where B is ice cream
	if (gameobject_B->GetTag() == ICECREAM)
	{
		//set body to tub type
		ice_cream_tub = (ice_cream*)bodyB->GetUserData();

		//pickup by player and they have empty hand trying to grab
		if (gameobject_A->GetTag() == PLAYER && gameobject_A->GetHolding() == GRABBING)
		{
			//scoop out
			ice_cream_tub->GetScoop(&scoops, bodyA, audio_manager_, scoop_sfx, platform_);
			//set hand to holding an item
			gameobject_A->SetHolding(HELD);			
		}
	}
	
	//where A is ice cream
	else if (gameobject_A->GetTag() == ICECREAM)
	{
		//set body to tub type
		ice_cream_tub = (ice_cream*)bodyA->GetUserData();

		//pickup by player and they have empty hand trying to grab
		if (gameobject_B->GetTag() == PLAYER && gameobject_A->GetHolding() == GRABBING)
		{
			//scoop out
			ice_cream_tub->GetScoop(&scoops, bodyB, audio_manager_, scoop_sfx, platform_);
			//set hand to holding an item
			gameobject_B->SetHolding(HELD);
			
		}
	}

	///////////////////////////////////////////////////////
	//cone detection
	//where A is cone
	else if (gameobject_A->GetTag() == CONE)
	{
		//set body to cone type
		current_cone = (cone*)bodyA->GetUserData();

		//check if scoop hits cone
		if (gameobject_B->GetTag() == SCOOP && gameobject_B->GetHolding() == EMPTY)
		{
			//store current scoop
			current_scoop = (scoop*)bodyB->GetUserData();	

			//place scoop ontop of cone
			current_cone->FixScoop(current_scoop, audio_manager_, scoop_sfx);
		}
	}

	//where B is cone
	else if (gameobject_B->GetTag() == CONE)
	{
		//set body to cone type
		current_cone = (cone*)bodyB->GetUserData();

		//check if scoop hits cone
		if (gameobject_A->GetTag() == SCOOP && gameobject_A->GetHolding() == EMPTY)
		{
			//store current scoop
			current_scoop = (scoop*)bodyA->GetUserData();

			//place scoop ontop of cone
			current_cone->FixScoop(current_scoop, audio_manager_, scoop_sfx);
		}
	}
}

