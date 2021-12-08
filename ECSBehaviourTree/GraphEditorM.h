#pragma once

#include "application/timestep.hpp"
#include "core/core.h"
#include "entt/entt.hpp"
#include "events/event.h"
#include "ui/ui_window.h"
#include "ImNodesEz.h"
#include "TempGraphs.h"

static std::string GetBehaviourToName(BehaviourType behaviour)
{
    switch (behaviour)
    {
    case BehaviourType::Go_To:
        return "GoTo";
    case BehaviourType::Collect:
        return "Collect";
    case BehaviourType::Store:
        return "Store";
    case BehaviourType::JobQueue:
        return "JobSearch";
    default:
        return "Name";
    }
}

static std::string GetNodeTypeToName(TreeNodeType type)
{
    switch (type)
    {
    case TreeNodeType::Sequence:
        return "Sequence";
    case TreeNodeType::Selector:
        return "Selector";
    case TreeNodeType::Invertor:
        return "Invertor";
    case TreeNodeType::Leaf:
        return "Leaf";
    case TreeNodeType::Instant:
        return "Instant";
    case TreeNodeType::Nested:
        return "Nested";
    default:
        return "Name";
    }
}

struct DebugMarker
{
    bool isFileLoaded = false;
};

struct Connection
{
    void* inputNode;
    const char* inputSlot;
    void* outputNode;
    const char* outputSlot;

    bool operator==(const Connection& other) const
    {
        return inputNode == other.inputNode &&
            inputSlot == other.inputSlot &&
            outputNode == other.outputNode &&
            outputSlot == other.outputSlot;
    }

    bool operator!=(const Connection& other) const
    {
        return !operator ==(other);
    }
};


struct Node
{
    Node(ImVec2 inPos, bool isSelected, std::string inName)
    {
        name = inName;
        pos = inPos;
        selected = isSelected;
    };
    Node(ImVec2 inPos, bool isSelected, std::vector<ImNodes::Ez::SlotInfo> inInputs, std::vector<ImNodes::Ez::SlotInfo> inOutputs, std::vector<Connection> inConnections)
    {
        pos = inPos;
        selected = isSelected;
        inputs = inInputs;
        outputs = inOutputs;
        connections = inConnections;
    };
    ~Node()
    {
        for (std::string* name : inputNames)
            delete name;
        inputNames.clear();

        for (std::string* name : outputNames)
            delete name;
        outputNames.clear();
    }

    std::string name = "Null";
    ImVec2 pos{};
    bool selected{};
    bool isHighlighting = false;
    bool isLoaded = false;
    std::vector<ImNodes::Ez::SlotInfo> inputs;
    std::vector<ImNodes::Ez::SlotInfo> outputs;

    std::vector<std::string*> inputNames;
    std::vector<std::string*> outputNames;

    std::vector<Connection> connections;

    void DeleteConnection(const Connection& connection)
    {
        auto it = std::find(connections.begin(), connections.end(), connection);

        if (it != connections.end())
            connections.erase(it);
    }

    void ChangeInputConnection(const Connection& connection)
    {
        // remove at input 
        auto it = std::find_if(connections.begin(), connections.end(), [connection](Connection x) { return x.inputSlot == connection.inputSlot; });
        if (it != connections.end())
        {
            static_cast<Node*>(connection.outputNode)->DeleteConnection(connection);
            connections.erase(it);
        }

        connections.push_back(connection);
    }

    void ChangeOutputConnection(const Connection& connection)
    {
        // remove at output pin
        auto it = std::find_if(connections.begin(), connections.end(), [connection](Connection x) { return x.outputSlot == connection.outputSlot; });
        if (it != connections.end())
        {
            static_cast<Node*>(connection.inputNode)->DeleteConnection(connection);
            connections.erase(it);
        }

        connections.push_back(connection);
    }
};

class GraphEditorM
	: public Qualia::BaseUIWindow
{
private:
	bool m_isOpen = true;
	ImGuiWindowFlags m_windowFlags;
public:
	virtual void Init(void* data = nullptr) override;
	virtual void Update(const Timestep& timeStep) override;
	virtual void OnEvent(Qualia::Event& event) override;

	void SetRegistry(entt::registry* registry) { m_registry = registry; };


private:
	entt::registry* m_registry;
    GraphMaker* m_maker = nullptr;
	std::vector<Node*> m_nodes;


	// create, load and save a blueprint

	// pick an entity from list (ones with tree component)
	// pulls up its blueprint to display
	// adds in a little highlight to show the current task
	// shows which branches have been covered?
    void DisplayEntitySelect();
    bool m_entitySelectOpen = false;
    entt::entity m_currentlySelectedEntity;

	void UpdateRuntimeVisuals();
    void HighlightLoadedRuntimeRecursive(TreeNodeRuntime* node);


	void OpenGraph(uint32_t graphID);
	void CloseGraph();
};