#include "TaskNode.h"

//////////////// BEHAVIOUR TREES

// BLUEPRINT

TreeNodeBlueprint::TreeNodeBlueprint(uint32_t id, TreeNodeType nodeType)
	: m_nodeID(id)
	, m_nodeType(nodeType)
{
	// VALIDATE IF NOT A GOOD TYPE

}

TreeNodeBlueprint::TreeNodeBlueprint(uint32_t id, TreeNodeType nodeType, BehaviourType behaviourType)
	: m_nodeID(id)
	, m_nodeType(nodeType)
	, m_behaviourType(behaviourType)
{
	// VALIDATE IF NOT A GOOD TYPE
}

TreeNodeBlueprint::TreeNodeBlueprint(uint32_t id, TreeNodeType nodeType, BehaviourType behaviourType, BlackboardMarker blackboardMarker)
	: m_nodeID(id)
	, m_nodeType(nodeType)
	, m_behaviourType(behaviourType)
	, m_blackboardMarker(blackboardMarker)
{

	// VALIDATE IF NOT A GOOD TYPE
}

void TreeNodeBlueprint::AddBranch(TreeNodeBlueprint* node, uint32_t branchID)
{
	// force place into this branch id???
	m_branches.push_back(node);

	// tell the node we've branched into what its distance from root is
	node->SetDistanceFromRoot(m_distanceFromRoot + 1);

}

// NO CHECK DONE IF IT'S VALID ID
TreeNodeBlueprint* TreeNodeBlueprint::GetBranch(uint32_t id) const
{
	return m_branches[id];
}


// RUNTIME

TreeNodeRuntime::TreeNodeRuntime(TreeNodeBlueprint* blueprint, TreeNodeRuntime* parent)
	: m_blueprintNode(blueprint)
	, m_parentNode(parent)
{
	// If our parent is a nested node or somehting above us in the chain is nested, store its blocking status
	if (parent)
		m_isOwnerNesterBlocker = parent->GetIsOwnerNesterBlocker() || (parent->GetNodeType() == TreeNodeType::Nested && parent->GetIsBlocker());
}

TreeNodeRuntime* TreeNodeRuntime::EvaluateNextNode(ConditionTypes condition)
{
	// WILL NEED TO DO A CHECK ON THE ROOTS OF THINGS AS ONES THAT ARE NESTED WILL NO LONGER BE ROOTS IN THE SAME WAY
	// THERE WILL ALSO BE DIFFERENCES BETWEEN A NESTED ASYNC ROOT AND A LINEAR ONE
	
	switch (GetNodeType())
	{
	case(TreeNodeType::Sequence):
		return EvaluateSequenceNode(condition);

	case(TreeNodeType::Selector):
		return EvaluateSelectorNode(condition);

	case(TreeNodeType::Invertor):
		if (condition == ConditionTypes::Success)
			return m_parentNode->EvaluateNextNode(ConditionTypes::EarlyExit1);
		return m_parentNode->EvaluateNextNode(ConditionTypes::Success);

	case(TreeNodeType::Leaf):
		if (condition == ConditionTypes::Running)
			return this;
		return m_parentNode->EvaluateNextNode(condition);

	case(TreeNodeType::Instant):
		if(condition == ConditionTypes::Running)
			return this;
		return m_parentNode->EvaluateNextNode(condition);

	case(TreeNodeType::Nested):
		if (condition == ConditionTypes::Running)
			return this;
		ClearRuntimeChildren();
		return m_parentNode->EvaluateNextNode(condition);

	default:
		break;
	}

	return nullptr;
}

std::vector<uint32_t> TreeNodeRuntime::EvaluateNestedGraphIDs()
{
	std::vector<uint32_t> graphIDs;
	for (uint32_t i = 0; i < m_blueprintNode->GetBranchCount(); ++i)
	{
		graphIDs.push_back(m_blueprintNode->GetBranch(i)->GetGraphID());
	}

	return graphIDs;
}

bool TreeNodeRuntime::GetIsBlocker() const
{
	return (m_blueprintNode->GetIsBlocker() || m_isOwnerNesterBlocker);
}

bool TreeNodeRuntime::GetIsOwnerNesterBlocker() const
{
	return m_isOwnerNesterBlocker;
}


TreeNodeRuntime* TreeNodeRuntime::EvaluateSequenceNode(ConditionTypes condition)
{
	if (condition == ConditionTypes::Running)
		return EnterNewRuntimeNode(m_blueprintNode->GetBranch(m_currentBranch));

	if (condition != ConditionTypes::Success)
	{
		ClearRuntimeChildren();
		// EVALUATE PARENT NODE WITH CONDITION FAILURE
		if(m_parentNode)
			return m_parentNode->EvaluateNextNode(condition);
		
		// restart
		return EvaluateSequenceNode(ConditionTypes::Running);
	}

	if (m_currentBranch + 1 < m_blueprintNode->GetBranchCount())
	{
		// check the next branch
		++m_currentBranch;
		return EnterNewRuntimeNode(m_blueprintNode->GetBranch(m_currentBranch));
	}
	
	ClearRuntimeChildren();

	// ELSE EVALUATE PARENT NODE WITH CONDITION SUCCESS
	if(m_parentNode)
		return m_parentNode->EvaluateNextNode(condition);

	// no parent so reset (CONDITION IS NOW GOING TO LOCK US INTO SUCCESS)
	return EvaluateSequenceNode(ConditionTypes::Running);
}

TreeNodeRuntime* TreeNodeRuntime::EvaluateSelectorNode(ConditionTypes condition)
{
	if (condition == ConditionTypes::Running)
		return EnterNewRuntimeNode(m_blueprintNode->GetBranch(m_currentBranch));

	if (condition == ConditionTypes::Success)
	{
		ClearRuntimeChildren();

		if (m_parentNode)
			return m_parentNode->EvaluateNextNode(condition);

		// restart
		return EvaluateSelectorNode(ConditionTypes::Running);
	}

	// else try another
	if (m_currentBranch + 1 < m_blueprintNode->GetBranchCount())
	{
		// check the next branch
		++m_currentBranch;
		return EnterNewRuntimeNode(m_blueprintNode->GetBranch(m_currentBranch));
	}

	ClearRuntimeChildren();

	if (m_parentNode)
		return m_parentNode->EvaluateNextNode(condition);

	// no parent so reset (CONDITION IS NOW GOING TO LOCK US INTO SUCCESS)
	return EvaluateSelectorNode(ConditionTypes::Running);
}

void TreeNodeRuntime::ClearRuntimeChildren()
{
	for (TreeNodeRuntime* branch : m_runtimeBranches)
		delete branch;
	m_runtimeBranches.clear();

	m_currentBranch = 0;
}

TreeNodeRuntime* TreeNodeRuntime::EnterNewRuntimeNode(TreeNodeBlueprint* blueprint)
{
	m_runtimeBranches.push_back(new TreeNodeRuntime(blueprint, this));
	return m_runtimeBranches.back()->EvaluateNextNode(ConditionTypes::Running);
}