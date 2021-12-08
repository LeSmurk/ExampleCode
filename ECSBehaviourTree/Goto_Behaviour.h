#pragma once

#include "application/layers/BehaviourTree/TaskSystemHelper.h"
#include "application/layers/Tool_Timer.h"
#include "ecs/component.h"
#include "maths/maths.h"

// THIS WOULD BE MOVED TO A TOOLS SYSTEM OR NOT EXIST AT ALL
static Vector3f CalculateRandomPos()
{
	const int xMax = 16;
	const int yMax = 8;

	float randX = (rand() % (xMax * 2)) - xMax;
	float randY = (rand() % (yMax * 2)) - yMax;
	return Vector3f(randX, randY, 0.0f);
}

struct GotToBehaviour
{
	entt::entity rootEntity;

	BlackboardMarker blackboardMarker = BlackboardMarker::COUNT;
	Vector3f targetPos{ 0, 0, 0 };
	float acceptableDistance = 2;
	float walkSpeed = 2;
	Timer timer;
	float taskTime = 10;
};

inline void UpdateGotoBehaviour(entt::registry* registry, const Timestep& timeStep)
{
	auto viewer = registry->view<GotToBehaviour>();
	auto parentViewer = registry->view<STransformComponent>();

	for (auto entity : viewer)
	{
		// BLOCKED
		if (registry->any_of<ForceBlockTaskComponent>(entity))
		{
			// pause timer?
			continue;
		}

		GotToBehaviour& gotoBehaviour = viewer.get<GotToBehaviour>(entity);

		if (!gotoBehaviour.timer.GetIsRunning())
		{
			gotoBehaviour.timer.SetCountdownTime(gotoBehaviour.taskTime);
			gotoBehaviour.timer.Start();

			// pick a random position
			if(Blackboard::GetBlackboardValue(gotoBehaviour.blackboardMarker, gotoBehaviour.targetPos) == false)
				gotoBehaviour.targetPos = CalculateRandomPos();
		}
		if (gotoBehaviour.timer.GetTime() <= 0)
		{
			EndBehaviour<GotToBehaviour>(registry, entity, ConditionTypes::EarlyExit1);
			continue;
		}

		STransformComponent& tranComp = registry->valid(gotoBehaviour.rootEntity) ? parentViewer.get<STransformComponent>(gotoBehaviour.rootEntity) : parentViewer.get<STransformComponent>(entity);

		// move dir
		Vector3f resultant = gotoBehaviour.targetPos - tranComp.Position;
		tranComp.Position += glm::normalize(resultant) * (gotoBehaviour.walkSpeed * timeStep);

		if (glm::length2(resultant) <= gotoBehaviour.acceptableDistance)
		{
			EndBehaviour<GotToBehaviour>(registry, entity, ConditionTypes::Success);
		}
	}
}