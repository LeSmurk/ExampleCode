#pragma once

#include "application/layers/BehaviourTree/TaskSystemHelper.h"
#include "application/layers/Tool_Timer.h"
#include "maths/maths.h"

struct StoreBehaviour
{
	entt::entity rootEntity;

	Timer timer;
	float taskTime = 2;
};

inline void UpdateStoreBehaviour(entt::registry* registry, const Timestep& timeStep)
{
	auto viewer = registry->view<StoreBehaviour>();

	for (auto entity : viewer)
	{
		// BLOCKED
		if (registry->any_of<ForceBlockTaskComponent>(entity))
		{
			// pause timer?
			continue;
		}

		StoreBehaviour& storeBehaviour = viewer.get<StoreBehaviour>(entity);

		if (!storeBehaviour.timer.GetIsRunning())
		{
			storeBehaviour.timer.SetCountdownTime(storeBehaviour.taskTime);
			storeBehaviour.timer.Start();
		}
		if (storeBehaviour.timer.GetTime() <= 0)
		{
			EndBehaviour<StoreBehaviour>(registry, entity, ConditionTypes::Success);
			continue;
		}
	}
}