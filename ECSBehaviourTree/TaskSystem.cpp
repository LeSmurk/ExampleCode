#include "TaskSystem.h"

#include "logging/log.h"

//#include "application/layers/TargetSystem.h"

// Have ConditionExitType so we can have a regular out and then branches to define early out
// anything not within the regular out would be a failure of the task and it would have to go back up the tree
// specify within the task node stuff if its a behaviour one so we can make actual behaviour trees instead

void TaskSystem::Update(entt::registry* registry, const Timestep& timeStep)
{
	auto listViewer = registry->view<TaskTreeComponent, TaskTransitionComponent>();		
	for (auto entity : listViewer)
	{
		TaskTreeComponent& taskTreeComp = listViewer.get<TaskTreeComponent>(entity);
		TaskTransitionComponent& transitionComp = listViewer.get<TaskTransitionComponent>(entity);

		if (TreeNodeRuntime* node = taskTreeComp.currentLeaf)
		{
			taskTreeComp.currentLeaf = node->EvaluateNextNode(transitionComp.condition);
			if (taskTreeComp.currentLeaf == nullptr)
			{
				Q_LOG_ERROR("TaskSystem: following leaf was null for entity %u", entity);
				continue;
			}

			if (taskTreeComp.isAsyncBlocker || taskTreeComp.currentLeaf->GetIsBlocker())
			{
				if (!registry->any_of<BlockingTaskComponent>(entity))
					registry->emplace<BlockingTaskComponent>(entity);
			}
			else if(registry->any_of<BlockingTaskComponent>(entity))
			{
				registry->remove<BlockingTaskComponent>(entity);
			}

			AddBehaviourComponent(registry, entity, taskTreeComp.currentLeaf->GetBehaviourType(), taskTreeComp.m_rootEntity, taskTreeComp.m_parentEntity, taskTreeComp.currentLeaf->GetBlackboardMarker());

			if(registry->any_of<TaskTransitionComponent>(entity))
				registry->remove<TaskTransitionComponent>(entity);			
		}
	}

	UpdateAsyncTree(registry, timeStep);
	UpdateNestedTree(registry, timeStep);

	// Should tidy so we can properly schedule these
	UpdateGotoBehaviour(registry, timeStep);
	UpdateCollectBehaviour(registry, timeStep);
	UpdateStoreBehaviour(registry, timeStep);
	UpdateJobBehaviour(registry, timeStep);
}

void TaskSystem::UpdateAsyncTree(entt::registry* registry, const Timestep& timeStep)
{	
	auto viewer = registry->view<TaskTreeComponent, AsyncTaskComponent>();
	auto nestedGraphPusher = registry->view<NestedGraphPusherComponent>();

	for (auto entity : viewer)
	{
		TaskTreeComponent& taskTreeComp = viewer.get<TaskTreeComponent>(entity);
		AsyncTaskComponent& asyncComp = viewer.get<AsyncTaskComponent>(entity);

		// if not init, run first time setup
		if (asyncComp.childTrees.size() == 0)
		{		
			// spin up trees from branches
			uint32_t createdBranches = 0;
			std::vector<uint32_t> graphIDs = taskTreeComp.currentLeaf->EvaluateNestedGraphIDs();
			while (createdBranches != graphIDs.size())
			{
				// CHECK NEW GRAPH ID IS A VALID GRAPH
				entt::entity newEntity = registry->create();
				TaskTreeComponent& newTreeComp = registry->emplace<TaskTreeComponent>(newEntity);

				// IS THIS BLOCKER IS A BIT MESSY?
				newTreeComp.OnTreeInit(registry, newEntity, taskTreeComp.m_rootEntity, entity, g_graphMaker, graphIDs[createdBranches], taskTreeComp.currentLeaf->GetIsBranchBlocker(createdBranches));
				asyncComp.childTrees.push_back(newEntity);

				++createdBranches;
			}
		}

		for (uint32_t i = 0; i < asyncComp.childTrees.size(); ++i)
		{
			entt::entity& child = asyncComp.childTrees[i];
			if (registry->any_of<BlockingTaskComponent>(child))
			{
				// set blocking on subsequent children
				for (uint32_t x = i + 1; x < asyncComp.childTrees.size(); ++x)
				{
					if(!registry->any_of<ForceBlockTaskComponent>(asyncComp.childTrees[x]))
						registry->emplace<ForceBlockTaskComponent>(asyncComp.childTrees[x]);
				}
			}
			// remove blocking on subsequent children (we know if this is the root cause of blocking if this child is not force blocked)
			else if(!registry->any_of<ForceBlockTaskComponent>(child))
			{
				for (uint32_t x = i + 1; x < asyncComp.childTrees.size(); ++x)
				{
					if (registry->any_of<ForceBlockTaskComponent>(asyncComp.childTrees[x]))
						registry->remove<ForceBlockTaskComponent>(asyncComp.childTrees[x]);
				}	
			}
		}
	}
}

void TaskSystem::UpdateNestedTree(entt::registry* registry, const Timestep& timeStep)
{
	auto viewer = registry->view<TaskTreeComponent, NestedGraphPusherComponent>();

	for (auto entity : viewer)
	{
		TaskTreeComponent& taskTreeComp = viewer.get<TaskTreeComponent>(entity);
		NestedGraphPusherComponent& nestedPusherComp = viewer.get<NestedGraphPusherComponent>(entity);

		if (taskTreeComp.currentLeaf->GetNodeType() != TreeNodeType::Nested)
			continue;

		taskTreeComp.currentLeaf = taskTreeComp.currentLeaf->EnterNewRuntimeNode(g_graphMaker->m_graphs[nestedPusherComp.graphID].m_nodes[0]);
		registry->remove<NestedGraphPusherComponent>(entity);

		if (taskTreeComp.isAsyncBlocker || taskTreeComp.currentLeaf->GetIsBlocker())
		{
			if (!registry->any_of<BlockingTaskComponent>(entity))
				registry->emplace<BlockingTaskComponent>(entity);
		}
		else if (registry->any_of<BlockingTaskComponent>(entity))
		{
			registry->remove<BlockingTaskComponent>(entity);
		}

		AddBehaviourComponent(registry, entity, taskTreeComp.currentLeaf->GetBehaviourType(), taskTreeComp.m_rootEntity, taskTreeComp.m_parentEntity, taskTreeComp.currentLeaf->GetBlackboardMarker());
	}
}

void TaskSystem::AddBehaviourComponent(entt::registry* registry, entt::entity entity, BehaviourType behaviour, entt::entity rootEntity, entt::entity parentEntity, BlackboardMarker blackboardMarker)
{
	switch (behaviour)
	{
	case BehaviourType::Go_To:
		if (!registry->any_of<GotToBehaviour>(entity))
			registry->emplace<GotToBehaviour>(entity, rootEntity, blackboardMarker);
		break;
	case BehaviourType::Collect:
		if (!registry->any_of<CollectBehaviour>(entity))
			registry->emplace<CollectBehaviour>(entity, rootEntity);
		break;
	case BehaviourType::Store:
		if (!registry->any_of<StoreBehaviour>(entity))
			registry->emplace<StoreBehaviour>(entity, rootEntity);
		break;
	case BehaviourType::JobQueue:
		if (!registry->any_of<JobBehaviour>(entity))
			registry->emplace<JobBehaviour>(entity, parentEntity);
		break;
	default:
		break;
	}
}