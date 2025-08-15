using UnityEngine;
using UnityEngine.UI;

public class SkillTreeTestConfig : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private bool useFirestore = false;
    [SerializeField] private bool createDebugUI = true;
    
    private SimpleCurriculumParser parser;
    private ExerciseSkillTreeManager treeManager;
    
    void Start()
    {
        if (!runTestOnStart) return;
        
        Debug.Log("=== SKILL TREE TEST CONFIGURATION STARTING ===");
        
        // Configure parser for sample data
        parser = GetComponent<SimpleCurriculumParser>();
        if (parser == null)
        {
            parser = gameObject.AddComponent<SimpleCurriculumParser>();
        }
        
        // Force parser to use sample data
        var parserType = parser.GetType();
        var useFirestoreField = parserType.GetField("useFirestore", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (useFirestoreField != null)
        {
            useFirestoreField.SetValue(parser, false);
            Debug.Log("Configured parser to use sample data (Firestore disabled)");
        }
        
        // Configure tree manager
        treeManager = GetComponent<ExerciseSkillTreeManager>();
        if (treeManager != null)
        {
            Debug.Log("ExerciseSkillTreeManager found and ready");
        }
        
        // Create debug UI if needed
        if (createDebugUI)
        {
            CreateDebugControls();
        }
        
        // Verify scene setup
        VerifySceneSetup();
        
        Debug.Log("=== TEST CONFIGURATION COMPLETE ===");
        Debug.Log("The skill tree should now display with sample data:");
        Debug.Log("- Root: Fitness Curriculum");
        Debug.Log("- Discipline: Calisthenics");
        Debug.Log("- Curriculum: Beginner Program");
        Debug.Log("- Groups: Push Exercises, Pull Exercises");
        Debug.Log("- Exercises: Push-ups → Diamond Push-ups → Archer Push-ups");
        Debug.Log("- Exercises: Pull-ups → Muscle-ups");
    }
    
    void VerifySceneSetup()
    {
        // Check Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene!");
        }
        else
        {
            Debug.Log($"✓ Canvas found: {canvas.name}");
            
            // Verify Canvas scale
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect.localScale == Vector3.zero)
            {
                Debug.LogWarning("Canvas had zero scale - fixing...");
                canvasRect.localScale = Vector3.one;
            }
        }
        
        // Check SkillTreeContainer
        GameObject container = GameObject.Find("SkillTreeContainer");
        if (container == null)
        {
            Debug.LogError("SkillTreeContainer not found!");
        }
        else
        {
            Debug.Log("✓ SkillTreeContainer found");
            
            // Check camera controller
            SkillTreeCameraController cameraController = container.GetComponent<SkillTreeCameraController>();
            if (cameraController == null)
            {
                Debug.LogWarning("SkillTreeCameraController not found on container - adding...");
                cameraController = container.AddComponent<SkillTreeCameraController>();
            }
        }
        
        // Check EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("EventSystem not found - creating...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        else
        {
            Debug.Log("✓ EventSystem found");
        }
    }
    
    void CreateDebugControls()
    {
        // Find or create a canvas for debug controls
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Create debug panel
        GameObject debugPanel = new GameObject("DebugPanel");
        debugPanel.transform.SetParent(canvas.transform);
        
        RectTransform panelRect = debugPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(200, 150);
        
        Image panelBg = debugPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);
        
        // Add vertical layout group
        VerticalLayoutGroup layoutGroup = debugPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.spacing = 5;
        
        // Create buttons for different layout types
        CreateDebugButton(debugPanel, "Hierarchical Layout", () => SetLayoutType(ExerciseSkillTreeManager.LayoutType.Hierarchical));
        CreateDebugButton(debugPanel, "Radial Layout", () => SetLayoutType(ExerciseSkillTreeManager.LayoutType.Radial));
        CreateDebugButton(debugPanel, "Force-Directed", () => SetLayoutType(ExerciseSkillTreeManager.LayoutType.ForceDirected));
        CreateDebugButton(debugPanel, "Reload Tree", ReloadTree);
        
        Debug.Log("Debug controls created in top-left corner");
    }
    
    void CreateDebugButton(GameObject parent, string text, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(180, 30);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.3f, 0.5f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(() => onClick());
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
    }
    
    void SetLayoutType(ExerciseSkillTreeManager.LayoutType layoutType)
    {
        if (treeManager == null) return;
        
        // Use reflection to set the layout type
        var fieldInfo = treeManager.GetType().GetField("layoutType",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
            
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(treeManager, layoutType);
            Debug.Log($"Layout type changed to: {layoutType}");
            ReloadTree();
        }
    }
    
    void ReloadTree()
    {
        if (treeManager == null) return;
        
        // Use reflection to call BuildSkillTree
        var methodInfo = treeManager.GetType().GetMethod("BuildSkillTree",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
            
        if (methodInfo != null)
        {
            // First clear the tree
            var clearMethod = treeManager.GetType().GetMethod("ClearSkillTree",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
                
            if (clearMethod != null)
            {
                clearMethod.Invoke(treeManager, null);
            }
            
            // Then rebuild
            methodInfo.Invoke(treeManager, null);
            Debug.Log("Skill tree reloaded");
        }
    }
}