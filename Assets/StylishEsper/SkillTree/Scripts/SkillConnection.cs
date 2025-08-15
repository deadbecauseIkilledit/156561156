//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Stores skill node connection data.
//***************************************************************************************

namespace Esper.SkillTree
{
    [System.Serializable]
    public class SkillConnection
    {
        /// <summary>
        /// The reference of the graph that this connection is a part of.
        /// </summary>
        public int graphReferenceID = -1;

        /// <summary>
        /// The first skill node.
        /// </summary>
        public SkillNode skillNodeA;

        /// <summary>
        /// The second skill node.
        /// </summary>
        public SkillNode skillNodeB;

        /// <summary>
        /// If this connection goes both ways (from A to B and B to A).
        /// </summary>
        public bool twoWay;

        /// <summary>
        /// Gets the skill graph of this connection.
        /// </summary>
        /// <returns>The skill graph this connection is a part of or null if it doesn't exist.</returns>
        public SkillGraph GetGraph()
        {
            return SkillTree.GetSkillGraph(graphReferenceID);
        }

        /// <summary>
        /// Entirely copies the data of another connection.
        /// </summary>
        /// <param name="other">The other connection.</param>
        public void CopyData(SkillConnection other)
        {
            graphReferenceID = other.graphReferenceID;
            skillNodeA = other.skillNodeA;
            skillNodeB = other.skillNodeB;
            twoWay = other.twoWay;
        }

        /// <summary>
        /// Creates and returns a copy of itself.
        /// </summary>
        /// <returns>A new copy of itself.</returns>
        public SkillConnection CreateCopy()
        {
            var copy = new SkillConnection()
            {
                graphReferenceID = graphReferenceID,
                skillNodeA = skillNodeA,
                skillNodeB = skillNodeB,
                twoWay = twoWay
            };

            return copy;
        }

        /// <summary>
        /// Checks if this connection is valid. A connection must have all reference IDs set.
        /// </summary>
        /// <returns>True if the connection is valid. Otherwise, false.</returns>
        public bool IsValid()
        {
            return graphReferenceID != -1 && skillNodeA && skillNodeB;
        }

        /// <summary>
        /// Checks if another connection has similar data.
        /// </summary>
        /// <param name="other">The other connection.</param>
        /// <returns>True if the other connection is similar. Otherwise, false.</returns>
        public bool IsEqualTo(SkillConnection other)
        {
            return graphReferenceID == other.graphReferenceID &&
                (skillNodeA == other.skillNodeA || skillNodeA == other.skillNodeB) &&
                (skillNodeB == other.skillNodeB || skillNodeB == other.skillNodeA);
        }
    }
}