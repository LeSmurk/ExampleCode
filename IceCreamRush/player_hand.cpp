#include "player_hand.h"

void player_hand::player_hand_init(bool handR, PrimitiveBuilder* prim, b2World* world_, gef::Platform& platform_, Renderer3D *rend, gef::Scene* model_scene_)
{
	//player variables
	startRotZ = 0;
	SetTag(PLAYER);

	//set player to holding nothing
	SetHolding(EMPTY);

	//set texture
	handMat.set_texture(CreateTextureFromPNG("handTxr.png", platform_));

	if(handR == true)
		model_scene_->ReadSceneFromFile(platform_, "HandRight.scn");

	else
		model_scene_->ReadSceneFromFile(platform_, "HandLeft.scn");

	// we do want to render the data stored in the scene file so lets create the materials from the material data present in the scene file
	model_scene_->CreateMaterials(platform_);

	// now check to see if there is any mesh data in the file, if so lets create a mesh from it
	if (model_scene_->meshes.size() > 0)
		mesh_ = model_scene_->CreateMesh(platform_, model_scene_->meshes.front());

	// get the player mesh instance to use this mesh for drawing
	set_mesh(mesh_);

	// create a physics body for the player
	b2BodyDef player_body_def;
	player_body_def.type = b2_dynamicBody;
	player_body_def.position = b2Vec2(0.0f, 4.0f);

	player_body_ = world_->CreateBody(&player_body_def);

	// create the shape for the player
	b2Vec2 vertices[7];
	//bottton left, anti-clockwise (I know, unconventional)
	vertices[0].Set(0.3, -0.7);
	vertices[1].Set(0.4, -0.7);
	vertices[2].Set(0.2, 0.5);
	vertices[3].Set(-0.3, 0.5);
	vertices[4].Set(-0.2, 0);
	vertices[5].Set(-0.4, -0.2);
	vertices[6].Set(0, -0.1);


	//check if left or right hand being initialised
	//left hand setup
	if(handR == false)
	{
		handRight = false;

		//change the shape for left hand
		vertices[0].Set(-0.3, -0.7);
		vertices[1].Set(-0.4, -0.7);
		vertices[2].Set(-0.2, 0.5);
		vertices[3].Set(0.3, 0.5);
		vertices[4].Set(0.2, 0);
		vertices[5].Set(0.4, -0.2);
		vertices[6].Set(0, -0.1);

		rotation.set_z(DegToRad(startRotZ));
		translation = b2Vec2(-1.0f, 0.0f);
	}

	//right hand setup
	else
	{
		handRight = true;

		rotation.set_z(DegToRad(-startRotZ));
		translation = b2Vec2(1.0f, 0.0f);
	}

	b2PolygonShape player_shape;
	player_shape.Set(vertices, 7);

	// create the fixture
	b2FixtureDef player_fixture_def;
	player_fixture_def.shape = &player_shape;
	player_fixture_def.density = 1.0f;

	// create the fixture on the rigid body
	player_body_->CreateFixture(&player_fixture_def);

	player_body_->SetGravityScale(0.0f);
	player_body_->SetTransform(translation, rotation.z());
	player_body_->SetFixedRotation(true);

	player_body_->SetUserData(this);

}

void player_hand::UpdateMatrices()
{
	translation = player_body_->GetTransform().p;
	//lock from moving too far
	//x
	if (translation.x > 3)
	{
		translation.Set(3, translation.y);
		player_body_->SetTransform(translation, player_body_->GetAngle());
	}

	if (translation.x < -3)
	{
		translation.Set(-3, translation.y);
		player_body_->SetTransform(translation, player_body_->GetAngle());
	}
		

	//y
	if (translation.y > 2)
	{
		translation.Set(translation.x, 2);
		player_body_->SetTransform(translation, player_body_->GetAngle());
	}

	if (translation.y < -2)
	{
		translation.Set(translation.x, -2);
		player_body_->SetTransform(translation, player_body_->GetAngle());
	}

	//set body to move using velocity
	player_body_->SetLinearVelocity(movement);
	//update from collision sim
	UpdateFromSimulation(player_body_);

}

void player_hand::Draw(gef::Renderer3D* meshRend)
{
	meshRend->set_override_material(&handMat);
	meshRend->DrawMesh(*this);
}