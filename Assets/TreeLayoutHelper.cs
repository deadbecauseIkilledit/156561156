using System.Collections.Generic;
using UnityEngine;

public class TreeLayoutHelper
{
    public class TreeNode
    {
        public string id;
        public string name;
        public Vector2 position;
        public List<TreeNode> children = new List<TreeNode>();
        public TreeNode parent;
        public int depth;
        public float xOffset;
        public float mod;
        public float prelim;
        public float change;
        public float shift;
        public int number;
        public TreeNode thread;
    }
    
    // Improved tree layout using Walker's algorithm
    public static void CalculateTreeLayout(TreeNode root, float nodeWidth, float nodeHeight, float horizontalSpacing, float verticalSpacing)
    {
        if (root == null) return;
        
        // First pass - calculate preliminary x positions
        FirstWalk(root, null, nodeWidth + horizontalSpacing);
        
        // Second pass - calculate final positions
        SecondWalk(root, -root.prelim, 0, nodeHeight + verticalSpacing);
    }
    
    private static void FirstWalk(TreeNode node, TreeNode leftSibling, float distance)
    {
        if (node.children.Count == 0)
        {
            // Leaf node
            if (leftSibling != null)
            {
                node.prelim = leftSibling.prelim + distance;
            }
            else
            {
                node.prelim = 0;
            }
        }
        else
        {
            // Internal node
            TreeNode defaultAncestor = node.children[0];
            TreeNode previousChild = null;
            
            foreach (var child in node.children)
            {
                FirstWalk(child, previousChild, distance);
                defaultAncestor = Apportion(child, previousChild, defaultAncestor, distance);
                previousChild = child;
            }
            
            ExecuteShifts(node);
            
            float midpoint = (node.children[0].prelim + node.children[node.children.Count - 1].prelim) / 2;
            
            if (leftSibling != null)
            {
                node.prelim = leftSibling.prelim + distance;
                node.mod = node.prelim - midpoint;
            }
            else
            {
                node.prelim = midpoint;
            }
        }
    }
    
    private static TreeNode Apportion(TreeNode node, TreeNode leftSibling, TreeNode defaultAncestor, float distance)
    {
        if (leftSibling != null)
        {
            TreeNode vInnerRight = node;
            TreeNode vOuterRight = node;
            TreeNode vInnerLeft = leftSibling;
            TreeNode vOuterLeft = vInnerRight.parent.children[0];
            
            float sInnerRight = vInnerRight.mod;
            float sOuterRight = vOuterRight.mod;
            float sInnerLeft = vInnerLeft.mod;
            float sOuterLeft = vOuterLeft.mod;
            
            while (NextRight(vInnerLeft) != null && NextLeft(vInnerRight) != null)
            {
                vInnerLeft = NextRight(vInnerLeft);
                vInnerRight = NextLeft(vInnerRight);
                vOuterLeft = NextLeft(vOuterLeft);
                vOuterRight = NextRight(vOuterRight);
                
                vOuterRight.thread = NextRight(vInnerRight);
                
                float shift = (vInnerLeft.prelim + sInnerLeft) - (vInnerRight.prelim + sInnerRight) + distance;
                
                if (shift > 0)
                {
                    TreeNode ancestor = FindAncestor(vInnerLeft, node, defaultAncestor);
                    MoveSubtree(ancestor, node, shift);
                    sInnerRight += shift;
                    sOuterRight += shift;
                }
                
                sInnerLeft += vInnerLeft.mod;
                sInnerRight += vInnerRight.mod;
                sOuterLeft += vOuterLeft.mod;
                sOuterRight += vOuterRight.mod;
            }
            
            if (NextRight(vInnerLeft) != null && NextRight(vOuterRight) == null)
            {
                vOuterRight.thread = NextRight(vInnerLeft);
                vOuterRight.mod += sInnerLeft - sOuterRight;
            }
            
            if (NextLeft(vInnerRight) != null && NextLeft(vOuterLeft) == null)
            {
                vOuterLeft.thread = NextLeft(vInnerRight);
                vOuterLeft.mod += sInnerRight - sOuterLeft;
                defaultAncestor = node;
            }
        }
        
        return defaultAncestor;
    }
    
    private static void MoveSubtree(TreeNode wLeft, TreeNode wRight, float shift)
    {
        int subtrees = wRight.number - wLeft.number;
        if (subtrees > 0)
        {
            wRight.change -= shift / subtrees;
            wRight.shift += shift;
            wLeft.change += shift / subtrees;
            wRight.prelim += shift;
            wRight.mod += shift;
        }
    }
    
