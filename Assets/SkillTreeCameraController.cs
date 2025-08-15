using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class SkillTreeCameraController : MonoBehaviour
{
    [Header("Camera Target")]
    [SerializeField] private RectTransform skillTreeContainer;
    
    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private float smoothPanSpeed = 5f;
    [SerializeField] private bool useSmoothPan = true;
    [SerializeField] private Vector2 panLimits = new Vector2(2000f, 2000f);
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float smoothZoomSpeed = 5f;
    [SerializeField] private bool useSmoothZoom = true;
    [SerializeField] private float minZoom = 0.3f;
    [SerializeField] private float maxZoom = 2f;
    
    [Header("Input Settings")]
    [SerializeField] private bool enableMouseDrag = true;
    [SerializeField] private bool enableTouchGestures = true;
    [SerializeField] private bool enableKeyboardPan = true;
    [SerializeField] private float keyboardPanSpeed = 500f;
    
    [Header("Focus Animation")]
    [SerializeField] private float focusAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve focusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // Input System references
    private Mouse mouse;
    private Keyboard keyboard;
    private Touchscreen touchscreen;
    private Gamepad gamepad;
    
    // State tracking
    private bool isDragging = false;
    private Vector2 lastMousePosition;
    private Vector2 dragStartPosition;
    private Vector2 containerStartPosition;
    private float lastClickTime = 0f;
    
    // Zoom state
    private float currentZoom = 1f;
    private float targetZoom = 1f;
    
    // Pan state
    private Vector2 currentPan;
    private Vector2 targetPan;
    
    // Touch tracking for pinch zoom
    private float lastPinchDistance;
    private bool isPinching = false;
    
    // Focus animation
    private bool isFocusing = false;
    private float focusTimer = 0f;
    private Vector2 focusStartPosition;
    private Vector2 focusEndPosition;
    private float focusStartZoom;
    private float focusEndZoom;
    
    // Bounds calculation
    private Rect containerBounds;
    
    void Start()
    {
        if (skillTreeContainer == null)
        {
            skillTreeContainer = GetComponent<RectTransform>();
            if (skillTreeContainer == null)
            {
                Debug.LogError("SkillTreeCameraController: No skill tree container assigned!");
                enabled = false;
                return;
            }
        }
        
        // Ensure EventSystem exists for UI interaction
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("Created EventSystem with InputSystemUIInputModule for UI interaction");
        }
        
        // Initialize input devices
        mouse = Mouse.current;
        keyboard = Keyboard.current;
        touchscreen = Touchscreen.current;
        gamepad = Gamepad.current;
        
        // Enable enhanced touch support for mobile
        EnhancedTouchSupport.Enable();
        
        // Initialize state
        currentPan = skillTreeContainer.anchoredPosition;
        targetPan = currentPan;
        currentZoom = skillTreeContainer.localScale.x;
        targetZoom = currentZoom;
        
        CalculateContainerBounds();
    }
    
    void OnDestroy()
    {
        // Disable enhanced touch when not needed
        EnhancedTouchSupport.Disable();
    }
    
    void Update()
    {
        // Update input device references if they change
        if (mouse == null) mouse = Mouse.current;
        if (keyboard == null) keyboard = Keyboard.current;
        if (touchscreen == null) touchscreen = Touchscreen.current;
        if (gamepad == null) gamepad = Gamepad.current;
        
        HandleFocusAnimation();
        
        if (!isFocusing)
        {
            HandleMouseInput();
            HandleTouchInput();
            HandleKeyboardInput();
            HandleScrollWheel();
        }
        
        ApplyPanAndZoom();
    }
    
    void HandleMouseInput()
    {
        if (!enableMouseDrag || mouse == null) return;
        
        bool leftButtonPressed = mouse.leftButton.wasPressedThisFrame;
        bool leftButtonHeld = mouse.leftButton.isPressed;
        Vector2 mousePosition = mouse.position.ReadValue();
        
        // Simplified drag logic - allow dragging from anywhere
        if (leftButtonPressed && !isDragging)
        {
            // Check for double-click first
            if (Time.time - lastClickTime < 0.3f) // Double-click detection
            {
                TryFocusOnClickedNode(mousePosition);
            }
            else
            {
                // Start drag
                StartDrag(mousePosition);
            }
            lastClickTime = Time.time;
        }
        
        if (isDragging)
        {
            if (leftButtonHeld)
            {
                ContinueDrag(mousePosition);
            }
            else
            {
                EndDrag();
            }
        }
    }
    
    bool IsChildOfSkillTree(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current == skillTreeContainer)
                return true;
            current = current.parent;
        }
        return false;
    }
    
    void StartDrag(Vector2 mousePosition)
    {
        isDragging = true;
        lastMousePosition = mousePosition;
        dragStartPosition = lastMousePosition;
        containerStartPosition = skillTreeContainer.anchoredPosition;
    }
    
    void ContinueDrag(Vector2 currentMousePosition)
    {
        Vector2 delta = currentMousePosition - lastMousePosition;
        
        targetPan += delta * panSpeed;
        targetPan = ClampPan(targetPan);
        
        lastMousePosition = currentMousePosition;
    }
    
    void EndDrag()
    {
        isDragging = false;
    }
    
    void HandleTouchInput()
    {
        if (!enableTouchGestures) return;
        
        var touches = Touch.activeTouches;
        
        if (touches.Count == 1)
        {
            var touch = touches[0];
            
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                lastMousePosition = touch.screenPosition;
                isDragging = true;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.screenPosition - lastMousePosition;
                targetPan += delta * panSpeed;
                targetPan = ClampPan(targetPan);
                lastMousePosition = touch.screenPosition;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else if (touches.Count == 2)
        {
            // Pinch zoom
            var touch0 = touches[0];
            var touch1 = touches[1];
            
            float currentPinchDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
            
            if (!isPinching)
            {
                isPinching = true;
                lastPinchDistance = currentPinchDistance;
            }
            else
            {
                float pinchDelta = currentPinchDistance - lastPinchDistance;
                targetZoom += pinchDelta * zoomSpeed * 0.01f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                lastPinchDistance = currentPinchDistance;
            }
        }
        else
        {
            isPinching = false;
        }
    }
    
    void HandleKeyboardInput()
    {
        if (!enableKeyboardPan || keyboard == null) return;
        
        Vector2 keyboardDelta = Vector2.zero;
        
        // Arrow keys or WASD for panning
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
            keyboardDelta.x += 1;
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
            keyboardDelta.x -= 1;
        if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed)
            keyboardDelta.y -= 1;
        if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed)
            keyboardDelta.y += 1;
        
        // Gamepad support
        if (gamepad != null)
        {
            Vector2 leftStick = gamepad.leftStick.ReadValue();
            keyboardDelta.x -= leftStick.x;
            keyboardDelta.y += leftStick.y;
        }
        
        if (keyboardDelta != Vector2.zero)
        {
            keyboardDelta = keyboardDelta.normalized * keyboardPanSpeed * Time.deltaTime;
            targetPan += keyboardDelta;
            targetPan = ClampPan(targetPan);
        }
        
        // Plus/Minus or Q/E for zoom
        bool zoomIn = (keyboard != null && (keyboard.equalsKey.isPressed || keyboard.numpadPlusKey.isPressed || keyboard.eKey.isPressed))
                      || (gamepad != null && gamepad.rightTrigger.isPressed);
        bool zoomOut = (keyboard != null && (keyboard.minusKey.isPressed || keyboard.numpadMinusKey.isPressed || keyboard.qKey.isPressed))
                       || (gamepad != null && gamepad.leftTrigger.isPressed);
        
        if (zoomIn)
        {
            targetZoom += zoomSpeed * 2f * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        if (zoomOut)
        {
            targetZoom -= zoomSpeed * 2f * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        
        // R to reset view
        if (keyboard.rKey.wasPressedThisFrame)
        {
            ResetView();
        }
        
        // F to fit all nodes in view
        if (keyboard.fKey.wasPressedThisFrame)
        {
            FitAllNodesInView();
        }
    }
    
    void HandleScrollWheel()
    {
        if (mouse == null) return;
        
        Vector2 scrollDelta = mouse.scroll.ReadValue();
        float scroll = scrollDelta.y / 120f; // Normalize scroll value
        
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Zoom towards mouse position
            Vector2 mousePosition = mouse.position.ReadValue();
            Vector2 localPoint;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                skillTreeContainer.parent as RectTransform,
                mousePosition,
                null,
                out localPoint
            );
            
            float zoomDelta = scroll * zoomSpeed * 10f;
            float newZoom = Mathf.Clamp(targetZoom + zoomDelta, minZoom, maxZoom);
            
            if (newZoom != targetZoom)
            {
                // Adjust pan to zoom towards mouse position
                Vector2 zoomPoint = localPoint - skillTreeContainer.anchoredPosition;
                float zoomRatio = newZoom / targetZoom;
                Vector2 newPan = localPoint - zoomPoint * zoomRatio;
                
                targetZoom = newZoom;
                targetPan = newPan;
                targetPan = ClampPan(targetPan);
            }
        }
    }
    
    void ApplyPanAndZoom()
    {
        // Apply smooth or instant pan
        if (useSmoothPan)
        {
            currentPan = Vector2.Lerp(currentPan, targetPan, smoothPanSpeed * Time.deltaTime);
        }
        else
        {
            currentPan = targetPan;
        }
        
        // Apply smooth or instant zoom
        if (useSmoothZoom)
        {
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, smoothZoomSpeed * Time.deltaTime);
        }
        else
        {
            currentZoom = targetZoom;
        }
        
        // Apply to transform
        skillTreeContainer.anchoredPosition = currentPan;
        skillTreeContainer.localScale = new Vector3(currentZoom, currentZoom, 1f);
    }
    
    Vector2 ClampPan(Vector2 pan)
    {
        // Calculate bounds based on zoom
        float zoomedLimitX = panLimits.x * currentZoom;
        float zoomedLimitY = panLimits.y * currentZoom;
        
        pan.x = Mathf.Clamp(pan.x, -zoomedLimitX, zoomedLimitX);
        pan.y = Mathf.Clamp(pan.y, -zoomedLimitY, zoomedLimitY);
        
        return pan;
    }
    
    void CalculateContainerBounds()
    {
        // Calculate the bounds of all nodes in the skill tree
        RectTransform[] children = skillTreeContainer.GetComponentsInChildren<RectTransform>();
        
        if (children.Length > 1) // More than just the container itself
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            
            foreach (RectTransform child in children)
            {
                if (child == skillTreeContainer) continue;
                
                Vector2 position = child.anchoredPosition;
                Vector2 size = child.sizeDelta * 0.5f;
                
                min.x = Mathf.Min(min.x, position.x - size.x);
                min.y = Mathf.Min(min.y, position.y - size.y);
                max.x = Mathf.Max(max.x, position.x + size.x);
                max.y = Mathf.Max(max.y, position.y + size.y);
            }
            
            containerBounds = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }
    
    public void ResetView()
    {
        targetPan = Vector2.zero;
        targetZoom = 1f;
    }
    
    public void FitAllNodesInView()
    {
        CalculateContainerBounds();
        
        if (containerBounds.width > 0 && containerBounds.height > 0)
        {
            RectTransform parentRect = skillTreeContainer.parent as RectTransform;
            if (parentRect != null)
            {
                float screenWidth = parentRect.rect.width;
                float screenHeight = parentRect.rect.height;
                
                float scaleX = screenWidth / containerBounds.width;
                float scaleY = screenHeight / containerBounds.height;
                
                targetZoom = Mathf.Min(scaleX, scaleY) * 0.9f; // 90% to add padding
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                
                // Center the view
                targetPan = -containerBounds.center * targetZoom;
            }
        }
    }
    
    public void FocusOnNode(GameObject nodeObject, float zoomLevel = 1.2f)
    {
        if (nodeObject == null) return;
        
        RectTransform nodeRect = nodeObject.GetComponent<RectTransform>();
        if (nodeRect == null) return;
        
        // Start focus animation
        isFocusing = true;
        focusTimer = 0f;
        
        focusStartPosition = currentPan;
        focusEndPosition = -nodeRect.anchoredPosition * zoomLevel;
        
        focusStartZoom = currentZoom;
        focusEndZoom = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
    }
    
    void HandleFocusAnimation()
    {
        if (!isFocusing) return;
        
        focusTimer += Time.deltaTime;
        float t = focusTimer / focusAnimationDuration;
        
        if (t >= 1f)
        {
            t = 1f;
            isFocusing = false;
        }
        
        float curveValue = focusCurve.Evaluate(t);
        
        currentPan = Vector2.Lerp(focusStartPosition, focusEndPosition, curveValue);
        currentZoom = Mathf.Lerp(focusStartZoom, focusEndZoom, curveValue);
        
        targetPan = currentPan;
        targetZoom = currentZoom;
    }
    
    void TryFocusOnClickedNode(Vector2 mousePosition)
    {
        // Check if EventSystem exists
        if (EventSystem.current == null)
        {
            Debug.LogWarning("No EventSystem found in scene. Double-click focus requires an EventSystem.");
            return;
        }
        
        // Raycast to find what was clicked
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = mousePosition;
        
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        
        foreach (var result in raycastResults)
        {
            // Check if this is a skill tree node
            if (result.gameObject.name.Contains("Node") || 
                result.gameObject.name.Contains("Exercise") ||
                result.gameObject.GetComponent<SharpUI.Source.Common.UI.Elements.BaseElement>() != null)
            {
                FocusOnNode(result.gameObject, 1.5f);
                break;
            }
        }
    }
    
    // Public methods for external control
    public void PanTo(Vector2 position)
    {
        targetPan = position;
    }
    
    public void ZoomTo(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
    
    public void SetPanLimits(Vector2 limits)
    {
        panLimits = limits;
    }
    
    public Vector2 GetCurrentPan()
    {
        return currentPan;
    }
    
    public float GetCurrentZoom()
    {
        return currentZoom;
    }
}