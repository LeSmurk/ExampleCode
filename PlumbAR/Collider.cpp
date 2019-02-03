#include "Collider.h"



Collider::Collider()
{
}


Collider::~Collider()
{
}

void Collider::InitCollider(gef::Matrix44 relativeMat, const gef::Mesh* setMesh)
{
	gef::MeshInstance* newMesh = new gef::MeshInstance;
	colliderMesh = newMesh;

	colliderMesh->set_mesh(setMesh);

	//positions relative
	//localMat = relativeMat;
	localScale.SetIdentity();
	localScale.Scale(relativeMat.GetScale());
	localPos = relativeMat.GetTranslation();


}

void Collider::SetTransform(gef::Matrix44 objectTran)
{
	//use wanted position, scale and rotation to set to relative of object
	gef::Matrix44 newTran = objectTran;

	//separate into positions
	gef::Vector4 xVector(objectTran.GetRow(0));
	gef::Vector4 yVector(objectTran.GetRow(1));
	gef::Vector4 zVector(objectTran.GetRow(2));

	//set to relative to these axes
	gef::Vector4 newPos = newTran.GetTranslation() + xVector * localPos.x();
	newPos += yVector * localPos.y();
	newPos += zVector * localPos.z();


	//scale
	newTran = newTran * localScale;

	//position
	newTran.SetTranslation(newPos);

	colliderMesh->set_transform(newTran);
}