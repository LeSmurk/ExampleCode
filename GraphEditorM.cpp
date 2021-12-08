#include "GraphEditorM.h"

#include "entt/entity/helper.hpp"
#include "logging/log.h"
#include "graphics/renderer.h"

#include "graphics/render_commands.h"

#include <glm/gtc/type_ptr.hpp>
#include "input/input.h"
#include "TaskSystem.h"


void GraphEditorM::Init(void* data)
{
	m_windowFlags = ImGuiWindowFlags_None;
	m_windowFlags |= ImGuiWindowFlags_UnsavedDocument;

    m_maker = static_cast<GraphMaker*>(data);
}

void GraphEditorM::Update(const Timestep& timestep)
{
    auto& style = ImGui::GetStyle();

    DisplayEntitySelect();

    UpdateRuntimeVisuals();

   if (ImGui::Begin("ImNodes", nullptr, ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoScrollWithMouse))
   {
       if (ImGui::Button("Select Entity To Target"))
       {
           m_entitySelectOpen = true;
       }

       static ImNodes::CanvasState canvas;

       ImNodes::BeginCanvas(&canvas);

       for (Node* node : m_nodes)
       {
           if (node->isHighlighting)
               ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(220, 0, 0, 255));
           if(node->isLoaded)
               canvas.Colors[ImNodes::StyleColor::ColNodeBg] = ImColor(0, 50, 0);

           if (ImNodes::Ez::BeginNode(node, node->name.c_str(), &node->pos, &node->selected))
           {
               ImNodes::Ez::InputSlots(node->inputs.data(), node->inputs.size());
               ImNodes::Ez::OutputSlots(node->outputs.data(), node->outputs.size());

               for (Connection& connection : node->connections)
               {
                   if (static_cast<Node*>(connection.outputNode) != node)
                       continue;
                   //ImNodes::Connection(connection.inputNode, "In", connection.outputNode, "Out");

                   if (!ImNodes::Connection(connection.inputNode, connection.inputSlot, connection.outputNode,
                       connection.outputSlot))
                   {
                       // Remove deleted connections
                       static_cast<Node*>(connection.inputNode)->DeleteConnection(connection);
                       static_cast<Node*>(connection.outputNode)->DeleteConnection(connection);
                   }
               }

               // Delete
               if (node->selected && Qualia::InputHandler::IsKeyPressed(Keys::C))
               {
                   for (Connection& connection : node->connections)
                   {
                       if (static_cast<Node*>(connection.inputNode) == node)
                           static_cast<Node*>(connection.outputNode)->DeleteConnection(connection);

                       else if (static_cast<Node*>(connection.outputNode) == node)
                           static_cast<Node*>(connection.inputNode)->DeleteConnection(connection);
                   }

                   node->connections.clear();
                   delete &node;
               }
           }
           ImNodes::Ez::EndNode();

           if (node->isHighlighting)
               ImGui::PopStyleColor();
           if(node->isLoaded)
               canvas.Colors[ImNodes::StyleColor::ColNodeBg] = ImColor(0.1f, 0.1f, 0.1f);
       }

       // New
       Connection newConnection;
       if (ImNodes::GetNewConnection(&newConnection.inputNode, &newConnection.inputSlot,
           &newConnection.outputNode, &newConnection.outputSlot))
       {
           //node.connections.push_back(newConnection);
           static_cast<Node*>(newConnection.inputNode)->ChangeInputConnection(newConnection);
           static_cast<Node*>(newConnection.outputNode)->ChangeOutputConnection(newConnection);
       }

       ImNodes::EndCanvas();
   }
   ImGui::End();
}

void GraphEditorM::OnEvent(Qualia::Event& event)
{

}

/// /////////////

void GraphEditorM::DisplayEntitySelect()
{
    if (m_entitySelectOpen)
    {
        if (ImGui::Begin("Entity Select", nullptr, ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoScrollWithMouse))
        {
            if (ImGui::Button("Close Selector"))
                m_entitySelectOpen = false;

            ImGui::NewLine();

            // pick an entity and force isselected for its nodes
            auto listViewer = m_registry->view<const TaskTreeComponent>();
            for (auto entity : listViewer)
            {
                // this is the one already selected
                if (m_registry->any_of<DebugMarker>(entity))
                {
                    continue;
                }

                uint32_t entityNum = static_cast<uint32_t>(entity);
                ImGui::Text(std::to_string(entityNum).c_str());
                ImGui::SameLine();
                ImGui::PushID(entityNum);
                if (ImGui::Button("Select Entity"))
                {
                    m_registry->emplace<DebugMarker>(entity);
                    // remove from previous
                    if (m_registry->valid(m_currentlySelectedEntity) && m_registry->any_of<DebugMarker>(m_currentlySelectedEntity))
                    {
                        m_registry->remove<DebugMarker>(m_currentlySelectedEntity);
                        CloseGraph();
                    }
                    m_currentlySelectedEntity = entity;

                    //open file
                    const TaskTreeComponent& taskTreeComp = listViewer.get<const TaskTreeComponent>(entity);
                    OpenGraph(taskTreeComp.graphID);
                }
                ImGui::PopID();
            }
        }
        ImGui::End();
    }
}

