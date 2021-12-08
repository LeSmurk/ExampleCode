#pragma once

#include "application/layers/BehaviourTree/TaskSystemHelper.h"
#include "application/layers/BehaviourTree/TempGraphs.h"

#include "application/layers/TargetSystem.h"
#include "application/layers/Tool_Timer.h"
#include "application/timestep.hpp"
#include "entt/entity/helper.hpp"
#include "maths/maths.h"

#include "application/layers/BehaviourTree/Behaviours/Collect_Behaviour.h"
#include "application/layers/BehaviourTree/Behaviours/Goto_Behaviour.h"
#include "application/layers/BehaviourTree/Behaviours/Job_Behaviour.h"
#include "application/layers/BehaviourTree/Behaviours/Store_Behaviour.h"

///////////////////////
// ASYNC
struct AsyncTaskComponent
{
	// entity ids of children
	std::vector<entt::entity> childTrees;
};

/////////////////////
struct TaskTreeComponent
{
	entt::entity m_rootEntity;
	entt::entity m_parentEntity;

	// make root of runtime held as a unique 
	std::unique_ptr<TreeNodeRuntime> m_rootRuntimeNode;
	TreeNodeRuntime* currentLeaf = nullptr;

	uint32_t graphID = 0;
	bool isAsyncBlocker = false;

	void OnTreeInit(entt::registry* registry, entt::entity entity, entt::entity rootEntity, entt::entity parentEntity, GraphMaker* maker, uint32_t inGraphID, bool markAsBlocker = false)
	{
		m_rootEntity = rootEntity;
		m_parentEntity = parentEntity;

		graphID = inGraphID;
		m_rootRuntimeNode = std::make_unique<TreeNodeRuntime>(maker->m_graphs[graphID].m_nodes[0], nullptr);
		currentLeaf = m_rootRuntimeNode.get();
		isAsyncBlocker = markAsBlocker;

		if (maker->m_graphs[graphID].m_nodes[0]->GetIsAsync())
		{
			registry->emplace<AsyncTaskComponent>(entity);
		}
		else
		{
			registry->emplace<TaskTransitionComponent>(entity, ConditionTypes::Running);
		}

	};
};

class TaskSystem
{
public:
	TaskSystem() {};
	~TaskSystem() {};

	void SetGraphMaker(GraphMaker* maker) { g_graphMaker = maker; };

	void Update(entt::registry* registry, const Timestep& timeStep);
	void UpdateAsyncTree(entt::registry* registry, const Timestep& timeStep);
	void UpdateNestedTree(entt::registry* registry, const Timestep& timeStep);

	void AddBehaviourComponent(entt::registry* registry, entt::entity entity, BehaviourType behaviour, entt::entity rootEntity, entt::entity parentEntity, BlackboardMarker blackboardMarker);

private:

	// Holding onto the graph maker - SHOULD TIDY THIS
	GraphMaker* g_graphMaker = nullptr;
};