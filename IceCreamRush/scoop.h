#pragma once
#include <maths/vector4.h>
#include <vector>
#include "primitive_builder.h"
#include <maths/math_utils.h>
#include <system/debug_log.h>
#include <Box2D\Box2D.h>
#include "game_object.h"
#include <graphics\renderer_3d.h>
#include <audio\audio_manager.h>
#include <graphics\scene.h>
#include <load_texture.h>
using namespace gef;

class scoop : public GameObject
{
public:
	//init ice cream block
	void scoop_init(PrimitiveBuilder* prim, b2World* world_, Vector4 pos, gef::Platform& platform, Renderer3D *rend, gef::Scene* model_scene_);
	//draw mesh in scene
	void Draw(Renderer3D* meshRend);

	//reset
	void Reset();

	//moving
	void InitMove(b2Body *playerBody, FLAVOUR_TAG fla, AudioManager* audio_manager_, int scoop_sfx, gef::Platform& platform_);
	void MoveScoop();

	//hand info
	b2Body *currentHand;
	GameObject *currentHandObject;

	//sccop offset
	b2Vec2 offset;

	//set flavour
	FLAVOUR_TAG flavour;

	class gef::Mesh* mesh_;
	Material scoopMat;

	//premake the textures
	Texture *chocTex;
	Texture *straTex;
	Texture *mintTex;
	Texture *vaniTex;

	//body
	b2Body* scoop_body_;
};

