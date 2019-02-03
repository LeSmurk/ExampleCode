#pragma once
#include <maths/vector4.h>
#include <vector>
#include "primitive_builder.h"
#include <maths/math_utils.h>
#include <system/debug_log.h>
#include <Box2D\Box2D.h>
#include "game_object.h"
#include <graphics\renderer_3d.h>
#include "scoop.h"
#include <vector>
#include <stdlib.h>
#include <time.h>
#include "load_texture.h"
#include <string>
#include <graphics\scene.h>
#include <graphics\material.h>
using namespace gef;

class cone : GameObject
{
public:
	//init cone
	void cone_init(PrimitiveBuilder* prim, b2World* world_, Vector4 pos, gef::Platform& platform, Renderer3D *rend, gef::Scene* model_scene_);
	//draw mesh in scene
	void Draw(Renderer3D* meshRend);

	class gef::Mesh* mesh_;
	Material coneMat;

	//cone storing scoops
	void FixScoop(scoop* topScoop, AudioManager* audio_manager_, int scoop_sfx);
	std::vector<scoop*> placedScoops;

	//generate combination
	void Generate();
	FLAVOUR_TAG sequence;

	//detect when finished
	void Detect(AudioManager* audio_manager_, int point_sfx, int gag_sfx);

	//clear whats on cone
	void Clear(bool newGen);

	//body
	b2Body* cone_body_;

	//score
	int score = 0;

};

