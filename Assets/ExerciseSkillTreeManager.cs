using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System.Linq;
using SharpUI.Source.Common.UI.Elements;
using SharpUI.Source.Common.UI.Elements.Events;
using SharpUI.Source.Common.UI.Elements.State;
using CurriculumSystem;

// Data Models matching your JSON structure
[System.Serializable]
public class ExerciseData
{
    public string id;
    public string name;
    public string description_key;
    public int position;
    public bool is_active;
    public string[] tags;
    public string[] equipment_required;
    public string[] benchmarks;
}

[System.Serializable]
public class ModuleData
{
    public string id;
    public string name;
    public string description_key;
    public bool is_active;
    public List<ExerciseData> exercises;
}

[System.Serializable]
public class GroupData
{
    public string id;
    public string name;
    public string description_key;
    public bool is_active;
    public List<ExerciseData> exercises;
    public List<ModuleData> modules;
}

// Using ExerciseProgressionData from CurriculumSystem namespace

public class ExerciseSkillTreeManager : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject skillTreeNodePrefab;
    [SerializeField] private GameObject skillTreeButtonPrefab;
    [SerializeField] private GameObject skillTreeProgressLinePrefab;
    [SerializeField] private GameObject modalViewPrefab;

    [Header("UI References")]
    [SerializeField] private Transform skillTreeContainer;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private SkillTreeCameraController cameraController;
    [SerializeField] private ExerciseDetailModal detailModal;
    [SerializeField] private SkillTreeAnimations animations;

    [Header("Layout Settings")]
    [SerializeField] private float horizontalSpacing = 250f;
    [SerializeField] private float verticalSpacing = 180f;
    [SerializeField] private float groupSpacing = 400f;
    [SerializeField] private float moduleSpacing = 100f;
    [SerializeField] private Vector2 startPosition = new Vector2(0, 400);
    [SerializeField] private bool useHierarchicalLayout = true;
    [SerializeField] private LayoutType layoutType = LayoutType.Hierarchical;
    
    public enum LayoutType
    {
        Hierarchical,
        Radial,
        ForceDirected
    }
    
    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    // Data storage
    private Dictionary<string, ExerciseNode> exerciseNodes = new Dictionary<string, ExerciseNode>();
    private Dictionary<string, ExerciseData> exerciseDataMap = new Dictionary<string, ExerciseData>();
    private List<CurriculumSystem.ExerciseProgressionData> progressions = new List<CurriculumSystem.ExerciseProgressionData>();
    private Dictionary<string, bool> completedExercises = new Dictionary<string, bool>();
    
    // Hierarchy nodes for proper tree structure
    private Dictionary<string, GameObject> hierarchyNodes = new Dictionary<string, GameObject>();
    private GameObject rootNode;
    private CurriculumSystem.CurriculumExportData fullCurriculumData;

    // Modal reference
    private GameObject currentModal;
    
    // Parser reference
    private SimpleCurriculumParser parser;
    // private bool isDataLoaded = false; // Reserved for future use

    // Custom node wrapper class
    public class ExerciseNode
    {
        public GameObject gameObject;
        public ExerciseData data;
        public BaseElement button;  // Now using the correct SharpUI BaseElement
        public bool isUnlocked = false;
        public bool isCompleted = false;
        public List<string> prerequisites = new List<string>();
        public List<GameObject> connectionLines = new List<GameObject>();
    }
    
    // Node type enum for hierarchy
    public enum NodeType
    {
        Root,
        Discipline,
        Curriculum,
        Group,
        Module,
        Exercise
    }

    void Start()
    {
        Debug.Log("ExerciseSkillTreeManager Started");
        
        // Ensure scene is properly set up
        if (GetComponent<SceneSetupFixer>() == null)
        {
            gameObject.AddComponent<SceneSetupFixer>();
        }
        
        // Add test configuration in editor
        #if UNITY_EDITOR
        if (GetComponent<SkillTreeTestConfig>() == null)
        {
            gameObject.AddComponent<SkillTreeTestConfig>();
        }
        #endif
        
        // Get the parser component
        parser = GetComponent<SimpleCurriculumParser>();
        
        // Setup camera controller if not assigned
        if (cameraController == null && skillTreeContainer != null)
        {
            cameraController = skillTreeContainer.GetComponent<SkillTreeCameraController>();
            if (cameraController == null)
            {
                cameraController = skillTreeContainer.gameObject.AddComponent<SkillTreeCameraController>();
            }
        }
        
        // Setup animations if not assigned
        if (animations == null)
        {
            animations = GetComponent<SkillTreeAnimations>();
            if (animations == null)
            {
                animations = gameObject.AddComponent<SkillTreeAnimations>();
            }
        }
        
        if (parser != null)
        {
            // Check if data is already loaded
            if (parser.AllExercises != null && parser.AllExercises.Count > 0)
            {
                // Data already loaded
                LoadExerciseData();
                BuildSkillTree();
                SetupProgressionLogic();
            }
            else
            {
                // Wait for data to load
                ShowLoadingUI(true);
                parser.OnDataLoaded += OnDataLoaded;
                parser.OnLoadingStatusChanged += OnLoadingStatusChanged;
            }
        }
        else
        {
            Debug.LogError("SimpleCurriculumParser component not found!");
        }
    }

    void OnDataLoaded(bool success)
    {
        if (success)
        {
            Debug.Log("Data loaded successfully from Firestore");
            LoadExerciseData();
            BuildSkillTree();
            SetupProgressionLogic();
            ShowLoadingUI(false);
            // isDataLoaded = true;
        }
        else
        {
            Debug.LogError("Failed to load data from Firestore");
            ShowLoadingUI(false);
            UpdateLoadingText("Failed to load data. Using sample data.");
        }
    }
    
    void OnLoadingStatusChanged(string status)
    {
        UpdateLoadingText(status);
    }
    
    void ShowLoadingUI(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
    }
    
    void UpdateLoadingText(string text)
    {
        if (loadingText != null)
        {
            loadingText.text = text;
        }
    }
    
    void LoadExerciseData()
    {
        Debug.Log("Loading Exercise Data...");
        
        if (parser != null)
        {
        // Get data from the parser
        var allExercises = parser.GetAllExercises();
        var progressions = parser.GetProgressions();
        
        // Convert to our format
        foreach (var exercise in allExercises.Values)
        {
            var exerciseData = new ExerciseData
            {
                id = exercise.id,
                name = exercise.name,
                description_key = exercise.description_key,
                position = exercise.position,
                is_active = exercise.is_active,
                tags = exercise.tags?.ToArray() ?? new string[0],
                equipment_required = exercise.equipment_required?.ToArray() ?? new string[0],
                benchmarks = exercise.benchmarks?.ToArray() ?? new string[0]
            };
            
            exerciseDataMap[exercise.id] = exerciseData;
        }
        
        // Store progressions directly
        this.progressions = progressions;
        
            Debug.Log($"Loaded {exerciseDataMap.Count} exercises and {this.progressions.Count} progressions from parser");
        }
        else
        {
            Debug.LogWarning("No SimpleCurriculumParser found! Using fallback data.");
            CreateSampleData(); // Use the existing sample data method as fallback
        }
    }

    void CreateSampleData()
    {
        // Create sample exercises based on your data structure
        var exercise1 = new ExerciseData
        {
            id = "fb1ab11e-7a69-487d-9ffd-9528ee54538f",
            name = "Push-ups",
            description_key = "jingDesc",
            position = 1,
            is_active = true,
            tags = new string[0],
            equipment_required = new string[0],
            benchmarks = new string[0]
        };

        var exercise2 = new ExerciseData
        {
            id = "d8fe8b04-e044-4b18-9b37-8795a40f2698",
            name = "Diamond Push-ups",
            description_key = "jingDesc",
            position = 2,
            is_active = true,
            tags = new string[0],
            equipment_required = new string[0],
            benchmarks = new string[0]
        };

        var exercise3 = new ExerciseData
        {
            id = "a1081bb7-ba40-487c-8dd8-ae5763535de2",
            name = "Archer Push-ups",
            description_key = "jingDesc",
            position = 3,
            is_active = true,
            tags = new string[0],
            equipment_required = new string[0],
            benchmarks = new string[0]
        };

        exerciseDataMap[exercise1.id] = exercise1;
        exerciseDataMap[exercise2.id] = exercise2;
        exerciseDataMap[exercise3.id] = exercise3;

        // Create a sample progression chain
        var progression = new CurriculumSystem.ExerciseProgressionData
        {
            id = "sample_progression",
            parent_id = "sample_group",
            chains = new List<CurriculumSystem.Chain>
            {
                new CurriculumSystem.Chain
                {
                    id = "chain1",
                    color = "#3B82F6",
                    exercise_ids = new List<string> { exercise1.id, exercise2.id, exercise3.id }
                }
            }
        };

        progressions.Add(progression);
    }

    void BuildSkillTree()
    {
        Debug.Log("Building Skill Tree...");
        
        // Clear existing nodes if any
        ClearSkillTree();

        if (skillTreeContainer == null)
        {
            Debug.LogError("Skill Tree Container is not assigned!");
            return;
        }

        // Always use hierarchical layout for proper tree structure
        BuildProperHierarchicalTree();
        
        // Animate all nodes appearing (including hierarchy nodes)
        if (animations != null)
        {
            List<GameObject> allNodes = new List<GameObject>();
            
            // Add hierarchy nodes
            foreach (var kvp in hierarchyNodes)
            {
                allNodes.Add(kvp.Value);
            }
            
            // Add exercise nodes
            foreach (var kvp in exerciseNodes)
            {
                allNodes.Add(kvp.Value.gameObject);
            }
            
            animations.AnimateNodesAppear(allNodes.ToArray());
        }
        
        // After building, fit all nodes in view
        if (cameraController != null)
        {
            StartCoroutine(FitToViewAfterFrame());
        }
    }
    
    System.Collections.IEnumerator FitToViewAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        cameraController.FitAllNodesInView();
    }
    
    void BuildSimpleLayout()
    {
        int columnIndex = 0;

        foreach (var progression in progressions)
        {
            foreach (var chain in progression.chains)
            {
                int rowIndex = 0;
                foreach (string exerciseId in chain.exercise_ids)
                {
                    if (!exerciseNodes.ContainsKey(exerciseId))
                    {
                        Vector2 position = startPosition + new Vector2(columnIndex * horizontalSpacing, -rowIndex * verticalSpacing);
                        CreateExerciseNodeAtPosition(exerciseId, position);
                    }
                    rowIndex++;
                }

                // Create connections between exercises in the chain
                for (int i = 0; i < chain.exercise_ids.Count - 1; i++)
                {
                    CreateConnection(chain.exercise_ids[i], chain.exercise_ids[i + 1], chain.color);
                }

                columnIndex++;
            }
        }
    }
    
    void BuildProperHierarchicalTree()
    {
        Debug.Log($"Building proper hierarchical tree structure with layout type: {layoutType}");
        
        // Create root node
        Vector2 rootPosition = startPosition;
        rootNode = CreateHierarchyNode("Fitness Curriculum", NodeType.Root, rootPosition);
        hierarchyNodes["root"] = rootNode;
        
        // Get curriculum data from parser if available
        var curriculumData = GetFullCurriculumData();
        
        if (curriculumData != null && curriculumData.collections != null && 
            curriculumData.collections.disciplines_v3 != null && 
            curriculumData.collections.disciplines_v3.Count > 0)
        {
            if (layoutType == LayoutType.Hierarchical)
            {
                BuildFromCurriculumData(curriculumData);
            }
            else
            {
                BuildWithImprovedLayout(curriculumData);
            }
        }
        else
        {
            // Fallback to building from progressions
            BuildFromProgressions();
        }
    }
    
    CurriculumSystem.CurriculumExportData GetFullCurriculumData()
    {
        // Try to get full curriculum data from parser
        if (parser != null)
        {
            return parser.FullCurriculumData;
        }
        return null;
    }
    
    void BuildWithImprovedLayout(CurriculumSystem.CurriculumExportData data)
    {
        Debug.Log($"Building tree with improved {layoutType} layout");
        
        // Build tree structure
        TreeLayoutHelper.TreeNode rootTreeNode = new TreeLayoutHelper.TreeNode
        {
            id = "root",
            name = "Fitness Curriculum",
            position = Vector2.zero
        };
        
        Dictionary<string, TreeLayoutHelper.TreeNode> treeNodeMap = new Dictionary<string, TreeLayoutHelper.TreeNode>();
        treeNodeMap["root"] = rootTreeNode;
        
        // Build tree hierarchy from curriculum data
        foreach (var discipline in data.collections.disciplines_v3)
        {
            var disciplineNode = new TreeLayoutHelper.TreeNode
            {
                id = discipline.id,
                name = discipline.name,
                parent = rootTreeNode
            };
            rootTreeNode.children.Add(disciplineNode);
            treeNodeMap[discipline.id] = disciplineNode;
            
            if (discipline.curriculums != null)
            {
                foreach (var curriculum in discipline.curriculums)
                {
                    var curriculumNode = new TreeLayoutHelper.TreeNode
                    {
                        id = curriculum.id,
                        name = curriculum.name,
                        parent = disciplineNode
                    };
                    disciplineNode.children.Add(curriculumNode);
                    treeNodeMap[curriculum.id] = curriculumNode;
                    
                    if (curriculum.groups != null)
                    {
                        foreach (var group in curriculum.groups)
                        {
                            var groupNode = new TreeLayoutHelper.TreeNode
                            {
                                id = group.id,
                                name = group.name,
                                parent = curriculumNode
                            };
                            curriculumNode.children.Add(groupNode);
                            treeNodeMap[group.id] = groupNode;
                        }
                    }
                }
            }
        }
        
        // Calculate layout based on type
        switch (layoutType)
        {
            case LayoutType.Radial:
                TreeLayoutHelper.CalculateRadialLayout(rootTreeNode, 300f);
                break;
                
            case LayoutType.ForceDirected:
                // Convert to list for force-directed
                List<TreeLayoutHelper.TreeNode> allNodes = new List<TreeLayoutHelper.TreeNode>(treeNodeMap.Values);
                Dictionary<string, List<string>> edges = new Dictionary<string, List<string>>();
                
                foreach (var node in allNodes)
                {
                    if (node.parent != null)
                    {
                        if (!edges.ContainsKey(node.parent.id))
                            edges[node.parent.id] = new List<string>();
                        edges[node.parent.id].Add(node.id);
                    }
                }
                
                TreeLayoutHelper.CalculateForceDirectedLayout(allNodes, edges, 50);
                break;
                
            default:
                TreeLayoutHelper.CalculateTreeLayout(rootTreeNode, 150f, 60f, horizontalSpacing, verticalSpacing);
                break;
        }
        
        // Create visual nodes based on calculated positions
        foreach (var kvp in treeNodeMap)
        {
            TreeLayoutHelper.TreeNode treeNode = kvp.Value;
            NodeType nodeType = GetNodeTypeFromTreeNode(treeNode);
            
            GameObject visualNode = CreateHierarchyNode(treeNode.name, nodeType, treeNode.position + startPosition);
            hierarchyNodes[treeNode.id] = visualNode;
            
            // Connect to parent
            if (treeNode.parent != null && hierarchyNodes.ContainsKey(treeNode.parent.id))
            {
                string color = GetConnectionColorForNodeType(nodeType);
                CreateHierarchyConnection(hierarchyNodes[treeNode.parent.id], visualNode, color);
            }
        }
        
        // Now add exercise nodes
        AddExerciseNodesToImprovedLayout(data, treeNodeMap);
    }
    
    NodeType GetNodeTypeFromTreeNode(TreeLayoutHelper.TreeNode node)
    {
        if (node.id == "root") return NodeType.Root;
        if (node.parent != null && node.parent.id == "root") return NodeType.Discipline;
        if (node.parent != null && node.parent.parent != null && node.parent.parent.id == "root") return NodeType.Curriculum;
        if (node.children.Count == 0) return NodeType.Exercise;
        return NodeType.Group;
    }
    
    string GetConnectionColorForNodeType(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Discipline: return "#8B5CF6"; // Purple
            case NodeType.Curriculum: return "#3B82F6"; // Blue
            case NodeType.Group: return "#10B981"; // Green
            case NodeType.Module: return "#F59E0B"; // Orange
            default: return "#6B7280"; // Gray
        }
    }
    
    void AddExerciseNodesToImprovedLayout(CurriculumSystem.CurriculumExportData data, Dictionary<string, TreeLayoutHelper.TreeNode> treeNodeMap)
    {
        // Add exercises under their parent groups
        foreach (var discipline in data.collections.disciplines_v3)
        {
            if (discipline.curriculums == null) continue;
            
            foreach (var curriculum in discipline.curriculums)
            {
                if (curriculum.groups == null) continue;
                
                foreach (var group in curriculum.groups)
                {
                    if (!treeNodeMap.ContainsKey(group.id)) continue;
                    
                    TreeLayoutHelper.TreeNode groupTreeNode = treeNodeMap[group.id];
                    GameObject groupVisualNode = hierarchyNodes[group.id];
                    
                    // Find progressions for this group
                    var groupProgressions = progressions.Where(p => p.parent_id == group.id).ToList();
                    
                    if (groupProgressions.Count > 0)
                    {
                        // Layout exercises based on progression chains
                        int chainIndex = 0;
                        foreach (var progression in groupProgressions)
                        {
                            foreach (var chain in progression.chains)
                            {
                                float chainOffset = (chainIndex - progression.chains.Count / 2f) * 100f;
                                GameObject previousExerciseNode = null;
                                
                                for (int i = 0; i < chain.exercise_ids.Count; i++)
                                {
                                    string exerciseId = chain.exercise_ids[i];
                                    
                                    Vector2 exercisePos = groupTreeNode.position + startPosition + 
                                        new Vector2(chainOffset, -(i + 1) * 80f);
                                    
                                    CreateExerciseNodeAtPosition(exerciseId, exercisePos);
                                    
                                    if (exerciseNodes.ContainsKey(exerciseId))
                                    {
                                        GameObject exerciseNode = exerciseNodes[exerciseId].gameObject;
                                        
                                        if (i == 0)
                                        {
                                            // Connect to group
                                            CreateHierarchyConnection(groupVisualNode, exerciseNode, chain.color);
                                        }
                                        else if (previousExerciseNode != null)
                                        {
                                            // Connect to previous in chain
                                            CreateConnection(chain.exercise_ids[i - 1], exerciseId, chain.color);
                                        }
                                        
                                        previousExerciseNode = exerciseNode;
                                    }
                                }
                                
                                chainIndex++;
                            }
                        }
                    }
                    else if (group.exercises != null)
                    {
                        // No progressions, lay out exercises in a grid
                        int cols = Mathf.CeilToInt(Mathf.Sqrt(group.exercises.Count));
                        int index = 0;
                        
                        foreach (var exercise in group.exercises)
                        {
                            int row = index / cols;
                            int col = index % cols;
                            
                            Vector2 exercisePos = groupTreeNode.position + startPosition + 
                                new Vector2((col - cols / 2f) * 100f, -(row + 1) * 80f);
                            
                            CreateExerciseNodeAtPosition(exercise.id, exercisePos);
                            
                            if (exerciseNodes.ContainsKey(exercise.id))
                            {
                                CreateHierarchyConnection(groupVisualNode, exerciseNodes[exercise.id].gameObject, "#6B7280");
                            }
                            
                            index++;
                        }
                    }
                }
            }
        }
    }
    
    void BuildFromCurriculumData(CurriculumSystem.CurriculumExportData data)
    {
        float disciplineY = startPosition.y - 150f;
        float disciplineX = startPosition.x;
        
        foreach (var discipline in data.collections.disciplines_v3)
        {
            // Create discipline node
            Vector2 disciplinePos = new Vector2(disciplineX, disciplineY);
            GameObject disciplineNode = CreateHierarchyNode(discipline.name, NodeType.Discipline, disciplinePos);
            hierarchyNodes[discipline.id] = disciplineNode;
            
            // Connect to root
            CreateHierarchyConnection(rootNode, disciplineNode, "#8B5CF6"); // Purple
            
            if (discipline.curriculums != null)
            {
                float curriculumX = disciplineX - (discipline.curriculums.Count - 1) * 200f;
                float curriculumY = disciplineY - 200f;
                
                foreach (var curriculum in discipline.curriculums)
                {
                    // Create curriculum node
                    Vector2 curriculumPos = new Vector2(curriculumX, curriculumY);
                    GameObject curriculumNode = CreateHierarchyNode(curriculum.name, NodeType.Curriculum, curriculumPos);
                    hierarchyNodes[curriculum.id] = curriculumNode;
                    
                    // Connect to discipline
                    CreateHierarchyConnection(disciplineNode, curriculumNode, "#3B82F6"); // Blue
                    
                    if (curriculum.groups != null)
                    {
                        float groupX = curriculumX - (curriculum.groups.Count - 1) * 150f;
                        float groupY = curriculumY - 200f;
                        
                        foreach (var group in curriculum.groups)
                        {
                            // Create group node
                            Vector2 groupPos = new Vector2(groupX, groupY);
                            GameObject groupNode = CreateHierarchyNode(group.name, NodeType.Group, groupPos);
                            hierarchyNodes[group.id] = groupNode;
                            
                            // Connect to curriculum
                            CreateHierarchyConnection(curriculumNode, groupNode, "#10B981"); // Green
                            
                            // Create exercise nodes under this group
                            CreateExerciseNodesForGroup(group, groupNode, groupPos);
                            
                            groupX += 300f;
                        }
                    }
                    
                    curriculumX += 400f;
                }
            }
            
            disciplineX += 800f;
        }
    }
    
    void CreateExerciseNodesForGroup(CurriculumSystem.Group group, GameObject groupNode, Vector2 groupPos)
    {
        float exerciseStartX = groupPos.x - 100f;
        float exerciseStartY = groupPos.y - 150f;
        int columnIndex = 0;
        
        // Find progressions for this group
        var groupProgressions = progressions.Where(p => p.parent_id == group.id).ToList();
        
        if (groupProgressions.Count > 0)
        {
            // Use progression chains for layout
            foreach (var progression in groupProgressions)
            {
                foreach (var chain in progression.chains)
                {
                    float exerciseY = exerciseStartY;
                    GameObject previousExerciseNode = null;
                    
                    foreach (string exerciseId in chain.exercise_ids)
                    {
                        Vector2 exercisePos = new Vector2(exerciseStartX + columnIndex * 120f, exerciseY);
                        CreateExerciseNodeAtPosition(exerciseId, exercisePos);
                        
                        if (exerciseNodes.ContainsKey(exerciseId))
                        {
                            GameObject exerciseNode = exerciseNodes[exerciseId].gameObject;
                            
                            // Connect first exercise to group
                            if (previousExerciseNode == null)
                            {
                                CreateHierarchyConnection(groupNode, exerciseNode, "#6B7280"); // Gray
                            }
                            else
                            {
                                // Connect to previous exercise in chain
                                CreateConnection(previousExerciseNode.name.Replace("Node_", ""), exerciseId, chain.color);
                            }
                            
                            previousExerciseNode = exerciseNode;
                        }
                        
                        exerciseY -= verticalSpacing * 0.7f;
                    }
                    
                    columnIndex++;
                }
            }
        }
        else if (group.exercises != null && group.exercises.Count > 0)
        {
            // No progressions, just lay out exercises
            int index = 0;
            foreach (var exercise in group.exercises)
            {
                Vector2 exercisePos = new Vector2(
                    exerciseStartX + (index % 3) * 120f,
                    exerciseStartY - (index / 3) * 100f
                );
                CreateExerciseNodeAtPosition(exercise.id, exercisePos);
                
                if (exerciseNodes.ContainsKey(exercise.id))
                {
                    CreateHierarchyConnection(groupNode, exerciseNodes[exercise.id].gameObject, "#6B7280");
                }
                
                index++;
            }
        }
        
        // Handle modules if present
        if (group.modules != null)
        {
            float moduleX = exerciseStartX + columnIndex * 150f;
            float moduleY = exerciseStartY;
            
            foreach (var module in group.modules)
            {
                Vector2 modulePos = new Vector2(moduleX, moduleY);
                GameObject moduleNode = CreateHierarchyNode(module.name, NodeType.Module, modulePos);
                hierarchyNodes[module.id] = moduleNode;
                
                // Connect to group
                CreateHierarchyConnection(groupNode, moduleNode, "#F59E0B"); // Orange
                
                // Add module exercises
                if (module.exercises != null)
                {
                    float exY = moduleY - 100f;
                    foreach (var exercise in module.exercises)
                    {
                        Vector2 exPos = new Vector2(moduleX, exY);
                        CreateExerciseNodeAtPosition(exercise.id, exPos);
                        
                        if (exerciseNodes.ContainsKey(exercise.id))
                        {
                            CreateHierarchyConnection(moduleNode, exerciseNodes[exercise.id].gameObject, "#6B7280");
                        }
                        
                        exY -= 80f;
                    }
                }
                
                moduleY -= 250f;
            }
        }
    }
    
    void BuildFromProgressions()
    {
        Debug.Log("Building tree from progressions (fallback method)");
        
        // Group progressions by parent
        Dictionary<string, List<CurriculumSystem.ExerciseProgressionData>> progressionsByParent = 
            new Dictionary<string, List<CurriculumSystem.ExerciseProgressionData>>();
        
        foreach (var progression in progressions)
        {
            string parentKey = progression.parent_id ?? "ungrouped";
            if (!progressionsByParent.ContainsKey(parentKey))
            {
                progressionsByParent[parentKey] = new List<CurriculumSystem.ExerciseProgressionData>();
            }
            progressionsByParent[parentKey].Add(progression);
        }
        
        // Create group nodes and connect to root
        float groupX = startPosition.x - (progressionsByParent.Count - 1) * 200f;
        float groupY = startPosition.y - 200f;
        
        foreach (var kvp in progressionsByParent)
        {
            string parentId = kvp.Key;
            List<CurriculumSystem.ExerciseProgressionData> groupProgressions = kvp.Value;
            
            // Create group node
            Vector2 groupPos = new Vector2(groupX, groupY);
            string groupName = GetGroupName(parentId);
            GameObject groupNode = CreateHierarchyNode(groupName, NodeType.Group, groupPos);
            hierarchyNodes[parentId] = groupNode;
            
            // Connect to root
            CreateHierarchyConnection(rootNode, groupNode, "#3B82F6");
            
            // Create exercise chains under this group
            float chainX = groupX - 100f;
            float chainY = groupY - 150f;
            int chainIndex = 0;
            
            foreach (var progression in groupProgressions)
            {
                foreach (var chain in progression.chains)
                {
                    float exerciseY = chainY;
                    GameObject previousNode = null;
                    
                    for (int i = 0; i < chain.exercise_ids.Count; i++)
                    {
                        string exerciseId = chain.exercise_ids[i];
                        Vector2 exercisePos = new Vector2(chainX + chainIndex * 150f, exerciseY);
                        
                        CreateExerciseNodeAtPosition(exerciseId, exercisePos);
                        
                        if (exerciseNodes.ContainsKey(exerciseId))
                        {
                            GameObject exerciseNode = exerciseNodes[exerciseId].gameObject;
                            
                            // Connect first exercise to group
                            if (i == 0)
                            {
                                CreateHierarchyConnection(groupNode, exerciseNode, "#6B7280");
                            }
                            
                            // Connect to previous exercise in chain
                            if (previousNode != null)
                            {
                                CreateConnection(
                                    chain.exercise_ids[i - 1],
                                    exerciseId,
                                    chain.color
                                );
                            }
                            
                            previousNode = exerciseNode;
                        }
                        
                        exerciseY -= verticalSpacing * 0.8f;
                    }
                    
                    chainIndex++;
                }
            }
            
            groupX += 400f;
        }
    }
    
    GameObject CreateHierarchyNode(string nodeName, NodeType nodeType, Vector2 position)
    {
        GameObject node = new GameObject($"HierarchyNode_{nodeName}");
        node.transform.SetParent(skillTreeContainer);
        
        RectTransform rect = node.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        
        // Set size based on node type
        switch (nodeType)
        {
            case NodeType.Root:
                rect.sizeDelta = new Vector2(200, 80);
                break;
            case NodeType.Discipline:
                rect.sizeDelta = new Vector2(180, 70);
                break;
            case NodeType.Curriculum:
                rect.sizeDelta = new Vector2(160, 60);
                break;
            case NodeType.Group:
                rect.sizeDelta = new Vector2(140, 50);
                break;
            case NodeType.Module:
                rect.sizeDelta = new Vector2(120, 45);
                break;
            default:
                rect.sizeDelta = new Vector2(100, 40);
                break;
        }
        
        // Add background image
        Image bgImage = node.AddComponent<Image>();
        
        // Set color based on node type
        switch (nodeType)
        {
            case NodeType.Root:
                bgImage.color = new Color(0.55f, 0.35f, 0.96f, 0.95f); // Purple
                break;
            case NodeType.Discipline:
                bgImage.color = new Color(0.45f, 0.4f, 0.9f, 0.9f); // Lighter purple
                break;
            case NodeType.Curriculum:
                bgImage.color = new Color(0.23f, 0.51f, 0.96f, 0.9f); // Blue
                break;
            case NodeType.Group:
                bgImage.color = new Color(0.06f, 0.72f, 0.51f, 0.9f); // Green
                break;
            case NodeType.Module:
                bgImage.color = new Color(0.96f, 0.62f, 0.04f, 0.9f); // Orange
                break;
            default:
                bgImage.color = new Color(0.5f, 0.5f, 0.5f, 0.9f); // Gray
                break;
        }
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(node.transform);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = nodeName;
        text.fontSize = nodeType == NodeType.Root ? 20 : (nodeType == NodeType.Discipline ? 18 : 14);
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        // Make text not block clicks
        text.raycastTarget = false;
        
        // Add button component for interactivity
        Button button = node.AddComponent<Button>();
        button.targetGraphic = bgImage; // Set target graphic
        button.transition = Button.Transition.ColorTint;
        
        // Setup hover effects
        if (animations != null)
        {
            animations.SetupHoverEffects(node);
        }
        
        return node;
    }
    
    void CreateHierarchyConnection(GameObject fromNode, GameObject toNode, string colorHex)
    {
        if (fromNode == null || toNode == null) return;
        
        GameObject line = skillTreeProgressLinePrefab != null ?
            Instantiate(skillTreeProgressLinePrefab, skillTreeContainer) :
            CreateSimpleLine();
        
        if (line != null)
        {
            RectTransform fromRect = fromNode.GetComponent<RectTransform>();
            RectTransform toRect = toNode.GetComponent<RectTransform>();
            RectTransform lineRect = line.GetComponent<RectTransform>();
            
            Vector2 fromPos = fromRect.anchoredPosition;
            Vector2 toPos = toRect.anchoredPosition;
            Vector2 direction = toPos - fromPos;
            float distance = direction.magnitude;
            
            lineRect.anchoredPosition = (fromPos + toPos) / 2f;
            lineRect.sizeDelta = new Vector2(distance, 8f); // Thicker line for hierarchy
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lineRect.rotation = Quaternion.Euler(0, 0, angle);
            
            // Set line color
            Image lineImage = line.GetComponent<Image>();
            Color color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Default gray color
            
            if (lineImage != null && ColorUtility.TryParseHtmlString(colorHex, out color))
            {
                color.a = 0.7f; // Semi-transparent
                lineImage.color = color;
            }
            else if (lineImage != null)
            {
                lineImage.color = color;
            }
            
            // Make sure line renders behind nodes
            line.transform.SetAsFirstSibling();
            
            // Animate the connection line
            if (animations != null)
            {
                animations.AnimateConnectionLine(line, color);
            }
        }
    }
    
    void BuildHierarchicalLayout()
    {
        // Track positions for each group/module
        Dictionary<string, Vector2> groupPositions = new Dictionary<string, Vector2>();
        Dictionary<string, int> groupNodeCounts = new Dictionary<string, int>();
        
        float currentGroupX = startPosition.x;
        float currentY = startPosition.y;
        
        // Group progressions by parent_id (group or module)
        Dictionary<string, List<CurriculumSystem.ExerciseProgressionData>> progressionsByParent = new Dictionary<string, List<CurriculumSystem.ExerciseProgressionData>>();
        
        foreach (var progression in progressions)
        {
            string parentKey = progression.parent_id ?? "ungrouped";
            if (!progressionsByParent.ContainsKey(parentKey))
            {
                progressionsByParent[parentKey] = new List<CurriculumSystem.ExerciseProgressionData>();
            }
            progressionsByParent[parentKey].Add(progression);
        }
        
        // Layout each group
        foreach (var kvp in progressionsByParent)
        {
            string parentId = kvp.Key;
            List<CurriculumSystem.ExerciseProgressionData> groupProgressions = kvp.Value;
            
            // Calculate group bounds
            int maxChainLength = 0;
            int totalChains = 0;
            
            foreach (var progression in groupProgressions)
            {
                foreach (var chain in progression.chains)
                {
                    maxChainLength = Mathf.Max(maxChainLength, chain.exercise_ids.Count);
                    totalChains++;
                }
            }
            
            // Position this group
            Vector2 groupStartPos = new Vector2(currentGroupX, currentY);
            groupPositions[parentId] = groupStartPos;
            
            // Layout chains within this group
            int chainIndex = 0;
            foreach (var progression in groupProgressions)
            {
                foreach (var chain in progression.chains)
                {
                    // Layout exercises in this chain
                    for (int i = 0; i < chain.exercise_ids.Count; i++)
                    {
                        string exerciseId = chain.exercise_ids[i];
                        
                        if (!exerciseNodes.ContainsKey(exerciseId))
                        {
                            // Calculate position with improved spacing
                            float x = groupStartPos.x + (chainIndex * horizontalSpacing);
                            float y = groupStartPos.y - (i * verticalSpacing);
                            
                            // Add some variation to prevent perfect grid
                            float xVariation = UnityEngine.Random.Range(-20f, 20f);
                            float yVariation = UnityEngine.Random.Range(-10f, 10f);
                            
                            Vector2 position = new Vector2(x + xVariation, y + yVariation);
                            CreateExerciseNodeAtPosition(exerciseId, position);
                        }
                    }
                    
                    // Create connections for this chain
                    for (int i = 0; i < chain.exercise_ids.Count - 1; i++)
                    {
                        CreateConnection(chain.exercise_ids[i], chain.exercise_ids[i + 1], chain.color);
                    }
                    
                    chainIndex++;
                }
            }
            
            // Move to next group position
            currentGroupX += (totalChains * horizontalSpacing) + groupSpacing;
        }
        
        // Create group containers/backgrounds
        if (useHierarchicalLayout)
        {
            CreateGroupVisuals(progressionsByParent, groupPositions);
        }
    }
    
    void CreateGroupVisuals(Dictionary<string, List<CurriculumSystem.ExerciseProgressionData>> progressionsByParent, Dictionary<string, Vector2> groupPositions)
    {
        foreach (var kvp in progressionsByParent)
        {
            string parentId = kvp.Key;
            if (parentId == "ungrouped") continue;
            
            // Find all nodes in this group
            List<GameObject> groupNodes = new List<GameObject>();
            foreach (var progression in kvp.Value)
            {
                foreach (var chain in progression.chains)
                {
                    foreach (string exerciseId in chain.exercise_ids)
                    {
                        if (exerciseNodes.ContainsKey(exerciseId))
                        {
                            groupNodes.Add(exerciseNodes[exerciseId].gameObject);
                        }
                    }
                }
            }
            
            if (groupNodes.Count > 0)
            {
                // Calculate bounding box for this group
                Vector2 minPos = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 maxPos = new Vector2(float.MinValue, float.MinValue);
                
                foreach (var node in groupNodes)
                {
                    RectTransform rect = node.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        Vector2 pos = rect.anchoredPosition;
                        minPos.x = Mathf.Min(minPos.x, pos.x);
                        minPos.y = Mathf.Min(minPos.y, pos.y);
                        maxPos.x = Mathf.Max(maxPos.x, pos.x);
                        maxPos.y = Mathf.Max(maxPos.y, pos.y);
                    }
                }
                
                // Create background for group
                GameObject groupBg = new GameObject($"GroupBackground_{parentId}");
                groupBg.transform.SetParent(skillTreeContainer);
                groupBg.transform.SetAsFirstSibling(); // Put behind nodes
                
                RectTransform bgRect = groupBg.AddComponent<RectTransform>();
                float padding = 50f;
                bgRect.anchoredPosition = (minPos + maxPos) / 2f;
                bgRect.sizeDelta = new Vector2(
                    maxPos.x - minPos.x + padding * 2,
                    maxPos.y - minPos.y + padding * 2
                );
                
                Image bgImage = groupBg.AddComponent<Image>();
                bgImage.color = new Color(0.25f, 0.35f, 0.45f, 0.15f); // Very light semi-transparent blue-gray
                
                // Add group label
                GameObject labelObj = new GameObject($"GroupLabel_{parentId}");
                labelObj.transform.SetParent(groupBg.transform);
                
                RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.5f, 1f);
                labelRect.anchorMax = new Vector2(0.5f, 1f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = new Vector2(0, -20);
                labelRect.sizeDelta = new Vector2(200, 30);
                
                TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
                labelText.text = GetGroupName(parentId);
                labelText.fontSize = 18;
                labelText.fontStyle = FontStyles.Bold;
                labelText.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
                labelText.alignment = TextAlignmentOptions.Center;
            }
        }
    }
    
    string GetGroupName(string parentId)
    {
        // Remove UUID patterns and clean up the ID for display
        if (parentId == "ungrouped") return "Ungrouped Exercises";
        
        // Remove common UUID patterns
        string cleaned = System.Text.RegularExpressions.Regex.Replace(parentId, 
            @"[a-f0-9]{8}[\s\-]?[a-f0-9]{4}[\s\-]?[a-f0-9]{4}[\s\-]?[a-f0-9]{4}[\s\-]?[a-f0-9]{12}", 
            "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            // If the entire string was a UUID, show a short version
            return "Group " + parentId.Substring(0, Math.Min(8, parentId.Length)).ToUpper();
        }
        
        // Clean up remaining text
        cleaned = cleaned.Replace("_", " ").Replace("-", " ").Trim();
        
        // Capitalize first letter of each word
        var words = cleaned.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        
        return string.Join(" ", words);
    }

    void CreateExerciseNodeAtPosition(string exerciseId, Vector2 position)
    {
        Debug.Log($"Creating node for exercise: {exerciseId}");

        // Get exercise data
        ExerciseData exerciseData = exerciseDataMap.ContainsKey(exerciseId) ? exerciseDataMap[exerciseId] : null;
        if (exerciseData == null)
        {
            Debug.LogWarning($"No exercise data found for ID: {exerciseId}");
            // Try to get from parser directly
            var exercise = parser?.GetExercise(exerciseId);
            if (exercise != null)
            {
                exerciseData = new ExerciseData
                {
                    id = exercise.id,
                    name = exercise.name,
                    description_key = exercise.description_key,
                    position = exercise.position,
                    is_active = exercise.is_active,
                    tags = exercise.tags?.ToArray() ?? new string[0],
                    equipment_required = exercise.equipment_required?.ToArray() ?? new string[0],
                    benchmarks = exercise.benchmarks?.ToArray() ?? new string[0]
                };
                exerciseDataMap[exerciseId] = exerciseData;
            }
            else
            {
                // Create dummy data with the ID as name for debugging
                exerciseData = new ExerciseData
                {
                    id = exerciseId,
                    name = "Exercise " + exerciseId.Substring(0, Math.Min(8, exerciseId.Length)),
                    description_key = "",
                    position = 0,
                    is_active = true,
                    tags = new string[0],
                    equipment_required = new string[0],
                    benchmarks = new string[0]
                };
                exerciseDataMap[exerciseId] = exerciseData;
            }
        }

        // Use the button prefab if node prefab is not assigned
        GameObject prefabToUse = skillTreeNodePrefab != null ? skillTreeNodePrefab : skillTreeButtonPrefab;

        GameObject nodeObj;
        
        if (prefabToUse == null)
        {
            // Create a simple button if no prefab is assigned
            nodeObj = CreateFallbackNode(exerciseId, exerciseData.name);
        }
        else
        {
            // Instantiate the prefab
            nodeObj = Instantiate(prefabToUse, skillTreeContainer);
        }

        // Position the node
        RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position;
        }

        // Create the exercise node data
        ExerciseNode node = new ExerciseNode
        {
            gameObject = nodeObj,
            data = exerciseData
        };

        // Setup the node visuals and interaction
        SetupNodeVisualsAndInteraction(nodeObj, node, exerciseId);

        exerciseNodes[exerciseId] = node;
        
        // Setup hover effects
        if (animations != null)
        {
            animations.SetupHoverEffects(nodeObj);
        }
    }

    GameObject CreateFallbackNode(string exerciseId, string exerciseName)
    {
        GameObject nodeObj = new GameObject($"ExerciseNode_{exerciseId}");
        nodeObj.transform.SetParent(skillTreeContainer);

        // Add UI components
        RectTransform rectTransform = nodeObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(180, 60);

        // Background image
        Image bgImage = nodeObj.AddComponent<Image>();
        bgImage.color = new Color(0.35f, 0.45f, 0.55f, 0.95f); // Lighter blue-gray

        // Add button component
        Button button = nodeObj.AddComponent<Button>();
        button.targetGraphic = bgImage; // Set the target graphic for button

        // Create text object with proper setup
        GameObject textObj = new GameObject("NodeText");
        textObj.transform.SetParent(nodeObj.transform);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = exerciseName;
        text.fontSize = 14;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero; // Full size
        textRect.anchoredPosition = Vector2.zero;
        
        // Make sure text renders in front
        text.raycastTarget = false; // Don't block button clicks

        return nodeObj;
    }

    void SetupNodeVisualsAndInteraction(GameObject nodeObj, ExerciseNode node, string exerciseId)
    {
        // Try to find and setup BaseElement component (SharpUI)
        BaseElement baseElement = nodeObj.GetComponentInChildren<BaseElement>();
        
        if (baseElement != null)
        {
            node.button = baseElement;
            
            // Subscribe to click events using SharpUI's event system
            string capturedId = exerciseId;
            baseElement.GetEventListener().ObserveOnClicked()
                .Subscribe(_ => OnExerciseNodeClicked(capturedId))
                .AddTo(this);
        }
        else
        {
            // Fallback to standard Unity button
            Button standardButton = nodeObj.GetComponentInChildren<Button>();
            if (standardButton == null)
            {
                standardButton = nodeObj.GetComponent<Button>();
            }
            
            if (standardButton != null)
            {
                string capturedId = exerciseId;
                standardButton.onClick.RemoveAllListeners();
                standardButton.onClick.AddListener(() => OnExerciseNodeClicked(capturedId));
                Debug.Log($"Connected button click for exercise: {exerciseId}");
            }
            else
            {
                Debug.LogWarning($"No button found for exercise node: {exerciseId}");
            }
        }

        // Update text - look for any TextMeshProUGUI component
        TextMeshProUGUI[] textComponents = nodeObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (textComponents.Length > 0 && node.data != null)
        {
            // Update ALL text components to ensure name is shown
            foreach (var textComponent in textComponents)
            {
                // Only update if it's not already showing the correct name
                if (!textComponent.text.Contains(node.data.name))
                {
                    textComponent.text = node.data.name;
                }
                
                // Ensure text is visible and readable
                textComponent.fontSize = Mathf.Max(textComponent.fontSize, 14);
                if (textComponent.color.a < 0.8f)
                {
                    textComponent.color = new Color(1f, 1f, 1f, 1f);
                }
                textComponent.fontStyle = FontStyles.Bold;
            }
        }
        else if (node.data != null)
        {
            // If no text component exists, create one
            GameObject textObj = new GameObject("NodeText");
            textObj.transform.SetParent(nodeObj.transform);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = node.data.name;
            text.fontSize = 16;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-10, -10);
            textRect.anchoredPosition = Vector2.zero;
        }

        // Add a tooltip component to show more info on hover
        nodeObj.name = $"Node_{node.data?.name ?? exerciseId}";
    }

    void CreateConnection(string fromId, string toId, string colorHex)
    {
        if (!exerciseNodes.ContainsKey(fromId) || !exerciseNodes.ContainsKey(toId))
            return;

        ExerciseNode fromNode = exerciseNodes[fromId];
        ExerciseNode toNode = exerciseNodes[toId];

        // Add prerequisite relationship
        toNode.prerequisites.Add(fromId);

        // Create visual connection line
        GameObject line = skillTreeProgressLinePrefab != null ?
            Instantiate(skillTreeProgressLinePrefab, skillTreeContainer) :
            CreateSimpleLine();

        if (line != null)
        {
            // Position line between nodes
            RectTransform fromRect = fromNode.gameObject.GetComponent<RectTransform>();
            RectTransform toRect = toNode.gameObject.GetComponent<RectTransform>();
            RectTransform lineRect = line.GetComponent<RectTransform>();

            // Calculate line position and rotation
            Vector2 fromPos = fromRect.anchoredPosition;
            Vector2 toPos = toRect.anchoredPosition;
            Vector2 direction = toPos - fromPos;
            float distance = direction.magnitude;

            lineRect.anchoredPosition = (fromPos + toPos) / 2f;
            lineRect.sizeDelta = new Vector2(distance, 5f); // 5f is line thickness

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lineRect.rotation = Quaternion.Euler(0, 0, angle);

            // Set line color
            Image lineImage = line.GetComponent<Image>();
            Color color;
            if (lineImage != null && ColorUtility.TryParseHtmlString(colorHex, out color))
            {
                lineImage.color = color;
            }
            else
            {
                color = new Color(0.23f, 0.51f, 0.96f, 0.5f); // Default blue if parse fails
                if (lineImage != null)
                {
                    lineImage.color = color;
                }
            }

            // Make sure line renders behind nodes
            line.transform.SetAsFirstSibling();

            fromNode.connectionLines.Add(line);
            
            // Animate the connection line
            if (animations != null)
            {
                animations.AnimateConnectionLine(line, color);
            }
        }
    }

    GameObject CreateSimpleLine()
    {
        GameObject line = new GameObject("ConnectionLine");
        line.transform.SetParent(skillTreeContainer);
        RectTransform rectTransform = line.AddComponent<RectTransform>();
        Image image = line.AddComponent<Image>();
        image.color = new Color(0.23f, 0.51f, 0.96f, 0.5f); // Semi-transparent blue
        return line;
    }

    void SetupProgressionLogic()
    {
        Debug.Log("Setting up progression logic...");

        // Initialize first exercises as unlocked
        foreach (var progression in progressions)
        {
            foreach (var chain in progression.chains)
            {
                if (chain.exercise_ids.Count > 0)
                {
                    string firstExerciseId = chain.exercise_ids[0];
                    if (exerciseNodes.ContainsKey(firstExerciseId))
                    {
                        UnlockExercise(firstExerciseId);
                    }
                }
            }
        }
    }

    void OnExerciseNodeClicked(string exerciseId)
    {
        Debug.Log($"Exercise node clicked: {exerciseId}");

        if (!exerciseNodes.ContainsKey(exerciseId))
        {
            Debug.LogWarning($"Exercise {exerciseId} not found in exerciseNodes dictionary");
            return;
        }

        ExerciseNode node = exerciseNodes[exerciseId];
        Debug.Log($"Node state - Unlocked: {node.isUnlocked}, Completed: {node.isCompleted}");

        // Always show the modal regardless of state (for testing)
        ShowExerciseModal(node);
    }

    void ShowExerciseModal(ExerciseNode node)
    {
        Debug.Log($"Showing modal for: {node.data?.name}");

        if (detailModal == null)
        {
            // Try to find existing modal
            detailModal = GameObject.FindFirstObjectByType<ExerciseDetailModal>();
            
            // Create modal if it doesn't exist
            if (detailModal == null)
            {
                GameObject modalObj = new GameObject("ExerciseDetailModal");
                if (mainCanvas != null)
                {
                    modalObj.transform.SetParent(mainCanvas.transform);
                }
                else
                {
                    // Find or create a canvas
                    Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                    if (canvas == null)
                    {
                        GameObject canvasObj = new GameObject("Canvas");
                        canvas = canvasObj.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvasObj.AddComponent<CanvasScaler>();
                        canvasObj.AddComponent<GraphicRaycaster>();
                    }
                    modalObj.transform.SetParent(canvas.transform);
                }
                
                RectTransform modalRect = modalObj.AddComponent<RectTransform>();
                modalRect.anchorMin = Vector2.zero;
                modalRect.anchorMax = Vector2.one;
                modalRect.sizeDelta = Vector2.zero;
                modalRect.anchoredPosition = Vector2.zero;
                
                detailModal = modalObj.AddComponent<ExerciseDetailModal>();
            }
        }
        
        // Get exercise description from glossary
        string description = "";
        if (parser != null && node.data != null)
        {
            description = parser.GetDescription(node.data.description_key);
        }
        
        // Get prerequisite names
        List<string> prerequisiteNames = new List<string>();
        foreach (string prereqId in node.prerequisites)
        {
            if (exerciseNodes.ContainsKey(prereqId))
            {
                ExerciseNode prereqNode = exerciseNodes[prereqId];
                if (prereqNode.data != null)
                {
                    prerequisiteNames.Add(prereqNode.data.name);
                }
            }
        }
        
        // Convert Exercise to CurriculumSystem.Exercise
        var exercise = parser.GetExercise(node.data.id);
        if (exercise != null)
        {
            // Show the modal
            detailModal.ShowExerciseDetails(
                exercise,
                description,
                node.isUnlocked,
                node.isCompleted,
                prerequisiteNames,
                OnStartExercise,
                CompleteExercise
            );
        }
    }
    
    void OnStartExercise(string exerciseId)
    {
        Debug.Log($"Starting exercise: {exerciseId}");
        // Here you could integrate with tracking, timers, etc.
    }

    void ShowLockedMessage(ExerciseNode node)
    {
        Debug.Log($"Exercise '{node.data?.name}' is locked. Complete prerequisites first!");
        
        // Still show the modal but with locked state
        ShowExerciseModal(node);
    }

    void CompleteExercise(string exerciseId)
    {
        Debug.Log($"CompleteExercise called for: {exerciseId}");
        
        if (!exerciseNodes.ContainsKey(exerciseId))
        {
            Debug.LogWarning($"Exercise {exerciseId} not found in nodes dictionary");
            return;
        }

        ExerciseNode node = exerciseNodes[exerciseId];
        node.isCompleted = true;
        completedExercises[exerciseId] = true;

        Debug.Log($"Completed exercise: {node.data?.name} (ID: {exerciseId})");

        // Update visual state
        UpdateNodeVisual(node);

        // Check and unlock next exercises in the progression
        UnlockNextExercises(exerciseId);

        // Play complete animation
        if (animations != null && node.gameObject != null)
        {
            animations.AnimateNodeComplete(node.gameObject);
        }
        
        // Save progress
        SaveProgress();
    }

    void UnlockNextExercises(string completedExerciseId)
    {
        foreach (var kvp in exerciseNodes)
        {
            ExerciseNode node = kvp.Value;
            if (node.prerequisites.Contains(completedExerciseId))
            {
                bool allPrerequisitesComplete = node.prerequisites.All(
                    preReqId => completedExercises.ContainsKey(preReqId) && completedExercises[preReqId]
                );

                if (allPrerequisitesComplete && !node.isUnlocked)
                {
                    UnlockExercise(kvp.Key);
                }
            }
        }
    }

    void UnlockExercise(string exerciseId)
    {
        if (!exerciseNodes.ContainsKey(exerciseId))
            return;

        ExerciseNode node = exerciseNodes[exerciseId];
        node.isUnlocked = true;

        UpdateNodeVisual(node);
        
        // Play unlock animation
        if (animations != null && node.gameObject != null)
        {
            animations.AnimateNodeUnlock(node.gameObject);
        }
    }

    void UpdateNodeVisual(ExerciseNode node)
    {
        // Define colors for different states
        Color completedColor = new Color(0.2f, 0.8f, 0.3f, 1f);     // Green
        Color availableColor = new Color(0.3f, 0.6f, 0.9f, 1f);     // Blue
        Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);      // Dark gray
        
        Color completedTextColor = Color.white;
        Color availableTextColor = Color.white;
        Color lockedTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);    // Light gray

        // Update button state if using SharpUI
        if (node.button != null)
        {
            if (node.isCompleted)
            {
                node.button.SetSelected(true);
                node.button.SetClickable(false);
            }
            else if (node.isUnlocked)
            {
                node.button.SetClickable(true);
                node.button.isEnabled = true;
            }
            else
            {
                node.button.isEnabled = false;
                node.button.SetClickable(false);
            }
        }
        else
        {
            // Handle standard Unity button
            Button standardButton = node.gameObject.GetComponentInChildren<Button>();
            if (standardButton == null)
                standardButton = node.gameObject.GetComponent<Button>();
                
            if (standardButton != null)
            {
                standardButton.interactable = node.isUnlocked || node.isCompleted;
            }
        }

        // Update visual appearance - get the main image component
        Image nodeImage = node.gameObject.GetComponent<Image>();
        if (nodeImage != null)
        {
            if (node.isCompleted)
            {
                nodeImage.color = completedColor;
                Debug.Log($"Set node {node.data?.name} to completed color (green)");
            }
            else if (node.isUnlocked)
            {
                nodeImage.color = availableColor;
                Debug.Log($"Set node {node.data?.name} to available color (blue)");
            }
            else
            {
                nodeImage.color = lockedColor;
                Debug.Log($"Set node {node.data?.name} to locked color (gray)");
            }
        }
        else
        {
            // Try to find any Image in children
            Image[] images = node.gameObject.GetComponentsInChildren<Image>();
            if (images.Length > 0)
            {
                foreach (Image img in images)
                {
                    if (node.isCompleted)
                        img.color = completedColor;
                    else if (node.isUnlocked)
                        img.color = availableColor;
                    else
                        img.color = lockedColor;
                }
                Debug.Log($"Updated {images.Length} child images for node {node.data?.name}");
            }
            else
            {
                Debug.LogWarning($"No Image component found on node {node.data?.name}");
            }
        }

        // Update text color
        TextMeshProUGUI[] texts = node.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (node.isCompleted)
            {
                text.color = completedTextColor;
                text.fontStyle = FontStyles.Bold | FontStyles.Strikethrough;
            }
            else if (node.isUnlocked)
            {
                text.color = availableTextColor;
                text.fontStyle = FontStyles.Bold;
            }
            else
            {
                text.color = lockedTextColor;
                text.fontStyle = FontStyles.Normal;
            }
        }

        // Add visual indicators
        AddStateIndicator(node);

        // Update connection lines
        foreach (var line in node.connectionLines)
        {
            Image lineImage = line.GetComponent<Image>();
            if (lineImage != null)
            {
                Color lineColor = lineImage.color;
                if (node.isCompleted)
                {
                    lineColor.a = 1f;
                    // Make the line slightly brighter
                    lineColor = Color.Lerp(lineColor, Color.white, 0.2f);
                }
                else if (node.isUnlocked)
                {
                    lineColor.a = 0.7f;
                }
                else
                {
                    lineColor.a = 0.3f;
                }
                lineImage.color = lineColor;
            }
        }
    }

    void AddStateIndicator(ExerciseNode node)
    {
        // Look for or create a state indicator
        Transform indicatorTransform = node.gameObject.transform.Find("StateIndicator");
        GameObject indicator;
        
        if (indicatorTransform == null)
        {
            indicator = new GameObject("StateIndicator");
            indicator.transform.SetParent(node.gameObject.transform);
            
            RectTransform rect = indicator.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-10, -10);
            rect.sizeDelta = new Vector2(20, 20);
            
            TextMeshProUGUI indicatorText = indicator.AddComponent<TextMeshProUGUI>();
            indicatorText.fontSize = 16;
            indicatorText.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            indicator = indicatorTransform.gameObject;
        }
        
        TextMeshProUGUI text = indicator.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            if (node.isCompleted)
            {
                text.text = "✓";
                text.color = Color.white;
            }
            else if (node.isUnlocked)
            {
                text.text = "●";
                text.color = new Color(0.9f, 0.9f, 0.3f, 1f); // Yellow dot
            }
            else
            {
                text.text = "🔒";
                text.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            }
        }
    }

    void SaveProgress()
    {
        // Implement your save system here
        // You can use PlayerPrefs or a more robust save system

        foreach (var kvp in completedExercises)
        {
            PlayerPrefs.SetInt($"Exercise_Completed_{kvp.Key}", kvp.Value ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        // Load saved progress
        foreach (var kvp in exerciseNodes)
        {
            string exerciseId = kvp.Key;
            bool isCompleted = PlayerPrefs.GetInt($"Exercise_Completed_{exerciseId}", 0) == 1;

            if (isCompleted)
            {
                completedExercises[exerciseId] = true;
                kvp.Value.isCompleted = true;
                UpdateNodeVisual(kvp.Value);
            }
        }
    }
    
    void ClearSkillTree()
    {
        // Stop all animations first to prevent null reference exceptions
        if (animations != null)
        {
            animations.StopAllAnimations();
        }
        
        // Clear existing exercise nodes
        foreach (var node in exerciseNodes.Values)
        {
            if (node.gameObject != null)
            {
                // Remove any running coroutines on the node
                NodeHoverEffect hoverEffect = node.gameObject.GetComponent<NodeHoverEffect>();
                if (hoverEffect != null)
                {
                    hoverEffect.StopAllCoroutines();
                }
                
                Destroy(node.gameObject);
            }
            foreach (var line in node.connectionLines)
            {
                if (line != null)
                {
                    // Stop line animations
                    LineFlowAnimation flowAnim = line.GetComponent<LineFlowAnimation>();
                    if (flowAnim != null)
                    {
                        flowAnim.enabled = false;
                    }
                    
                    Destroy(line);
                }
            }
        }
        exerciseNodes.Clear();
        
        // Clear hierarchy nodes
        foreach (var node in hierarchyNodes.Values)
        {
            if (node != null)
            {
                // Remove any running coroutines on the node
                NodeHoverEffect hoverEffect = node.GetComponent<NodeHoverEffect>();
                if (hoverEffect != null)
                {
                    hoverEffect.StopAllCoroutines();
                }
                
                Destroy(node);
            }
        }
        hierarchyNodes.Clear();
        
        // Clear root node
        if (rootNode != null)
        {
            NodeHoverEffect hoverEffect = rootNode.GetComponent<NodeHoverEffect>();
            if (hoverEffect != null)
            {
                hoverEffect.StopAllCoroutines();
            }
            
            Destroy(rootNode);
            rootNode = null;
        }
        
        // Clear all remaining children of the container (including lines)
        foreach (Transform child in skillTreeContainer)
        {
            // Stop any animations on child objects
            NodeHoverEffect hoverEffect = child.GetComponent<NodeHoverEffect>();
            if (hoverEffect != null)
            {
                hoverEffect.StopAllCoroutines();
            }
            
            LineFlowAnimation flowAnim = child.GetComponent<LineFlowAnimation>();
            if (flowAnim != null)
            {
                flowAnim.enabled = false;
            }
            
            Destroy(child.gameObject);
        }
    }
    
    void OnDestroy()
    {
        if (parser != null)
        {
            parser.OnDataLoaded -= OnDataLoaded;
            parser.OnLoadingStatusChanged -= OnLoadingStatusChanged;
        }
    }
}