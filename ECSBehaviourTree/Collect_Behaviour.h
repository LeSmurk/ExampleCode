#pragma once

#include "application/layers/BehaviourTree/TaskSystemHelper.h"
#include "application/layers/Tool_Timer.h"
#include "maths/maths.h"

struct CollectBehaviour
{
	entt::entity rootEntity;

	Timer timer;
	float taskTime = 4;
};

inline void UpdateCollectBehaviour(entt::registry* registry, const Timestep& timeStep)
{
	auto viewer = registry->view<CollectBehaviour>();

	for (auto entity : viewer)
	{
		// BLOCKED
		if (registry->any_of<ForceBlockTaskComponent>(entity))
		{
			// pause timer?
			continue;
		}

		CollectBehaviour& collectBehaviour = viewer.get<CollectBehaviour>(entity);

		if (!collectBehaviour.timer.GetIsRunning())
		{
			collectBehaviour.timer.SetCountdownTime(collectBehaviour.taskTime);
			collectBehaviour.timer.Start();
		}
		if (collectBehaviour.timer.GetTime() <= 0)
		{
			EndBehaviour<CollectBehaviour>(registry, entity, ConditionTypes::Success);
			continue;
		}
	}
}