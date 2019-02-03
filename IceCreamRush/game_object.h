#ifndef _GAME_OBJECT_H
#define _GAME_OBJECT_H

#include <graphics/mesh_instance.h>
#include <box2d/Box2D.h>

enum OBJECT_TAG
{
	DEFAULT,
	PLAYER,
	ICECREAM,
	CONE,
	SCOOP
};

enum FLAVOUR_TAG
{
	NONE,
	CHOC,
	STRA,
	MINT,
	VANI
};

enum HOLDING
{
	EMPTY,
	GRABBING,
	HELD,
	STORED
};


class GameObject : public gef::MeshInstance
{
public:
	void UpdateFromSimulation(const b2Body* body);

	OBJECT_TAG GetTag();
	void SetTag(OBJECT_TAG tag);

	HOLDING GetHolding();
	void SetHolding(HOLDING state);

	OBJECT_TAG obj_tag = DEFAULT;
	HOLDING holdingItem = EMPTY;

	//determine which hand
	bool handRight;
};

#endif // _GAME_OBJECT_H