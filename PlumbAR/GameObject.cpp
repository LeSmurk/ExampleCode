#include "GameObject.h"
#include "maths\math_utils.h"

GameObject::GameObject()
{

}


GameObject::~GameObject()
{
}

void GameObject::InitObject(const gef::Mesh* objectMesh, const gef::Mesh* colliderMesh, bool puzzleStart, bool puzzleEnd, PipeType setPipe)
{
	//set this object to what we want
	set_mesh(objectMesh);

	//set to starting piece and connected
	startingPiece = puzzleStart;
	finalPiece = puzzleEnd;
	if (puzzleStart)
		connected = 0;
	else
		connected = -1;

	//set to a type of pipe
	thisPipe = setPipe;

	//clear colliders first
	colliders.clear();

	//create coliders for the type of pipe this is
	switch (thisPipe)
	{
		case(junction):
			CreateCollider(top, colliderMesh);
			CreateCollider(right, colliderMesh);
			CreateCollider(left, colliderMesh);
			CreateCollider(bottom, colliderMesh);
			break;
		case(line):
			CreateCollider(right, colliderMesh);
			CreateCollider(left, colliderMesh);
			break;
		case(bend):
			CreateCollider(right, colliderMesh);
			CreateCollider(bottom, colliderMesh);
			break;
		case(tetris):
			CreateCollider(right, colliderMesh);
			CreateCollider(bottom, colliderMesh);
			CreateCollider(left, colliderMesh);
			break;
	}


	//init position, scale and rotation
	SetPosition(gef::Vector4(0, 0, -10, 0));
	SetRotation(gef::Vector4(0, 0, 0));
	SetScale(gef::Vector4(0.5, 0.5, 0.5));

}

//////////////////////////////////////////
//manually place at a position
void GameObject::SetPosition(gef::Vector4 newPos)
{
	gef::Matrix44 newTran = transform_;
	newTran.SetTranslation(newPos);

	set_transform(newTran);
}

//move by an amount
void GameObject::MovePosition(gef::Vector4 moveValue)
{
	//gef::Matrix44 newTran = transform();
	//directly accessing the transform?
	transform_.SetTranslation(transform_.GetTranslation() + moveValue);
}

gef::Vector4 GameObject::GetPosition()
{
	return transform_.GetTranslation();
}

void GameObject::SetRotation(gef::Vector4 newRot)
{
	//create transform matrix
	gef::Matrix44 newTran = transform();

	//store in degrees
	rotationX = newRot.x();
	rotationX = CorrectRotation(rotationX);

	rotationY = newRot.y();
	rotationY = CorrectRotation(rotationY);

	rotationZ = newRot.z();
	rotationZ = CorrectRotation(rotationZ);

	//convert degrees to rad
	float x = (newRot.x() * 3.1415) / 180;
	float y = (newRot.y() * 3.1415) / 180;
	float z = (newRot.z() * 3.1415) / 180;

	//rotate X
	gef::Matrix44 rotX = transform();
	rotX.RotationX(x);

	//rotate Y
	gef::Matrix44 rotY = transform();
	rotY.RotationY(y);

	//rotate z
	gef::Matrix44 rotZ = transform();
	rotZ.RotationZ(z);

	rotMatrix = rotZ * rotY * rotZ;

	//perform all rotations and scale
	newTran = scaMatrix * rotMatrix;

	////readjust position
	newTran.SetTranslation(transform_.GetTranslation());

	set_transform(newTran);

}

//rotations
void GameObject::MoveRotation(gef::Vector4 moveRot)
{
	//transform matrix
	gef::Matrix44 newTran = transform();

	//store into rotation (degrees)
	rotationX += moveRot.x();
	rotationX = CorrectRotation(rotationX);

	rotationY += moveRot.y();
	rotationY = CorrectRotation(rotationY);

	rotationZ += moveRot.z();
	rotationZ = CorrectRotation(rotationZ);

	//convert degrees to rad
	float x = (rotationX * 3.1415) / 180;
	float y = (rotationY * 3.1415) / 180;
	float z = (rotationZ * 3.1415) / 180;

	//rotate X
	gef::Matrix44 rotX = transform();
	rotX.RotationX(x);

	//rotate Y
	gef::Matrix44 rotY = transform();
	rotY.RotationY(y);

	//rotate z
	gef::Matrix44 rotZ = transform();
	rotZ.RotationZ(z);

	//store into rotation matrix
	rotMatrix = rotX * rotY * rotZ;

	//perform all rotations and redo scale
	newTran = scaMatrix * rotMatrix;

	////readjust position
	newTran.SetTranslation(transform_.GetTranslation());

	set_transform(newTran);
}

