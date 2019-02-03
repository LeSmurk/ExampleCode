#include "ar_app.h"
#include <system/platform.h>
#include <graphics/sprite_renderer.h>
#include <graphics/texture.h>
#include <graphics/mesh.h>
#include <graphics/primitive.h>
#include <assets/png_loader.h>
#include <graphics/image_data.h>
#include <graphics/font.h>
#include <input/touch_input_manager.h>
#include <maths/vector2.h>
#include <input/input_manager.h>
#include <input/sony_controller_input_manager.h>
#include <maths/math_utils.h>
#include <graphics/scene.h>
#include <graphics/renderer_3d.h>
#include <graphics/render_target.h>

#include <sony_sample_framework.h>
#include <sony_tracking.h>


ARApp::ARApp(gef::Platform& platform) :
	Application(platform),
	input_manager_(NULL),
	sprite_renderer_(NULL),
	font_(NULL),
	renderer_3d_(NULL),
	primitive_builder_(NULL),
	orthoProjMatrix(NULL),
	tex_vita_map_(NULL),
	junctionScene(NULL),
	bendScene(NULL),
	straightScene(NULL),
	tetrisScene(NULL)

{
}

void ARApp::Init()
{
	input_manager_ = gef::InputManager::Create(platform_);
	sprite_renderer_ = gef::SpriteRenderer::Create(platform_);
	renderer_3d_ = gef::Renderer3D::Create(platform_);
	primitive_builder_ = new PrimitiveBuilder(platform_);

	InitFont();
	SetupLights();

	// initialise sony framework
	sampleInitialize();
	smartInitialize();

	// reset marker tracking
	AppData* dat = sampleUpdateBegin();
	smartTrackingReset();
	sampleUpdateEnd(dat);

	//CAMERA STUFF
	/////////////////////////////////////////////////////////////////////////////////////////////////
	//calculate scale y factor
	float camRatio = float(SCE_SMART_IMAGE_WIDTH) / float(SCE_SMART_IMAGE_HEIGHT);
	float displayRatio = float(platform_.width()) / float(platform_.height());
	scaleYFactor = displayRatio / camRatio;
	proj_matrix3d = platform_.PerspectiveProjectionFov(SCE_SMART_IMAGE_FOV, float(SCE_SMART_IMAGE_WIDTH) / float(SCE_SMART_IMAGE_HEIGHT), PROJECTION_ZNEAR, PROJECTION_ZFAR);

	//scale proj matrix
	gef::Matrix44 scaleMatrix;
	scaleMatrix.SetRow(0, gef::Vector4(1, 0, 0, 0));
	scaleMatrix.SetRow(1, gef::Vector4(0, scaleYFactor, 0, 0));
	scaleMatrix.SetRow(2, gef::Vector4(0, 0, 1, 0));
	scaleMatrix.SetRow(3, gef::Vector4(0, 0, 0, 1));
	proj_matrix3d = proj_matrix3d * scaleMatrix;

	//identity matrix for rendering
	view_matix3d.SetIdentity();

	//set feed width and height to scale
	cam_feed_sprite_.set_width(2.0f);
	cam_feed_sprite_.set_height(2.0f * scaleYFactor);
	//set feed position to be at back of projection volume
	cam_feed_sprite_.set_position(0, 0, 1);

	//set sprite image texture
	tex_vita_map_ = new gef::TextureVita();
	cam_feed_sprite_.set_texture(tex_vita_map_);

	//////////////////////////////////////////////////////////////////////////////////////////////
	//scenes
	junctionScene = new gef::Scene();
	junctionScene->ReadSceneFromFile(platform_, "junctionpipescn.scn");
	junctionScene->CreateMaterials(platform_);
	bendScene = new gef::Scene();
	bendScene->ReadSceneFromFile(platform_, "curvedpipescn.scn");
	bendScene->CreateMaterials(platform_);
	straightScene = new gef::Scene();
	straightScene->ReadSceneFromFile(platform_, "straightpipescn.scn");
	straightScene->CreateMaterials(platform_);
	tetrisScene = new gef::Scene();
	tetrisScene->ReadSceneFromFile(platform_, "tetrispipescn.scn");
	tetrisScene->CreateMaterials(platform_);

	//Pass meshes to level handler
	levelManager.meshJunction = GetFirstMesh(junctionScene);
	levelManager.meshBend = GetFirstMesh(bendScene);
	levelManager.meshStraight = GetFirstMesh(straightScene);
	levelManager.meshTetris = GetFirstMesh(tetrisScene);

	//Load texures from files
	stateHandler.LoadTextures(&platform_);

	//levelManager.LoadFromFile(0);

	//Start on Menu
	levelManager.ChangeLevel(-1);

	//init state that we are on
	stateHandler.InitState(&levelManager, primitive_builder_);

}

