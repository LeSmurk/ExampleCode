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
#include <graphics\scene.h>
#include "load_texture.h"
using namespace gef;

class player_hand : public GameObject
{
public:

	//hand stored variables
	b2Vec2 translation;
	Vector4 rotation;
	float startRotZ;
	b2Vec2 movement;

	// player body
	b2Body* player_body_;
	Material handMat;

	//init hand
	void player_hand_init(bool handR, PrimitiveBuilder* prim, b2World* world_, gef::Platform& platform_, Renderer3D *rend, gef::Scene* model_scene_);
	//update based on events
	void UpdateMatrices();
	//draw mesh in scene
	void Draw(Renderer3D* meshRend);
};