gef::Vector4 GameObject::GetRotation()
{
	return gef::Vector4(rotationX, rotationY, rotationZ, 0);
}

float GameObject::CorrectRotation(float rot)
{
	float newRot = rot;

	//If greater than full rotation, find difference
	if (rot > 360)
	{
		//find largest whole multiple of 360
		int multi = rot / 360;

		//find how much above whole 360 rotation
		newRot = rot - (360 * multi);
	}

	//going negatively
	else if (rot < 0)
	{
		//find largest whole multiple of 360
		int multi = (rot / -360);

		newRot = 360 + (360 * multi) + rot;
	}

	return newRot;
}

//scaling
void GameObject::SetScale(gef::Vector4 newScale)
{
	gef::Matrix44 newTran = transform_;

	gef::Matrix44 scaleMat = newTran;
	scaleMat.Scale(newScale);
	scaMatrix = scaleMat;

	//perform all scaling and rotation
	newTran = scaMatrix * rotMatrix;

	//redo position
	newTran.SetTranslation(transform_.GetTranslation());

	set_transform(newTran);
}

gef::Vector4 GameObject::GetScale()
{
	//gef::Vector4 scale = gef::Vector4(scaleX, scaleY, scaleZ, 0);
	//return scale;
}


////////////////////////////////////////////////////
//direct transform
void GameObject::SetTransform(gef::Matrix44 newTran, bool atMarker)
{
	//if first time at marker, set active
	activeInScene = atMarker;
	//activeInScene = true;

	////scale the new tran by what we want
	gef::Matrix44 setTran = newTran;
	gef::Matrix44 scaleMat;
	scaleMat.SetIdentity();
	scaleMat.Scale(gef::Vector4(0.12, 0.12, 0.12));
	
	//seems to only handle rotation and scale?
	setTran = newTran * scaleMat;
	//redo position to marker
	setTran.SetTranslation(newTran.GetTranslation());

	set_transform(setTran);

	//has been set to transform
	atMarker = true;

	//place at position we want to be relative to this
	SetRelative();
}

//place to a position relative to marker
void GameObject::SetRelative()
{
	//where we want to be locally
	gef::Matrix44 newTran;
	newTran = transform_;

	//separate into positions
	gef::Vector4 xVector(newTran.GetRow(0));
	gef::Vector4 yVector(newTran.GetRow(1));
	gef::Vector4 zVector(newTran.GetRow(2));

	//set to relative to these axes
	gef::Vector4 newPos = newTran.GetTranslation() + xVector * relPosition.x();
	newPos += yVector * relPosition.y();
	newPos += zVector * relPosition.z();

	newTran = newTran;

	newTran.SetTranslation(newPos);

	set_transform(newTran);

	//set colliders to relative
	for(int i = 0; i < colliders.size(); i++)
		colliders.at(i)->SetTransform(newTran);
}

//the transform we want to be at relative to a marker pos
void GameObject::SetLocalRelative(gef::Vector4 newPos)
{
	//set scale
	//set rotation
	//set position
	relPosition = newPos;

	relMatrix.SetIdentity();
	relMatrix.SetTranslation(relPosition);
}


//Colliders
void GameObject::CreateCollider(ColliderPlace placement, const gef::Mesh* colliderMesh)
{
	Collider* newCol = new Collider();

	//create collider relative transform
	gef::Matrix44 colliderRelative;
	colliderRelative.SetIdentity();
	colliderRelative.Scale(gef::Vector4(0.2f, 0.2f, 0.2f));

	switch (placement)
	{
		case(top):
			colliderRelative.SetTranslation(gef::Vector4(0, 0.5, 0));
			break;
		case(right):
			colliderRelative.SetTranslation(gef::Vector4(0.5, 0, 0));
			break;
		case(bottom):
			colliderRelative.SetTranslation(gef::Vector4(0, -0.5, 0));
			break;
		case(left):
			colliderRelative.SetTranslation(gef::Vector4(-0.5, 0, 0));
			break;
	}

	//init collider
	newCol->InitCollider(colliderRelative, colliderMesh);

	colliders.push_back(newCol);
}

//Connected piece
bool GameObject::SetConnectedCollider(int nowConnected)
{
	//only allow to change state if not starting piece
	if (startingPiece)
		return false;

	//update status
	connected = nowConnected;

	//if end piece, allow for end
	if (finalPiece)
		return true;

	return false;
}