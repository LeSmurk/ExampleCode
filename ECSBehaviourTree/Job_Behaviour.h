#pragma once

#include "application/layers/BehaviourTree/TaskSystemHelper.h"
#include "application/layers/Tool_Timer.h"

struct JobBehaviour
{
	entt::entity parentEntity;

	// sit and look for a job on the queue
	Timer timer;
	float taskTime = 8;
};

inline void UpdateJobBehaviour(entt::registry* registry, const Timestep& timeStep)
{
	// pull a job off the queue, if one valid. If not then just wait

	// Once we have a job, push the graph ID to the parent entity
	// The parent entity will then have to check its nested types if they want the graph id pushed to them

	auto viewer = registry->view<JobBehaviour>();

	for (auto entity : viewer)
	{
		// BLOCKED
		if (registry->any_of<ForceBlockTaskComponent>(entity))
		{
			// pause timer?
			continue;
		}

		JobBehaviour& jobBehaviour = viewer.get<JobBehaviour>(entity);

		// wait 10 seconds then push
		if (!jobBehaviour.timer.GetIsRunning())
		{
			jobBehaviour.timer.SetCountdownTime(jobBehaviour.taskTime);
			jobBehaviour.timer.Start();
		}
		if (jobBehaviour.timer.GetTime() <= 0)
		{
			// TODO - Make the job storage system so we can pop a graph ID off of it, for now it's just taking a hardcoded value
			uint32_t graphID = 0;
			registry->emplace<NestedGraphPusherComponent>(entity, graphID);
			EndBehaviour<JobBehaviour>(registry, entity, ConditionTypes::Success);
		}
	}
}