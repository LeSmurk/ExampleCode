#pragma once
#include <graphics/mesh_instance.h>
#include <graphics/sprite.h>
#include <maths/vector2.h>
#include <maths/vector4.h>
#include <maths/matrix44.h>
#include <vector>

//used to attach a separate collider component to the objects
//moving collider locally within a gameobject
class Collider
{
public:
	Collider();
	~Collider();

	//position and size relative
	void InitCollider(gef::Matrix44, const gef::Mesh*);

	void SetTransform(gef::Matrix44);

	gef::MeshInstance* colliderMesh;	


private:

	gef::Matrix44 localMat;
	gef::Vector4 localPos;
	gef::Matrix44 localScale;
};