    private static void ExecuteShifts(TreeNode node)
    {
        float shift = 0;
        float change = 0;
        
        for (int i = node.children.Count - 1; i >= 0; i--)
        {
            TreeNode child = node.children[i];
            child.prelim += shift;
            child.mod += shift;
            change += child.change;
            shift += child.shift + change;
        }
    }
    
    private static TreeNode FindAncestor(TreeNode vInnerLeft, TreeNode node, TreeNode defaultAncestor)
    {
        if (vInnerLeft.parent == node.parent)
        {
            return vInnerLeft;
        }
        return defaultAncestor;
    }
    
    private static TreeNode NextLeft(TreeNode node)
    {
        if (node.children.Count > 0)
        {
            return node.children[0];
        }
        return node.thread;
    }
    
    private static TreeNode NextRight(TreeNode node)
    {
        if (node.children.Count > 0)
        {
            return node.children[node.children.Count - 1];
        }
        return node.thread;
    }
    
    private static void SecondWalk(TreeNode node, float modSum, int depth, float ySpacing)
    {
        node.position = new Vector2(node.prelim + modSum, -depth * ySpacing);
        node.depth = depth;
        
        foreach (var child in node.children)
        {
            SecondWalk(child, modSum + node.mod, depth + 1, ySpacing);
        }
    }
    
    // Radial tree layout for circular arrangement
    public static void CalculateRadialLayout(TreeNode root, float radius, float startAngle = 0)
    {
        if (root == null) return;
        
        root.position = Vector2.zero;
        
        if (root.children.Count > 0)
        {
            LayoutChildrenRadially(root, radius, startAngle, 360f);
        }
    }
    
    private static void LayoutChildrenRadially(TreeNode parent, float radius, float startAngle, float angleRange)
    {
        if (parent.children.Count == 0) return;
        
        float angleStep = angleRange / parent.children.Count;
        float currentAngle = startAngle;
        
        for (int i = 0; i < parent.children.Count; i++)
        {
            TreeNode child = parent.children[i];
            float angleRad = currentAngle * Mathf.Deg2Rad;
            
            child.position = parent.position + new Vector2(
                Mathf.Cos(angleRad) * radius,
                Mathf.Sin(angleRad) * radius
            );
            
            // Recursively layout grandchildren
            float childAngleRange = angleStep * 0.8f; // Slightly narrow the range for children
            float childStartAngle = currentAngle - childAngleRange / 2;
            LayoutChildrenRadially(child, radius * 0.7f, childStartAngle, childAngleRange);
            
            currentAngle += angleStep;
        }
    }
    
    // Force-directed layout for organic appearance
    public static void CalculateForceDirectedLayout(List<TreeNode> nodes, Dictionary<string, List<string>> edges, int iterations = 100)
    {
        float k = Mathf.Sqrt(10000f / nodes.Count); // Optimal distance between nodes
        float temperature = 100f;
        float coolingRate = temperature / iterations;
        
        // Initialize random positions
        foreach (var node in nodes)
        {
            node.position = new Vector2(
                Random.Range(-500f, 500f),
                Random.Range(-500f, 500f)
            );
        }
        
        for (int iter = 0; iter < iterations; iter++)
        {
            Dictionary<TreeNode, Vector2> forces = new Dictionary<TreeNode, Vector2>();
            
            // Initialize forces
            foreach (var node in nodes)
            {
                forces[node] = Vector2.zero;
            }
            
            // Calculate repulsive forces between all nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    Vector2 delta = nodes[i].position - nodes[j].position;
                    float distance = Mathf.Max(delta.magnitude, 0.01f);
                    Vector2 force = (delta / distance) * (k * k / distance);
                    
                    forces[nodes[i]] += force;
                    forces[nodes[j]] -= force;
                }
            }
            
            // Calculate attractive forces for connected nodes
            foreach (var fromId in edges.Keys)
            {
                TreeNode fromNode = nodes.Find(n => n.id == fromId);
                if (fromNode == null) continue;
                
                foreach (var toId in edges[fromId])
                {
                    TreeNode toNode = nodes.Find(n => n.id == toId);
                    if (toNode == null) continue;
                    
                    Vector2 delta = toNode.position - fromNode.position;
                    float distance = Mathf.Max(delta.magnitude, 0.01f);
                    Vector2 force = (delta / distance) * (distance * distance / k);
                    
                    forces[fromNode] += force;
                    forces[toNode] -= force;
                }
            }
            
            // Apply forces with temperature
            foreach (var node in nodes)
            {
                Vector2 force = forces[node];
                float forceMagnitude = Mathf.Min(force.magnitude, temperature);
                
                if (force.magnitude > 0)
                {
                    node.position += (force / force.magnitude) * forceMagnitude;
                }
            }
            
            // Cool down
            temperature -= coolingRate;
        }
    }
}