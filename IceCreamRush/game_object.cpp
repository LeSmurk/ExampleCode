#include "game_object.h"


void GameObject::UpdateFromSimulation(const b2Body* body)
{
	if (body)
	{
		// setup object rotation
		gef::Matrix44 object_rotation;
		object_rotation.RotationZ(body->GetAngle());

		// setup the object translation
		gef::Vector4 object_translation(body->GetPosition().x, body->GetPosition().y, 0.0f);

		// build object transformation matrix
		gef::Matrix44 object_transform = object_rotation;
		object_transform.SetTranslation(object_translation);
		set_transform(object_transform);
	}
}

OBJECT_TAG GameObject::GetTag()
{
	return obj_tag;
}

void GameObject::SetTag(OBJECT_TAG tag)
{
	obj_tag = tag;
}

HOLDING GameObject::GetHolding()
{
	return holdingItem;
}

void GameObject::SetHolding(HOLDING state)
{
	holdingItem = state;
}