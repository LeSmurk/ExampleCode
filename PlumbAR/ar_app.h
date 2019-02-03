#ifndef _RENDER_TARGET_APP_H
#define _RENDER_TARGET_APP_H

#include <system/application.h>
#include <graphics/sprite.h>
#include <maths/vector2.h>
#include <vector>
#include <graphics/mesh_instance.h>
#include <platform/vita/graphics/texture_vita.h>
#include "primitive_builder.h"
#include "GameObject.h"
#include "MarkerHandler.h"
#include "LevelManager.h"
#include "States.h"
// Vita AR includes
#include <camera.h>
#include <gxm.h>
#include <motion.h>
#include <libdbg.h>
#include <libsmart.h>

// FRAMEWORK FORWARD DECLARATIONS
namespace gef
{
	class Platform;
	class SpriteRenderer;
	class Font;
	class Renderer3D;
	class Mesh;
	class RenderTarget;
	class TextureVita;
	class InputManager;
	class Scene;
}


class ARApp : public gef::Application
{
public:
	ARApp(gef::Platform& platform);
	void Init();
	void CleanUp();
	bool Update(float frame_time);
	void Render();
private:
	void InitFont();
	void CleanUpFont();
	void DrawHUD();

	void RenderOverlay();
	void SetupLights();
	
	gef::InputManager* input_manager_;

	gef::SpriteRenderer* sprite_renderer_;
	gef::Font* font_;
	
	float fps_;

	class gef::Renderer3D* renderer_3d_;
	PrimitiveBuilder* primitive_builder_;

	float scaleYFactor;
	gef::Matrix44* orthoProjMatrix;
	
	//camera image
	gef::Sprite cam_feed_sprite_;
	//set pos of sprite to back of proj volume
	//scale sprite to y-scaling factor	
	gef::TextureVita* tex_vita_map_;

	gef::Matrix44 proj_matrix3d;
	gef::Matrix44 view_matix3d;

	//Current State
	States stateHandler;

	//model scenes
	//model loading
	gef::Mesh* GetFirstMesh(gef::Scene*);
	//scene
	gef::Scene* junctionScene;
	gef::Scene* bendScene;
	gef::Scene* straightScene;
	gef::Scene* tetrisScene;

	//level info
	LevelManager levelManager;

};




#endif // _RENDER_TARGET_APP_H