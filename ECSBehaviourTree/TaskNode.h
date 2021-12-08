#pragma once

#include "Blackboard.h"
#include "entt/entity/helper.hpp"
#include "maths/maths.h"

// Should change this as is older stuff for FSM
enum class ConditionTypes
{
	Success,
	Running,
	EarlyExit1,
	EarlyExit2,
	EarlyExit3,
	COUNT
};


// BEHAVIOUR TREES

// REQUIRE THAT ALL BEHAVIOUR GRAPHS ARE REEXPORTED IF THIS IS MODIFIED FOR NOW
enum class BehaviourType
{
	Go_To,
	Collect,
	Store,
	JobQueue,
	COUNT
};

// to ADD - Always succeed
// to ADD - if fail set property
// REQUIRE THAT ALL BEHAVIOUR GRAPHS ARE REEXPORTED IF THIS IS MODIFIED FOR NOW
enum class TreeNodeType
{
	Sequence,
	Selector,
	Invertor,
	Leaf,
	Instant,
	Nested,
	COUNT
};

// This is the implementation of the data
class TreeNodeBlueprint
{
public:
	TreeNodeBlueprint(uint32_t nodeID, TreeNodeType nodeType);
	TreeNodeBlueprint(uint32_t nodeID, TreeNodeType nodeType, BehaviourType behaviourType);
	TreeNodeBlueprint(uint32_t nodeID, TreeNodeType nodeType, BehaviourType behaviourType, BlackboardMarker blackboardMarker);

	void SetGraphID(uint32_t value) { m_graphID = value; };
	uint32_t GetGraphID() const { return m_graphID; };
	// The ID of this node within its graph
	uint32_t GetNodeID() const { return m_nodeID; };

	void AddBranch(TreeNodeBlueprint* node, uint32_t branchID);
	uint32_t GetBranchCount() const { return m_branches.size(); };
	TreeNodeBlueprint* GetBranch(uint32_t id) const;
	TreeNodeType GetNodeType() const { return m_nodeType; };
	BehaviourType GetBehaviourType() const { return m_behaviourType; };

	BlackboardMarker GetBlackboardMarker() const { return m_blackboardMarker; };
	void SetBlackboardMarker(BlackboardMarker value) { m_blackboardMarker = value; };

	uint32_t GetDistanceFromRoot() const { return m_distanceFromRoot; };
	void SetDistanceFromRoot(uint32_t id) { m_distanceFromRoot = id; };

	bool GetIsRoot() const { return m_distanceFromRoot == 0; };

	bool GetIsAsync() const { return m_isAsync; };
	void SetIsAsync(bool value) { m_isAsync = value; };
	bool GetIsBlocker() const { return m_isBlocker; };
	void SetIsBlocker(bool value) { m_isBlocker = value; };

private:
	uint32_t m_graphID = 0;
	uint32_t m_nodeID = 0;
	TreeNodeType m_nodeType = TreeNodeType::COUNT;
	BehaviourType m_behaviourType = BehaviourType::COUNT;

	BlackboardMarker m_blackboardMarker = BlackboardMarker::COUNT;

	bool m_isAsync = false;
	bool m_isBlocker = false;

	std::vector<TreeNodeBlueprint*> m_branches;

	uint32_t m_distanceFromRoot = 0;
};

// This is the copy of the blueprint, that needs to hold the data to operate the tree
class TreeNodeRuntime
{
public:
	TreeNodeRuntime(TreeNodeBlueprint* blueprint, TreeNodeRuntime* parent);
	~TreeNodeRuntime() { ClearRuntimeChildren(); };

	uint32_t GetGraphID() const { return m_blueprintNode->GetGraphID(); };
	uint32_t GetNodeID() const { return m_blueprintNode->GetNodeID(); };

	TreeNodeRuntime* EvaluateNextNode(ConditionTypes condition);
	std::vector<uint32_t> EvaluateNestedGraphIDs();
	TreeNodeType GetNodeType() const { return m_blueprintNode->GetNodeType(); };
	BehaviourType GetBehaviourType() const { return m_blueprintNode->GetBehaviourType(); };
	BlackboardMarker GetBlackboardMarker() const { return m_blueprintNode->GetBlackboardMarker(); };
	TreeNodeRuntime* GetRuntimeParentNode() { return m_parentNode; };

	bool GetIsBlocker() const;
	bool GetIsOwnerNesterBlocker() const;
	// NO CHECK DONE IF IT'S VALID ID
	bool GetIsBranchBlocker(uint32_t id) const { return m_blueprintNode->GetBranch(id)->GetIsBlocker(); };

	TreeNodeRuntime* EnterNewRuntimeNode(TreeNodeBlueprint* blueprintNode);

	// This getter should only be used for tools to determine which parts of the tree are loaded
	std::vector<TreeNodeRuntime*>& GetActiveRuntimeBranches() { return m_runtimeBranches; };

private:
	TreeNodeRuntime* EvaluateSequenceNode(ConditionTypes condition);
	TreeNodeRuntime* EvaluateSelectorNode(ConditionTypes condition);
	// invertor change condition return and make sure only one child

	void ClearRuntimeChildren();
	std::vector<TreeNodeRuntime*> m_runtimeBranches;

	TreeNodeRuntime* m_parentNode = nullptr;
	TreeNodeBlueprint* m_blueprintNode = nullptr;
	
	// what branch I'm currently on
	uint32_t m_currentBranch = 0;
	// what branches I have done
	std::vector<uint32_t> m_testedBranches;

	// The parent that controls us, only available if we are nested
	bool m_isOwnerNesterBlocker = false;
};