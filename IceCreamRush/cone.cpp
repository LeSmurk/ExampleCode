#include "cone.h"

void cone::cone_init(PrimitiveBuilder* prim, b2World* world_, Vector4 pos, gef::Platform& platform_, Renderer3D *rend, gef::Scene* model_scene_)
{
	//tag
	obj_tag = CONE;

	//set texture
	coneMat.set_texture(CreateTextureFromPNG("coneTxr.png", platform_));
	
	model_scene_->ReadSceneFromFile(platform_, "Cone.scn");

	// we do want to render the data stored in the scene file so lets create the materials from the material data present in the scene file
	model_scene_->CreateMaterials(platform_);

	// now check to see if there is any mesh data in the file, if so lets create a mesh from it
	if (model_scene_->meshes.size() > 0)
		mesh_ = model_scene_->CreateMesh(platform_, model_scene_->meshes.front());

	// get the player mesh instance to use this mesh for drawing
	set_mesh(mesh_);

	// create a physics body for the ice cream
	b2BodyDef cone_body_def;
	cone_body_def.type = b2_staticBody;
	cone_body_def.position = b2Vec2(pos.x(), pos.y());

	cone_body_ = world_->CreateBody(&cone_body_def);

	// create the shape for the cone
	b2Vec2 vertices[3];
	vertices[0].Set(-0.3, 0.5);
	vertices[1].Set(0, -0.6);
	vertices[2].Set(0.3, 0.5);

	b2PolygonShape cone_shape;
	cone_shape.Set(vertices, 3);

	// create the fixture
	b2FixtureDef cone_fixture_def;
	cone_fixture_def.shape = &cone_shape;
	cone_fixture_def.density = 1.0f;

	// create the fixture on the rigid body
	cone_body_->CreateFixture(&cone_fixture_def);

	UpdateFromSimulation(cone_body_);

	cone_body_->SetUserData(this);

	//initialize random seed
	srand(time(NULL));

}

void cone::Draw(gef::Renderer3D* rend)
{
	rend->set_override_material(&coneMat);
	rend->DrawMesh(*this);
}

void cone::FixScoop(scoop* topScoop, AudioManager* audio_manager_, int scoop_sfx)
{
	//store the scoop in vector
	placedScoops.push_back(topScoop);

	//set scoop to stored
	topScoop->SetHolding(STORED);

	//physically place the scoop
	topScoop->scoop_body_->SetTransform(cone_body_->GetPosition() + b2Vec2(0, 0.8), 0);

	//make noise
	audio_manager_->PlaySample(scoop_sfx);
}

void cone::Generate()
{
	//pick between the four flavours
	switch (rand() % 4)
	{
	case(0) :
		sequence = CHOC;
		break;
	case(1) :
		sequence = STRA;
		break;
	case(2) :
		sequence = MINT;
		break;
	case(3) :
		sequence = VANI;
		break;
	}

	//output to screen
}

void cone::Detect(AudioManager* audio_manager_, int point_sfx, int gag_sfx)
{
	for (int i = 0; i < placedScoops.size(); i++)
	{
		//check if correct placement
		if (placedScoops.at(i)->flavour == sequence && placedScoops.size() == 1)
		{
			//increment score
			score++;

			//play ding noise
			audio_manager_->PlaySample(point_sfx);

			//clear the setup
			Clear(true);
		}

		//check if placed the correct under wrong ones
		else if (placedScoops.at(i)->flavour == sequence && placedScoops.size() > 1)
		{
			//increment score
			score--;

			//play ding noise
			audio_manager_->PlaySample(gag_sfx);

			//clear the setup
			Clear(true);
		}
	}

}

void cone::Clear(bool newGen)
{
	//empty those stored on cone
	while (placedScoops.size() != 0)
	{
		placedScoops.front()->Reset();
		placedScoops.erase(placedScoops.begin());
	}

	//create new sequence
	if (newGen == true)		
		Generate();

}