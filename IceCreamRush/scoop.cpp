#include "scoop.h"

void scoop::scoop_init(PrimitiveBuilder* prim, b2World* world_, Vector4 pos, gef::Platform& platform_, Renderer3D *rend, gef::Scene* model_scene_)
{
	//scoop variables
	SetTag(SCOOP);
	SetHolding(EMPTY);
	flavour = NONE;

	//Make textures
	chocTex = CreateTextureFromPNG("chocBall.png", platform_);
	straTex = CreateTextureFromPNG("straBall.png", platform_);
	mintTex = CreateTextureFromPNG("mintBall.png", platform_);
	vaniTex = CreateTextureFromPNG("vaniBall.png", platform_);

	model_scene_->ReadSceneFromFile(platform_, "Ball.scn");

	// we do want to render the data stored in the scene file so lets create the materials from the material data present in the scene file
	model_scene_->CreateMaterials(platform_);

	// now check to see if there is any mesh data in the file, if so lets create a mesh from it
	if (model_scene_->meshes.size() > 0)
		mesh_ = model_scene_->CreateMesh(platform_, model_scene_->meshes.front());

	// get the player mesh instance to use this mesh for drawing
	set_mesh(mesh_);

	//set_mesh(prim->CreateBoxMesh(gef::Vector4(0.1f, 0.1f, 0.1f), gef::Vector4(0.0f, 0.0f, 0.0f)));

	// create a physics body for the scoop
	b2BodyDef scoop_body_def;
	scoop_body_def.type = b2_dynamicBody;
	scoop_body_def.position = b2Vec2(pos.x(), pos.y());

	scoop_body_ = world_->CreateBody(&scoop_body_def);

	// create the shape for the scoop
	b2PolygonShape scoop_shape;
	scoop_shape.SetAsBox(0.1f, 0.1f);

	// create the fixture
	b2FixtureDef scoop_fixture_def;
	scoop_fixture_def.shape = &scoop_shape;
	scoop_fixture_def.density = 1.0f;

	// create the fixture on the rigid body
	scoop_body_->CreateFixture(&scoop_fixture_def);
	
	scoop_fixture_def.isSensor = true;
	UpdateFromSimulation(scoop_body_);

	scoop_body_->SetUserData(this);
}

void scoop::InitMove(b2Body *playerBody, FLAVOUR_TAG fla, AudioManager* audio_manager_, int scoop_sfx, gef::Platform& platform_)
{
	//set flavour
	flavour = fla;

	switch (flavour)
	{
	case(CHOC):
		//set texture
		scoopMat.set_texture(chocTex);
		break;

	case(STRA):
		scoopMat.set_texture(straTex);
		break;

	case(MINT):
		scoopMat.set_texture(mintTex);
		break;

	case(VANI):
		scoopMat.set_texture(vaniTex);
		break;
	}

	//store player info
	currentHand = playerBody;
	currentHandObject = (GameObject*)playerBody->GetUserData();

	//initialse holding offset based on hand
	if(currentHandObject->handRight == true)
		offset = b2Vec2(-0.5, -0.2);

	else
		offset = b2Vec2(0.5, -0.2);

	//say used to hand and scoop
	SetHolding(HELD);
	currentHandObject->SetHolding(HELD);

	//set body to move using velocity
	scoop_body_->SetTransform(playerBody->GetPosition() + offset, 0);

	UpdateFromSimulation(scoop_body_);

	//make noise
	audio_manager_->PlaySample(scoop_sfx);
}

void scoop::MoveScoop()
{
	scoop_body_->SetTransform(currentHand->GetPosition() + offset, 0);
	
	//if the hand lets go, drop scoop
	if (currentHandObject->GetHolding() == EMPTY)
		SetHolding(EMPTY);
}


void scoop::Draw(gef::Renderer3D* meshRend)
{
	meshRend->set_override_material(&scoopMat);
	meshRend->DrawMesh(*this);
}

void scoop::Reset()
{
	//reset as if new
	scoop_body_->SetTransform(b2Vec2(10, 10), 0);
	SetHolding(EMPTY);
	flavour = NONE;
}