void GraphEditorM::UpdateRuntimeVisuals()
{
    // pick an entity and force isselected for its nodes
    auto listViewer = m_registry->view<TaskTreeComponent, DebugMarker>();
    for (auto entity : listViewer)
    {
        TaskTreeComponent& taskTreeComp = listViewer.get<TaskTreeComponent>(entity);

        // figure out which node we're targeting currently
        if (TreeNodeRuntime* node = taskTreeComp.currentLeaf)
        {
            // reset all other highlights
            for (Node* node : m_nodes)
                node->isHighlighting = false;

            uint32_t id = node->GetNodeID();
            if (id < m_nodes.size())
                m_nodes[id]->isHighlighting = true;

            // get to the root of the current runtime graph
            TreeNodeRuntime* rootNode = node;
            while (TreeNodeRuntime* nextParent = rootNode->GetRuntimeParentNode())
                rootNode = nextParent;

            // reset loaded highlight
            for (Node* node : m_nodes)
                node->isLoaded = false;

            HighlightLoadedRuntimeRecursive(rootNode);
        }
    }
}

void GraphEditorM::HighlightLoadedRuntimeRecursive(TreeNodeRuntime* node)
{
    for (TreeNodeRuntime*& nextNode : node->GetActiveRuntimeBranches())
    {
        HighlightLoadedRuntimeRecursive(nextNode);
    }

    uint32_t id = node->GetNodeID();
    if (id < m_nodes.size())
        m_nodes[id]->isLoaded = true;
}

void GraphEditorM::OpenGraph(uint32_t graphID)
{
    // Init nodes from storage
    const uint32_t ySeparation = 50;
    const uint32_t xSeparation = 300;
    uint32_t newPos = 0;

    for (TreeNodeBlueprint* blueprint : m_maker->m_graphs[graphID].m_nodes)
    {
        std::string name = blueprint->GetBehaviourType() != BehaviourType::COUNT ? GetBehaviourToName(blueprint->GetBehaviourType()) : GetNodeTypeToName(blueprint->GetNodeType());

        // set position based on how far from the root it is
        Node* newNode = new Node(ImVec2(xSeparation * blueprint->GetDistanceFromRoot(), newPos += ySeparation), false, name);

        newNode->inputNames.push_back(new std::string("In"));
        newNode->inputs.push_back(ImNodes::Ez::SlotInfo{ newNode->inputNames.back()->c_str(), 1 });

        for (int x = 0; x < static_cast<int>(blueprint->GetBranchCount()); ++x)
        {
            // Store output names so that the c_str given to SlotInfo doesn't go out of scope
            newNode->outputNames.push_back(new std::string("Out" + std::to_string(x+1)));
            // we use x+1 as a slot ID of 0 isn't allowed
            newNode->outputs.push_back(ImNodes::Ez::SlotInfo{ newNode->outputNames.back()->c_str(), x+1 });
        }
        m_nodes.push_back(newNode);
    }

    // Create the connections properly
    for (uint32_t i = 0; i < m_nodes.size(); ++i)
    {
        Node* node = m_nodes[i];
        TreeNodeBlueprint* blueprint = m_maker->m_graphs[graphID].m_nodes[i];
        for (uint32_t x = 0; x < blueprint->GetBranchCount(); ++x)
        {
            //auto it = std::find(m_maker->m_behaviourBlueprints.begin(), m_maker->m_behaviourBlueprints.end(), blueprint->GetBranch(x));
            uint32_t nodeID = blueprint->GetBranch(x)->GetNodeID(); //std::distance(m_maker->m_behaviourBlueprints.begin(), it);

            Node* targetNode = m_nodes[nodeID];
            // readjust the id of in>out
            targetNode->inputs[0].kind = node->outputs[x].kind;

            // Create new connection between the nodes
            Connection newConnection({targetNode, targetNode->inputs[0].title, node, node->outputs[x].title});
            targetNode->ChangeInputConnection(newConnection);
            node->ChangeOutputConnection(newConnection);
        }
    }
}

void GraphEditorM::CloseGraph()
{
    for (Node* node : m_nodes)
        delete node;

    m_nodes.clear();
}