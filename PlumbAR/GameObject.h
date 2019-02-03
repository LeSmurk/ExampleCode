#pragma once
#include <graphics/mesh_instance.h>
#include <graphics/sprite.h>
#include <maths/vector2.h>
#include <maths/vector4.h>
#include <maths/matrix44.h>
#include <vector>
#include "Collider.h"
#include "graphics\material.h"

class GameObject : public gef::MeshInstance
{
public:
	GameObject();
	~GameObject();

	//create types of object
	enum PipeType
	{
		junction,
		line,
		bend,
		tetris
	};
	//different types of pipes
	PipeType thisPipe;
	
	//colliders
	std::vector<Collider*> colliders;
	
	//init
	void InitObject(const gef::Mesh*, const gef::Mesh*, bool, bool, PipeType);
	
	//////////////////////////////////////////////
	void SetPosition(gef::Vector4);
	void MovePosition(gef::Vector4);
	gef::Vector4 GetPosition();

	void SetRotation(gef::Vector4);
	void MoveRotation(gef::Vector4);
	gef::Vector4 GetRotation();

	void SetScale(gef::Vector4);
	gef::Vector4 GetScale();

	//////////////////////////////////////////////
	//directly changing transform
	void SetTransform(gef::Matrix44, bool);

	//how we want this object to be relatively
	void SetLocalRelative(gef::Vector4);

	//wanting to be stored at a marker
	bool atMarker = false;
	//relative to marker
	gef::Vector4 relPosition;

	//Connected state of whole object to green
	bool SetConnectedCollider(int);
	int connected = -1;
	//first piece in puzzle
	bool startingPiece = false;
	//final piece in puzzle
	bool finalPiece = false;

	//been seen yet
	bool activeInScene = false;


private:
	float CorrectRotation(float);
	void SetRelative();

	float scaleX = 0;
	float scaleY = 0;
	float scaleZ = 0;
	gef::Matrix44 scaMatrix;

	//rotation local
	float rotationX = 0;
	float rotationY = 0;
	float rotationZ = 0;
	gef::Matrix44 rotMatrix;

	//gef::Matrix44 posMatrix;

	//relative to marker
	gef::Matrix44 relMatrix;


	enum ColliderPlace
	{
		top,
		right,
		bottom,
		left
	};

	//create a colider at the wanted relative
	void CreateCollider(ColliderPlace, const gef::Mesh*);


};

