#ifndef _SCENE_APP_H
#define _SCENE_APP_H

#include <system/application.h>
#include <graphics/sprite.h>
#include <maths/vector2.h>
#include <maths/vector4.h>
#include <vector>
#include "primitive_builder.h"
#include <graphics/mesh_instance.h>
#include <graphics/material.h>
#include "player_hand.h"
#include "ice_cream.h"
#include <Box2D\Box2D.h>
#include "game_object.h"
#include "cone.h"
#include "scoop.h"
#include <vector>
#include <time.h> 
#include <load_texture.h>
#include <audio\audio_manager.h>
#include <graphics/scene.h>

enum GAMESTATE
{
	MENU,
	GAME
};

// FRAMEWORK FORWARD DECLARATIONS
namespace gef
{
	class Platform;
	class SpriteRenderer;
	class Font;
	class InputManager;
	class Renderer3D;
	class Mesh;
}

class SceneApp : public gef::Application
{
public:
	SceneApp(gef::Platform& platform);
	void Init();
	void CleanUp();
	bool Update(float frame_time);
	void Render();
private:
	void GameInit();
	void GameUpdate();
	void GameRender();
	void GameRelease();

	void InitFont();
	void CleanUpFont();
	void DrawHUD();
	void SetupLights();
	void DetectCollisions(b2Body* bodyA, b2Body* bodyB, b2Contact* contact);

	//main menu stuff
	void MenuInit();
	void MenuUpdate();
	void MenuRender();
	void MenuRelease();

	//models scene
	gef::Scene* model_scene_;

	//gamestate
	GAMESTATE gameState = MENU;
    
	gef::SpriteRenderer* sprite_renderer_;
	gef::Font* font_;
	gef::Renderer3D* renderer_3d_;

	//main menu stuff
	gef::Texture* menu_texture_;
	gef::Sprite menu_sprite_;

	//2d Sprites
	gef::Texture* background_texture_;
	gef::Sprite background_sprite_;

	//orders
	gef::Texture* order_texture_;
	gef::Sprite order_sprite_;

	//flavours
	gef::Texture* choc_texture_;
	gef::Texture* stra_texture_;
	gef::Texture* mint_texture_;
	gef::Texture* vani_texture_;

	//input manager
	gef::InputManager* input_manager_;

	//3d rendering player
	PrimitiveBuilder* primitive_builder_;

	//audio
	AudioManager* audio_manager_;

	// create the physics world
	b2World* world_;

	//hand class
	player_hand left_hand;
	player_hand right_hand;

	//ice cream classes
	ice_cream first_cream;
	ice_cream second_cream;
	ice_cream third_cream;
	ice_cream fourth_cream;

	//cone class
	cone main_cone;

	//scoop storage
	std::vector<scoop*> scoops;

	// audio variables
	int music;
	int scoop_sfx;
	int point_sfx;
	int gag_sfx;

	//game timer
	//std::clock_t timerStart;
	int timer;
	int difficulty = 60;

	//highscore
	int highscore = 0;

	float fps_;
};

#endif // _SCENE_APP_H
