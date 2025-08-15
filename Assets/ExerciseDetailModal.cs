using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CurriculumSystem;

public class ExerciseDetailModal : MonoBehaviour
{
    [Header("Modal UI Elements")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI equipmentText;
    [SerializeField] private TextMeshProUGUI benchmarksText;
    [SerializeField] private TextMeshProUGUI tagsText;
    [SerializeField] private TextMeshProUGUI prerequisitesText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image backgroundOverlay;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // Current exercise data
    private Exercise currentExercise;
    private System.Action<string> onStartCallback;
    private System.Action<string> onCompleteCallback;
    
    // Animation state
    private bool isAnimating = false;
    private float animationTimer = 0f;
    private bool isShowing = false;
    
    void Awake()
    {
        // Create modal structure if not provided
        if (modalPanel == null)
        {
            CreateModalStructure();
        }
        
        // Setup button listeners
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
        
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartExercise);
        }
        
        if (completeButton != null)
        {
            completeButton.onClick.RemoveAllListeners();
            completeButton.onClick.AddListener(OnCompleteExercise);
        }
        
        // Hide initially
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
            backgroundOverlay.raycastTarget = false;
        }
    }
    
    void CreateModalStructure()
    {
        // Create background overlay
        GameObject overlayObj = new GameObject("ModalOverlay");
        overlayObj.transform.SetParent(transform);
        RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.anchoredPosition = Vector2.zero;
        
        backgroundOverlay = overlayObj.AddComponent<Image>();
        backgroundOverlay.color = new Color(0, 0, 0, 0.7f);
        backgroundOverlay.raycastTarget = true;
        
        Button overlayButton = overlayObj.AddComponent<Button>();
        overlayButton.transition = Selectable.Transition.None;
        overlayButton.onClick.AddListener(Hide);
        
        // Create modal panel
        modalPanel = new GameObject("ModalPanel");
        modalPanel.transform.SetParent(transform);
        RectTransform panelRect = modalPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(600, 500);
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = modalPanel.AddComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        
        // Create content container
        GameObject contentContainer = new GameObject("Content");
        contentContainer.transform.SetParent(modalPanel.transform);
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.sizeDelta = new Vector2(-40, -40);
        contentRect.anchoredPosition = Vector2.zero;
        
        VerticalLayoutGroup layout = contentContainer.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 15;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        
        // Create title
        GameObject titleObj = CreateTextElement("Title", contentContainer.transform, 24, FontStyles.Bold);
        titleText = titleObj.GetComponent<TextMeshProUGUI>();
        
        // Create description
        GameObject descObj = CreateTextElement("Description", contentContainer.transform, 16, FontStyles.Normal);
        descriptionText = descObj.GetComponent<TextMeshProUGUI>();
        
        // Create equipment
        GameObject equipObj = CreateTextElement("Equipment", contentContainer.transform, 14, FontStyles.Normal);
        equipmentText = equipObj.GetComponent<TextMeshProUGUI>();
        
        // Create benchmarks
        GameObject benchObj = CreateTextElement("Benchmarks", contentContainer.transform, 14, FontStyles.Normal);
        benchmarksText = benchObj.GetComponent<TextMeshProUGUI>();
        
        // Create tags
        GameObject tagsObj = CreateTextElement("Tags", contentContainer.transform, 12, FontStyles.Italic);
        tagsText = tagsObj.GetComponent<TextMeshProUGUI>();
        
        // Create prerequisites
        GameObject prereqObj = CreateTextElement("Prerequisites", contentContainer.transform, 14, FontStyles.Normal);
        prerequisitesText = prereqObj.GetComponent<TextMeshProUGUI>();
        
        // Create button container
        GameObject buttonContainer = new GameObject("Buttons");
        buttonContainer.transform.SetParent(modalPanel.transform);
        RectTransform buttonRect = buttonContainer.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.sizeDelta = new Vector2(-40, 60);
        buttonRect.anchoredPosition = new Vector2(0, 30);
        
        HorizontalLayoutGroup buttonLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 10;
        buttonLayout.childForceExpandHeight = true;
        buttonLayout.childForceExpandWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childControlWidth = false;
        
        // Create buttons
        startButton = CreateButton("Start Exercise", buttonContainer.transform, new Color(0.3f, 0.6f, 0.9f, 1f));
        completeButton = CreateButton("Mark Complete", buttonContainer.transform, new Color(0.2f, 0.8f, 0.3f, 1f));
        closeButton = CreateButton("Close", buttonContainer.transform, new Color(0.5f, 0.5f, 0.5f, 1f));
    }
    
    GameObject CreateTextElement(string name, Transform parent, int fontSize, FontStyles fontStyle)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        
        LayoutElement layoutElement = obj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = fontSize * 2;
        
        return obj;
    }
    
    Button CreateButton(string label, Transform parent, Color color)
    {
        GameObject buttonObj = new GameObject(label);
        buttonObj.transform.SetParent(parent);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 40);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = color;
        
        Button button = buttonObj.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 14;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        return button;
    }
    
    public void ShowExerciseDetails(
        Exercise exercise, 
        string description,
        bool isUnlocked,
        bool isCompleted,
        List<string> prerequisiteNames,
        System.Action<string> onStart,
        System.Action<string> onComplete)
    {
        if (exercise == null) return;
        
        currentExercise = exercise;
        onStartCallback = onStart;
        onCompleteCallback = onComplete;
        
        // Update UI elements
        if (titleText != null)
        {
            titleText.text = exercise.name;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = !string.IsNullOrEmpty(description) ? description : "No description available.";
        }
        
        if (equipmentText != null)
        {
            if (exercise.equipment_required != null && exercise.equipment_required.Count > 0)
            {
                equipmentText.text = "<b>Equipment:</b> " + string.Join(", ", exercise.equipment_required);
                equipmentText.gameObject.SetActive(true);
            }
            else
            {
                equipmentText.text = "<b>Equipment:</b> None required";
            }
        }
        
        if (benchmarksText != null)
        {
            if (exercise.benchmarks != null && exercise.benchmarks.Count > 0)
            {
                benchmarksText.text = "<b>Benchmarks:</b>\n" + string.Join("\n", exercise.benchmarks.ConvertAll(b => "• " + b));
                benchmarksText.gameObject.SetActive(true);
            }
            else
            {
                benchmarksText.gameObject.SetActive(false);
            }
        }
        
        if (tagsText != null)
        {
            if (exercise.tags != null && exercise.tags.Count > 0)
            {
                tagsText.text = "<i>Tags: " + string.Join(", ", exercise.tags) + "</i>";
                tagsText.gameObject.SetActive(true);
            }
            else
            {
                tagsText.gameObject.SetActive(false);
            }
        }
        
        if (prerequisitesText != null)
        {
            if (prerequisiteNames != null && prerequisiteNames.Count > 0)
            {
                prerequisitesText.text = "<b>Prerequisites:</b> " + string.Join(", ", prerequisiteNames);
                prerequisitesText.gameObject.SetActive(true);
            }
            else
            {
                prerequisitesText.gameObject.SetActive(false);
            }
        }
        
        // Update button states
        if (startButton != null)
        {
            startButton.gameObject.SetActive(isUnlocked && !isCompleted);
        }
        
        if (completeButton != null)
        {
            completeButton.gameObject.SetActive(isUnlocked && !isCompleted);
        }
        
        // Show the modal
        Show();
    }
    
    void Show()
    {
        if (modalPanel == null) return;
        
        modalPanel.SetActive(true);
        
        // Enable overlay if it exists
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
            backgroundOverlay.raycastTarget = true;
            Color color = backgroundOverlay.color;
            color.a = 0f;
            backgroundOverlay.color = color;
        }
        
        isShowing = true;
        isAnimating = true;
        animationTimer = 0f;
        
        // Start with scale 0
        modalPanel.transform.localScale = Vector3.zero;
    }
    
    public void Hide()
    {
        isShowing = false;
        isAnimating = true;
        animationTimer = 0f;
    }
    
    void Update()
    {
        if (!isAnimating) return;
        
        animationTimer += Time.deltaTime;
        float t = animationTimer / animationDuration;
        
        if (t >= 1f)
        {
            t = 1f;
            isAnimating = false;
            
            if (!isShowing)
            {
                modalPanel.SetActive(false);
                
                // Also disable overlay to prevent blocking
                if (backgroundOverlay != null)
                {
                    backgroundOverlay.gameObject.SetActive(false);
                    backgroundOverlay.raycastTarget = false;
                }
            }
        }
        
        float curveValue = showCurve.Evaluate(t);
        
        if (isShowing)
        {
            // Animate in
            modalPanel.transform.localScale = Vector3.one * curveValue;
            
            if (backgroundOverlay != null)
            {
                Color color = backgroundOverlay.color;
                color.a = 0.7f * curveValue;
                backgroundOverlay.color = color;
            }
        }
        else
        {
            // Animate out
            modalPanel.transform.localScale = Vector3.one * (1f - curveValue);
            
            if (backgroundOverlay != null)
            {
                Color color = backgroundOverlay.color;
                color.a = 0.7f * (1f - curveValue);
                backgroundOverlay.color = color;
            }
        }
    }
    
    void OnStartExercise()
    {
        if (currentExercise != null && onStartCallback != null)
        {
            onStartCallback(currentExercise.id);
        }
        Hide();
    }
    
    void OnCompleteExercise()
    {
        if (currentExercise != null && onCompleteCallback != null)
        {
            onCompleteCallback(currentExercise.id);
        }
        Hide();
    }
}