void ARApp::CleanUp()
{
	delete primitive_builder_;
	primitive_builder_ = NULL;

	smartRelease();
	sampleRelease();

	CleanUpFont();
	delete sprite_renderer_;
	sprite_renderer_ = NULL;

	delete renderer_3d_;
	renderer_3d_ = NULL;

	delete input_manager_;
	input_manager_ = NULL;
#
	delete orthoProjMatrix;
	orthoProjMatrix = NULL;

	//delete cam_feed_sprite_;
	//cam_feed_sprite_ = NULL;

	delete tex_vita_map_;
	tex_vita_map_ = NULL;

}

bool ARApp::Update(float frame_time)
{
	fps_ = 1.0f / frame_time;
	
	//fps_ = cube_object_->GetRotation().x();

	stateHandler.UpdateState(&levelManager, input_manager_, primitive_builder_);


	return true;
}

void ARApp::Render()
{
	AppData* dat = sampleRenderBegin();

	//
	// Render the camera feed
	//

	// REMEMBER AND SET THE PROJECTION MATRIX HERE
	gef::Matrix44 proj_matrix2d;

	proj_matrix2d = platform_.OrthographicFrustum(-1, 1, -1, 1, -1, 1);
	sprite_renderer_->set_projection_matrix(proj_matrix2d);

	cam_feed_sprite_.set_texture(tex_vita_map_);

	sprite_renderer_->Begin(true);

	// check there is data present for the camera image before trying to draw it
	if (dat->currentImage)
	{
		tex_vita_map_->set_texture(dat->currentImage->tex_yuv);
		//sprite_renderer_->DrawSprite(camera_image_sprite_);

		// DRAW CAMERA FEED SPRITE HERE
		sprite_renderer_->DrawSprite(cam_feed_sprite_);

	}

	sprite_renderer_->End();

	//
	// Render 3D scene
	//

	// SET VIEW AND PROJECTION MATRIX HERE

	renderer_3d_->set_view_matrix(view_matix3d);
	

	//set to projection matrix
	renderer_3d_->set_projection_matrix(proj_matrix3d);

	// Begin rendering 3D meshes, don't clear the frame buffer
	renderer_3d_->Begin(false);

	//RENDER CALL
	stateHandler.Render3D(&levelManager, renderer_3d_, primitive_builder_);

	renderer_3d_->End();

	RenderOverlay();

	sampleRenderEnd();
}


void ARApp::RenderOverlay()
{
	//
	// render 2d hud on top
	//
	gef::Matrix44 proj_matrix2d;

	proj_matrix2d = platform_.OrthographicFrustum(0.0f, platform_.width(), 0.0f, platform_.height(), -1.0f, 1.0f);
	sprite_renderer_->set_projection_matrix(proj_matrix2d);
	sprite_renderer_->Begin(false);

	//RENDER CALL
	stateHandler.Render2D(&levelManager, sprite_renderer_);

	DrawHUD();
	sprite_renderer_->End();
}


void ARApp::InitFont()
{
	font_ = new gef::Font(platform_);
	font_->Load("comic_sans");
}

void ARApp::CleanUpFont()
{
	delete font_;
	font_ = NULL;
}

void ARApp::DrawHUD()
{
	if(font_)
	{
		//font_->RenderText(sprite_renderer_, gef::Vector4(850.0f, 510.0f, -0.9f), 1.0f, 0xffffffff, gef::TJ_LEFT, "FPS: %.1f", fps_);

		//render level number
		if (levelManager.currentLevel > 0)
			font_->RenderText(sprite_renderer_, gef::Vector4(750.0f, 0, -0.9f), 1.0f, 0xffffffff, gef::TJ_LEFT, "Level Number: %.1i", levelManager.currentLevel);
	}
}

void ARApp::SetupLights()
{
	gef::PointLight default_point_light;
	default_point_light.set_colour(gef::Colour(0.7f, 0.7f, 1.0f, 1.0f));
	default_point_light.set_position(gef::Vector4(-300.0f, -500.0f, 100.0f));

	gef::Default3DShaderData& default_shader_data = renderer_3d_->default_shader_data();
	default_shader_data.set_ambient_light_colour(gef::Colour(0.5f, 0.5f, 0.5f, 1.0f));
	default_shader_data.AddPointLight(default_point_light);
}

//MODEL LOADING
//GETTING MODELS FROM SCENES
gef::Mesh* ARApp::GetFirstMesh(gef::Scene* scene)
{
	gef::Mesh* mesh = NULL;

	if (scene)
	{
		// now check to see if there is any mesh data in the file, if so lets create a mesh from it
		if (scene->mesh_data.size() > 0)
			mesh = scene->CreateMesh(platform_, scene->mesh_data.front());
	}

	return mesh;
}
