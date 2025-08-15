//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Esper.SkillTree
{
    /// <summary>
    /// A skill graph contains data for all skills and skill connections.
    /// </summary>
    public class SkillGraph : ScriptableObject
    {
        /// <summary>
        /// The skill graphs directory. Works in the editor only.
        /// </summary>
        public static string directoryPath = Path.Combine("Assets", "StylishEsper", "SkillTree", "Resources", "SkillTree", "SkillGraphs");

        /// <summary>
        /// If this skill graph is enabled. Skill graphs must be enabled to be visible in-game.
        /// </summary>
        public bool enabled;

        /// <summary>
        /// If this skill graph is for the player.
        /// </summary>
        public bool forPlayer;

        /// <summary>
        /// The ID of the skill graph. This is best left unchanged.
        /// </summary>
        public int id;

        /// <summary>
        /// The display name of the skill graph.
        /// </summary>
        public string displayName;

        /// <summary>
        /// The grid dimensions. This will control the number of initial graph skills.
        /// </summary>
        public Vector2Int gridDimensions;

        /// <summary>
        /// The size of each node.
        /// </summary>
        public float nodeSize;

        /// <summary>
        /// The main shape of all nodes.
        /// </summary>
        public SkillNode.Shape nodeShape;

        /// <summary>
        /// The visibility of the graph.
        /// </summary>
        public GraphVisibility visibility;

        /// <summary>
        /// A custom object for any purpose.
        /// </summary>
        public Object customObject;

        /// <summary>
        /// A list of nodes in this graph.
        /// </summary>
        public List<SkillNode> nodes = new();

        /// <summary>
        /// A list of skill connections in this graph.
        /// </summary>
        public List<SkillConnection> connections = new();

        /// <summary>
        /// The amount of nodes that should exist.
        /// </summary>
        public int NodeCount { get => gridDimensions.x * gridDimensions.y; }

        /// <summary>
        /// Calculates the number of points that would be required to completely upgrade all skills.
        /// </summary>
        /// <returns>The amount of points that would be required to completely upgrade all skills.</returns>
        public int CountTotalPoints()
        {
            int total = 0;

            foreach (var node in nodes)
            {
                if (node.IsValid())
                {
                    total += node.maxLevel;
                }
            }

            return total;
        }

        /// <summary>
        /// Calculates the total number of points spent in this graph.
        /// </summary>
        /// <returns>The total number of points spent.</returns>
        public int CountPointsSpent()
        {
            int total = 0;

            foreach (var node in nodes)
            {
                if (node.IsValid())
                {
                    total += node.currentLevel;
                }
            }

            return total;
        }

        /// <summary>
        /// Resets all node states.
        /// </summary>
        public void ResetNodeStates()
        {
            foreach (var node in nodes)
            {
                node.currentLevel = 0;
                node.state = SkillNode.State.Locked;
            }
        }

        /// <summary>
        /// Gets a node by position index.
        /// </summary>
        /// <param name="positionIndex">The node's position index.</param>
        /// <returns>The node with the index or null if it doesn't exist.</returns>
        public SkillNode GetNodeByPosition(int positionIndex)
        {
            foreach (var node in nodes)
            {
                if (node.positionIndex == positionIndex)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a node by index.
        /// </summary>
        /// <param name="index">The node index.</param>
        /// <returns>The node with the index or null if it doesn't exist.</returns>
        public SkillNode GetNodeByIndex(int index)
        {
            if (index > nodes.Count - 1 || index < 0)
            {
                return null;
            }

            return nodes[index];
        }

        /// <summary>
        /// Gets a node by key.
        /// </summary>
        /// <param name="key">The node key.</param>
        /// <returns>The node with the key or null if it doesn't exist.</returns>
        public SkillNode GetNodeByKey(string key)
        {
            foreach (var node in nodes)
            {
                if (node.key == key)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first available position index.
        /// </summary>
        /// <returns>The first available position index or -1 if no position is available.</returns>
        public int GetFirstAvailablePositionIndex()
        {
            for (int i = 0; i < NodeCount; i++)
            {
                bool exists = false;

                foreach (var node in nodes)
                {
                    if (node.positionIndex == i)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Creates and adds an empty skill node.
        /// </summary>
        /// <param name="positionIndex">The position index of the skill node.</param>
        public SkillNode AddEmptySkillNode(int positionIndex)
        {
            var skillNode = SkillNode.Create(positionIndex.ToString(), positionIndex, this);
            nodes.Add(skillNode);
            Save();
            return skillNode;
        }

        /// <summary>
        /// Tries to unlock locked nodes.
        /// </summary>
        public void TryUnlockLockedNodes()
        {
            foreach (var node in nodes)
            {
                if (node.IsLocked)
                {
                    node.TryUnlock();
                }
            }
        }

        /// <summary>
        /// Depletes all nodes which don't meet the requirements of remaining in the
        /// obtained state.
        /// </summary>
        public void DepleteNodesWhereNecessary()
        {
            foreach (var node in nodes)
            {
                // When comparing counting points spent, the current node's upgrade count is ignored so it isn't counted towards
                // the total spent in the tree
                if (node.IsObtained && (node.playerLevelRequirement > SkillTree.playerLevel || node.treeTotalPointsSpentRequirement > (CountPointsSpent() - node.currentLevel)))
                {
                    node.Deplete();
                }
            }
        }

        /// <summary>
        /// Creates and returns a list of all connections of a specific node.
        /// </summary>
        /// <param name="skillNode">The skill node.</param>
        /// <returns>A list of all connections of a specific node.</returns>
        public List<SkillConnection> GetConnectionsOfNode(SkillNode skillNode)
        {
            List<SkillConnection> relevantConnections = new();

            foreach (var connection in connections)
            {
                if (connection.skillNodeA == skillNode || connection.skillNodeB == skillNode)
                {
                    relevantConnections.Add(connection);
                }
            }

            return relevantConnections;
        }

        /// <summary>
        /// Entirely copies the data of another skill graph.
        /// </summary>
        /// <param name="other">The other skill graph.</param>
        public void CopyData(SkillGraph other)
        {
            enabled = other.enabled;
            forPlayer = other.forPlayer;
            id = other.id;
            displayName = other.displayName;
            gridDimensions = other.gridDimensions;
            nodeSize = other.nodeSize;
            nodeShape = other.nodeShape;
            visibility = other.visibility;

            for (int i = 0; i < other.nodes.Count; i++)
            {
                if (i < nodes.Count)
                {
                    nodes[i].CopyData(other.nodes[i]);
                }
                else
                {
                    nodes.Add(other.nodes[i]);
                }
            }

            for (int i = 0; i < other.connections.Count; i++)
            {
                if (i < connections.Count)
                {
                    connections[i].CopyData(other.connections[i]);
                }
                else
                {
                    connections.Add(other.connections[i]);
                }
            }
        }

        /// <summary>
        /// Copies the data from a savable skill graph.
        /// </summary>
        /// <param name="savable">The savable skill graph.</param>
        public void CopyData(SavableSkillGraph savable)
        {
            if (savable.id != id)
            {
                Debug.LogError("Skill Tree: cannot copy data from a different skill graph.");
                return;
            }

            for (int i = 0; i < savable.nodes.Count; i++)
            {
                nodes[i].CopyData(savable.nodes[i]);
            }
        }

        /// <summary>
        /// Saves the skill graph (editor only).
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        /// <summary>
        /// Updates the name of the asset (editor only).
        /// </summary>
        public void UpdateAssetName()
        {
#if UNITY_EDITOR
            string name = $"{id}_{displayName}";
            UnityEditor.EditorApplication.delayCall += () =>
            {
                UnityEditor.AssetDatabase.RenameAsset(GetFullPath(this), name);

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    this.name = name;
                };
            };
#endif
        }

        /// <summary>
        /// Creates and returns a SavableSkillGraph from this graph.
        /// </summary>
        /// <returns>The created SavableSkillGraph.</returns>
        public SavableSkillGraph CreateSavableGraph()
        {
            return SavableSkillGraph.Create(this);
        }

        /// <summary>
        /// Gets a skill graph.
        /// </summary>
        /// <param name="name">The name of the skill graph.</param>
        /// <returns>The skill graph or null if one with the name doesn't exist.</returns>
        public static SkillGraph GetSkillGraph(string name)
        {
            var graphs = Resources.LoadAll<SkillGraph>("SkillTree/SkillGraphs");

            foreach (var graph in graphs)
            {
                if (graph.displayName == name)
                {
                    return graph;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a skill graph.
        /// </summary>
        /// <param name="id">The ID of the skill graph.</param>
        /// <returns>The skill graph or null if one with the ID doesn't exist.</returns>
        public static SkillGraph GetSkillGraph(int id)
        {
            var graphs = Resources.LoadAll<SkillGraph>("SkillTree/SkillGraphs");

            foreach (var graph in graphs)
            {
                if (graph.id == id)
                {
                    return graph;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new instance of a skill graph (editor only).
        /// </summary>
        /// <returns>The created skill graph instance.</returns>
        public static SkillGraph Create(string name)
        {
#if UNITY_EDITOR
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
#endif

            var obj = CreateInstance<SkillGraph>();
            obj.enabled = true;
            obj.forPlayer = true;
            obj.id = GetID();
            obj.gridDimensions = new Vector2Int(8, 4);
            obj.nodeSize = 100;
            obj.nodeShape = SkillNode.Shape.Square;
            obj.displayName = name;

#if UNITY_EDITOR
            var path = Path.Combine(directoryPath, $"{obj.id}_{name}.asset");
            UnityEditor.AssetDatabase.CreateAsset(obj, path);
#endif

            return GetSkillGraph(obj.id);
        }

        /// <summary>
        /// Creates a copy of a skill graph.
        /// </summary>
        /// <param name="original">The skill graph to copy.</param>
        /// <returns>The created copy.</returns>
        public static SkillGraph CreateCopy(SkillGraph original)
        {
#if UNITY_EDITOR
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
#endif

            var obj = CreateInstance<SkillGraph>();
            obj.enabled = original.enabled;
            obj.forPlayer = original.forPlayer;
            obj.id = GetID();
            obj.gridDimensions = original.gridDimensions;
            obj.nodeSize = original.nodeSize;
            obj.nodeShape = original.nodeShape;
            obj.displayName = original.displayName + " (Copy)";

            for (int i = 0; i < original.nodes.Count; i++)
            {
                obj.nodes.Add(SkillNode.CreateCopy(original.nodes[i], obj));
            }

            for (int i = 0; i < original.connections.Count; i++)
            {
                var connection = new SkillConnection()
                {
                    graphReferenceID = obj.id,
                    skillNodeA = obj.GetNodeByPosition(original.connections[i].skillNodeA.positionIndex),
                    skillNodeB = obj.GetNodeByPosition(original.connections[i].skillNodeB.positionIndex),
                    twoWay = original.connections[i].twoWay
                };

                obj.connections.Add(connection);
            }

#if UNITY_EDITOR
            var path = Path.Combine(directoryPath, $"{obj.id}_{obj.displayName}.asset");
            UnityEditor.AssetDatabase.CreateAsset(obj, path);
#endif

            return GetSkillGraph(obj.id);
        }

        /// <summary>
        /// Gets the full path of a skill graph. Works in the editor only.
        /// </summary>
        /// <param name="skillGraph">The skill graph.</param>
        /// <returns>The full path to the skill graph.</returns>
        public static string GetFullPath(SkillGraph skillGraph)
        {
            return Path.Combine(directoryPath, $"{skillGraph.name}.asset");
        }

        /// <summary>
        /// Returns an unused ID.
        /// </summary>
        /// <returns>An unused ID.</returns>
        public static int GetID()
        {
            var items = Resources.LoadAll<SkillGraph>("SkillTree/SkillGraphs");
            var ids = new List<int>();

            foreach (var item in items)
            {
                ids.Add(item.id);
            }

            for (int i = 0; i < int.MaxValue; i++)
            {
                if (!ids.Contains(i))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Visiblity of a Skill Graph.
        /// </summary>
        public enum GraphVisibility
        {
            /// <summary>
            /// Allow the graph to be displayed in the skill tree window.
            /// </summary>
            Visible,

            /// <summary>
            /// Do not display in skill tree window.
            /// </summary>
            Hidden
        }
    }
}