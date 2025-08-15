//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;

namespace Esper.SkillTree
{
    /// <summary>
    /// A savable version of the SkillGraph.
    /// </summary>
    public class SavableSkillGraph
    {
        /// <summary>
        /// The ID of the skill graph.
        /// </summary>
        public int id;

        /// <summary>
        /// A list of nodes in this graph.
        /// </summary>
        public List<SavableSkillNode> nodes;

        /// <summary>
        /// Creates a SavableSkillGraph object.
        /// </summary>
        /// <param name="graph">The SkillGraph object to take data from.</param>
        /// <returns>The created SavableSkillGraph.</returns>
        public static SavableSkillGraph Create(SkillGraph graph)
        {
            var nodes = new List<SavableSkillNode>();

            foreach (var node in graph.nodes)
            {
                nodes.Add(new SavableSkillNode()
                {
                    positionIndex = node.positionIndex,
                    currentLevel = node.currentLevel,
                    state = node.state
                });
            }

            var savable = new SavableSkillGraph()
            {
                id = graph.id,
                nodes = nodes
            };

            return savable;
        }

        /// <summary>
        /// A savable version of the SkillNode.
        /// </summary>
        public struct SavableSkillNode
        {
            public int positionIndex;
            public int currentLevel;
            public SkillNode.State state;
        }
    }
}