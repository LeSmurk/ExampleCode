#pragma once
#include <system/platform.h>
#include <maths/vector2.h>
#include <maths/vector4.h>
#include <vector>
#include "primitive_builder.h"
#include <maths/math_utils.h>
#include <system/debug_log.h>
#include <Box2D\Box2D.h>
#include "game_object.h"
#include <graphics\renderer_3d.h>
#include <vector>
#include "scoop.h"
#include <load_texture.h>
#include <graphics\scene.h>
using namespace gef;

class ice_cream : public GameObject
{
public:
	//init ice cream block
	void ice_cream_init(PrimitiveBuilder* prim, b2World* world_, Vector4 pos, FLAVOUR_TAG fla, gef::Platform& platform, Renderer3D *rend, gef::Scene* model_scene_);
	//draw mesh in scene
	void Draw(Renderer3D* meshRend);
	//create scoop after collision
	void GetScoop(std::vector<scoop*> *scoops, b2Body *hand, AudioManager* audio_manager_, int scoop_sfx, gef::Platform& platform_);

	class gef::Mesh* mesh_;
	Material tubMat;

	//set flavour
	FLAVOUR_TAG flavour;

	// ice cream body
	b2Body* ice_cream_body_;
};

