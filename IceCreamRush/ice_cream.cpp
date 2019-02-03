#include "ice_cream.h"

void ice_cream::ice_cream_init(PrimitiveBuilder* prim, b2World* world_, Vector4 pos, FLAVOUR_TAG fla, gef::Platform& platform_, Renderer3D *rend, gef::Scene* model_scene_)
{
	//ice cream variables
	SetTag(ICECREAM);

	//set flavour
	flavour = fla;
	switch (flavour)
	{
	case(CHOC) :
		//set texture
		tubMat.set_texture(CreateTextureFromPNG("chocTub.png", platform_));
		break;

	case(STRA) :
		tubMat.set_texture(CreateTextureFromPNG("straTub.png", platform_));
		break;

	case(MINT) :
		tubMat.set_texture(CreateTextureFromPNG("mintTub.png", platform_));
		break;

	case(VANI) :
		tubMat.set_texture(CreateTextureFromPNG("vaniTub.png", platform_));
		break;
	}

	model_scene_->ReadSceneFromFile(platform_, "Tub.scn");

	// we do want to render the data stored in the scene file so lets create the materials from the material data present in the scene file
	model_scene_->CreateMaterials(platform_);

	// now check to see if there is any mesh data in the file, if so lets create a mesh from it
	if (model_scene_->meshes.size() > 0)
		mesh_ = model_scene_->CreateMesh(platform_, model_scene_->meshes.front());

	// get the player mesh instance to use this mesh for drawing
	set_mesh(mesh_);

	//set_mesh(prim->CreateBoxMesh(gef::Vector4(0.25f, 0.25f, 0.5f), gef::Vector4(0.0f, 0.0f, 0.0f)));

	// create a physics body for the ice cream
	b2BodyDef ice_cream_body_def;
	ice_cream_body_def.type = b2_staticBody;
	ice_cream_body_def.position = b2Vec2(pos.x(), pos.y());

	ice_cream_body_ = world_->CreateBody(&ice_cream_body_def);

	// create the shape for the ice cream
	b2PolygonShape ice_cream_shape;
	ice_cream_shape.SetAsBox(0.4f, 0.25f);

	// create the fixture
	b2FixtureDef ice_cream_fixture_def;
	ice_cream_fixture_def.shape = &ice_cream_shape;
	ice_cream_fixture_def.density = 1.0f;

	// create the fixture on the rigid body
	ice_cream_body_->CreateFixture(&ice_cream_fixture_def);
	UpdateFromSimulation(ice_cream_body_);

	ice_cream_body_->SetUserData(this);

}

void ice_cream::Draw(gef::Renderer3D* meshRend)
{
	meshRend->set_override_material(&tubMat);
	meshRend->DrawMesh(*this);
}

void ice_cream::GetScoop(std::vector<scoop*> *scoops, b2Body *hand, AudioManager* audio_manager_, int scoop_sfx, gef::Platform& platform_)
{
	//check for nearest empty scoop
	for (int i = 0; i < scoops->size(); i++)
	{	
		if (scoops->at(i)->GetHolding() == EMPTY)
		{
			//spawn the correct scoop at the hand
			scoops->at(i)->InitMove(hand, flavour, audio_manager_, scoop_sfx, platform_);
			break;
		}
	}
}