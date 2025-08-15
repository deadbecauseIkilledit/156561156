using UnityEngine;
using UnityEngine.UI;

public class SceneSetupFixer : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("SceneSetupFixer: Checking for scene issues...");
        
        // Fix Canvas scale issues
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                // Check for zero scale
                if (canvasRect.localScale == Vector3.zero || canvasRect.localScale.magnitude < 0.1f)
                {
                    Debug.LogWarning($"Fixed Canvas '{canvas.name}' with zero scale. Setting to (1,1,1)");
                    canvasRect.localScale = Vector3.one;
                }
                
                // For Screen Space - Overlay canvases, ensure proper setup
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvasRect.anchorMin = Vector2.zero;
                    canvasRect.anchorMax = Vector2.one;
                    canvasRect.sizeDelta = Vector2.zero;
                    canvasRect.anchoredPosition = Vector2.zero;
                }
            }
        }
        
        // Check for CanvasScaler and add if missing
        foreach (Canvas canvas in canvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                Debug.Log($"Added CanvasScaler to '{canvas.name}'");
            }
        }
        
        // Clean up any stray UI elements outside of Canvas
        RectTransform[] allRectTransforms = FindObjectsOfType<RectTransform>();
        foreach (RectTransform rect in allRectTransforms)
        {
            // Check if this RectTransform is not under a Canvas
            Canvas parentCanvas = rect.GetComponentInParent<Canvas>();
            if (parentCanvas == null && rect.GetComponent<Canvas>() == null)
            {
                // This is a UI element not under any canvas
                Debug.LogWarning($"Found orphaned UI element '{rect.name}' at position {rect.anchoredPosition}. Disabling it.");
                
                // Check if it's far off screen (likely the visual artifact)
                if (Mathf.Abs(rect.anchoredPosition.x) > 2000 || Mathf.Abs(rect.anchoredPosition.y) > 2000)
                {
                    rect.gameObject.SetActive(false);
                    Debug.Log($"Disabled off-screen UI element '{rect.name}'");
                }
            }
        }
        
        // Ensure EventSystem exists
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("Created missing EventSystem");
        }
        
        // Fix LoadingPanel if it exists
        GameObject loadingPanel = GameObject.Find("LoadingPanel");
        if (loadingPanel != null)
        {
            RectTransform loadingRect = loadingPanel.GetComponent<RectTransform>();
            if (loadingRect != null)
            {
                // Center the loading panel
                loadingRect.anchorMin = new Vector2(0.5f, 0.5f);
                loadingRect.anchorMax = new Vector2(0.5f, 0.5f);
                loadingRect.pivot = new Vector2(0.5f, 0.5f);
                loadingRect.anchoredPosition = Vector2.zero;
                
                // Set reasonable size if needed
                if (loadingRect.sizeDelta.x < 100 || loadingRect.sizeDelta.y < 100)
                {
                    loadingRect.sizeDelta = new Vector2(400, 200);
                }
            }
        }
        
        Debug.Log("SceneSetupFixer: Scene setup complete");
    }
    
    void Start()
    {
        // Additional cleanup after all objects are initialized
        CleanupVisualArtifacts();
    }
    
    void CleanupVisualArtifacts()
    {
        // Find and disable any objects that might be causing corner artifacts
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Skip if object is already inactive
            if (!obj.activeInHierarchy) continue;
            
            // Check for objects with suspicious names that might be test objects
            string objName = obj.name.ToLower();
            if (objName.Contains("test") || objName.Contains("debug") || objName.Contains("temp"))
            {
                // Check if it has a renderer that might be visible
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.enabled)
                {
                    Debug.LogWarning($"Disabling potential test object: {obj.name}");
                    obj.SetActive(false);
                }
            }
            
            // Check for UI elements in corners
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null && rect.GetComponentInParent<Canvas>() != null)
            {
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, rect.position);
                
                // Check if element is in corner of screen
                if ((screenPos.x < 100 || screenPos.x > Screen.width - 100) && 
                    (screenPos.y < 100 || screenPos.y > Screen.height - 100))
                {
                    // Check if it's not a known UI element
                    if (!IsKnownUIElement(obj))
                    {
                        Debug.LogWarning($"Found corner UI element: {obj.name} at screen position {screenPos}");
                        // Don't automatically disable, just log for now
                    }
                }
            }
        }
    }
    
    bool IsKnownUIElement(GameObject obj)
    {
        string name = obj.name.ToLower();
        return name.Contains("skill") || 
               name.Contains("loading") || 
               name.Contains("canvas") || 
               name.Contains("event") ||
               name.Contains("modal") ||
               name.Contains("button");
    }
}