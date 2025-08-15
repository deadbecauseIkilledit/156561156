using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeAnimations : MonoBehaviour
{
    [Header("Node Animations")]
    [SerializeField] private float nodeAppearDelay = 0.05f;
    [SerializeField] private float nodeAppearDuration = 0.3f;
    [SerializeField] private AnimationCurve nodeAppearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Unlock Animation")]
    [SerializeField] private float unlockAnimationDuration = 0.5f;
    [SerializeField] private float unlockGlowIntensity = 1.5f;
    [SerializeField] private Color unlockGlowColor = new Color(0.3f, 0.8f, 1f, 1f);
    [SerializeField] private GameObject unlockParticlePrefab;
    
    [Header("Complete Animation")]
    [SerializeField] private float completeAnimationDuration = 0.6f;
    [SerializeField] private float completePulseScale = 1.3f;
    [SerializeField] private Color completeFlashColor = Color.green;
    [SerializeField] private GameObject completeParticlePrefab;
    
    [Header("Hover Effects")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverDuration = 0.2f;
    
    [Header("Line Animations")]
    [SerializeField] private float lineFlowSpeed = 2f;
    [SerializeField] private bool animateLineFlow = true;
    
    // Animation state tracking
    private Coroutine currentNodeAnimation;
    private List<Coroutine> activeAnimations = new List<Coroutine>();
    
    public void AnimateNodesAppear(GameObject[] nodes)
    {
        StopAllAnimations();
        currentNodeAnimation = StartCoroutine(NodesAppearSequence(nodes));
    }
    
    public void StopAllAnimations()
    {
        if (currentNodeAnimation != null)
        {
            StopCoroutine(currentNodeAnimation);
            currentNodeAnimation = null;
        }
        
        foreach (var animation in activeAnimations)
        {
            if (animation != null)
            {
                StopCoroutine(animation);
            }
        }
        activeAnimations.Clear();
        
        StopAllCoroutines();
    }
    
    IEnumerator NodesAppearSequence(GameObject[] nodes)
    {
        // Start all nodes invisible (scale only, not alpha)
        foreach (var node in nodes)
        {
            if (node != null)
            {
                node.transform.localScale = Vector3.zero;
                
                // Skip fading text for now - let's keep text visible
                // TextMeshProUGUI[] texts = node.GetComponentsInChildren<TextMeshProUGUI>();
                // foreach (var text in texts)
                // {
                //     Color c = text.color;
                //     c.a = 0;
                //     text.color = c;
                // }
            }
        }
        
        // Animate each node appearing
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                var animCoroutine = StartCoroutine(AnimateNodeAppear(nodes[i]));
                activeAnimations.Add(animCoroutine);
                yield return new WaitForSeconds(nodeAppearDelay);
            }
        }
    }
    
    IEnumerator AnimateNodeAppear(GameObject node)
    {
        // Check if node still exists
        if (node == null) yield break;
        
        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        
        // Skip text fading for now - keep text visible
        TextMeshProUGUI[] texts = node.GetComponentsInChildren<TextMeshProUGUI>();
        
        while (timer < nodeAppearDuration)
        {
            // Check if node was destroyed
            if (node == null) yield break;
            
            timer += Time.deltaTime;
            float t = timer / nodeAppearDuration;
            float curveValue = nodeAppearCurve.Evaluate(t);
            
            // Scale animation
            if (node != null && node.transform != null)
            {
                node.transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
            }
            
            // Skip text fading - keep text visible
            // for (int i = 0; i < texts.Length; i++)
            // {
            //     if (texts[i] != null)
            //     {
            //         Color c = originalTextColors[i];
            //         c.a = curveValue;
            //         texts[i].color = c;
            //     }
            // }
            
            yield return null;
        }
        
        // Ensure final values (only if node still exists)
        if (node != null && node.transform != null)
        {
            node.transform.localScale = endScale;
        }
        
        // Ensure text is visible
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                Color c = texts[i].color;
                c.a = 1f; // Full opacity
                texts[i].color = c;
            }
        }
    }
    
    public void AnimateNodeUnlock(GameObject node)
    {
        if (node == null) return;
        var animCoroutine = StartCoroutine(NodeUnlockAnimation(node));
        activeAnimations.Add(animCoroutine);
    }
    
    IEnumerator NodeUnlockAnimation(GameObject node)
    {
        // Check if node still exists
        if (node == null) yield break;
        
        // Create glow effect
        GameObject glowObj = CreateGlowEffect(node);
        
        // Spawn particles if prefab exists
        if (unlockParticlePrefab != null)
        {
            GameObject particles = Instantiate(unlockParticlePrefab, node.transform.position, Quaternion.identity);
            particles.transform.SetParent(node.transform);
            Destroy(particles, 2f);
        }
        
        // Pulse animation
        float timer = 0f;
        Vector3 originalScale = node.transform.localScale;
        Image nodeImage = node.GetComponent<Image>();
        Color originalColor = nodeImage != null ? nodeImage.color : Color.white;
        
        while (timer < unlockAnimationDuration)
        {
            // Check if node was destroyed
            if (node == null) yield break;
            
            timer += Time.deltaTime;
            float t = timer / unlockAnimationDuration;
            
            // Pulse scale
            float scalePulse = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            if (node != null && node.transform != null)
            {
                node.transform.localScale = originalScale * scalePulse;
            }
            
            // Flash color
            if (nodeImage != null)
            {
                nodeImage.color = Color.Lerp(originalColor, unlockGlowColor, Mathf.Sin(t * Mathf.PI));
            }
            
            // Animate glow
            if (glowObj != null)
            {
                Image glowImage = glowObj.GetComponent<Image>();
                if (glowImage != null)
                {
                    Color glowColor = unlockGlowColor;
                    glowColor.a = (1f - t) * 0.5f;
                    glowImage.color = glowColor;
                    
                    float glowScale = 1f + t * 0.5f;
                    glowObj.transform.localScale = Vector3.one * glowScale;
                }
            }
            
            yield return null;
        }
        
        // Cleanup (only if node still exists)
        if (node != null && node.transform != null)
        {
            node.transform.localScale = originalScale;
        }
        if (nodeImage != null)
        {
            nodeImage.color = originalColor;
        }
        if (glowObj != null)
        {
            Destroy(glowObj);
        }
    }
    
    public void AnimateNodeComplete(GameObject node)
    {
        if (node == null) return;
        var animCoroutine = StartCoroutine(NodeCompleteAnimation(node));
        activeAnimations.Add(animCoroutine);
    }
    
    IEnumerator NodeCompleteAnimation(GameObject node)
    {
        // Check if node still exists
        if (node == null) yield break;
        
        // Spawn celebration particles
        if (completeParticlePrefab != null)
        {
            GameObject particles = Instantiate(completeParticlePrefab, node.transform.position, Quaternion.identity);
            particles.transform.SetParent(node.transform.parent);
            Destroy(particles, 3f);
        }
        
        // Create checkmark overlay
        GameObject checkmark = CreateCheckmarkOverlay(node);
        
        // Animation
        float timer = 0f;
        Vector3 originalScale = node.transform.localScale;
        
        while (timer < completeAnimationDuration)
        {
            // Check if node was destroyed
            if (node == null) yield break;
            
            timer += Time.deltaTime;
            float t = timer / completeAnimationDuration;
            
            if (node != null && node.transform != null)
            {
                // Bounce scale
                float bounce = 1f + Mathf.Sin(t * Mathf.PI * 2) * 0.3f * (1f - t);
                node.transform.localScale = originalScale * bounce;
                
                // Rotate slightly
                float rotation = Mathf.Sin(t * Mathf.PI * 4) * 5f * (1f - t);
                node.transform.rotation = Quaternion.Euler(0, 0, rotation);
            }
            
            // Animate checkmark
            if (checkmark != null)
            {
                float checkScale = Mathf.Min(t * 2f, 1f);
                checkmark.transform.localScale = Vector3.one * checkScale;
            }
            
            yield return null;
        }
        
        // Reset (only if node still exists)
        if (node != null && node.transform != null)
        {
            node.transform.localScale = originalScale;
            node.transform.rotation = Quaternion.identity;
        }
    }
    
    GameObject CreateGlowEffect(GameObject node)
    {
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(node.transform);
        glow.transform.SetAsFirstSibling();
        
        RectTransform glowRect = glow.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.sizeDelta = Vector2.one * 20; // Slightly larger
        glowRect.anchoredPosition = Vector2.zero;
        
        Image glowImage = glow.AddComponent<Image>();
        glowImage.color = new Color(1, 1, 1, 0);
        
        // Try to use a circular sprite if available
        Sprite circleSprite = Resources.Load<Sprite>("Sprites/Circle");
        if (circleSprite != null)
        {
            glowImage.sprite = circleSprite;
        }
        
        return glow;
    }
    
    GameObject CreateCheckmarkOverlay(GameObject node)
    {
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(node.transform);
        
        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.sizeDelta = new Vector2(40, 40);
        checkRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI checkText = checkmark.AddComponent<TextMeshProUGUI>();
        checkText.text = "✓";
        checkText.fontSize = 32;
        checkText.fontStyle = FontStyles.Bold;
        checkText.color = Color.white;
        checkText.alignment = TextAlignmentOptions.Center;
        
        // Add shadow
        Shadow shadow = checkmark.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);
        
        checkmark.transform.localScale = Vector3.zero;
        
        return checkmark;
    }
    
    public void SetupHoverEffects(GameObject node)
    {
        NodeHoverEffect hoverEffect = node.GetComponent<NodeHoverEffect>();
        if (hoverEffect == null)
        {
            hoverEffect = node.AddComponent<NodeHoverEffect>();
        }
        
        hoverEffect.hoverScale = hoverScale;
        hoverEffect.hoverDuration = hoverDuration;
    }
    
    public void AnimateConnectionLine(GameObject line, Color color)
    {
        if (!animateLineFlow) return;
        
        LineFlowAnimation flowAnim = line.GetComponent<LineFlowAnimation>();
        if (flowAnim == null)
        {
            flowAnim = line.AddComponent<LineFlowAnimation>();
        }
        
        flowAnim.flowSpeed = lineFlowSpeed;
        flowAnim.lineColor = color;
    }
}

// Helper component for hover effects
public class NodeHoverEffect : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;
    
    private Vector3 originalScale = Vector3.one;
    private Coroutine hoverCoroutine;
    private bool scaleInitialized = false;
    private bool isHovering = false;
    
    void Start()
    {
        // Delay getting the original scale to ensure animations have completed
        StartCoroutine(InitializeScale());
    }
    
    IEnumerator InitializeScale()
    {
        // Wait longer to ensure animations complete (nodeAppearDuration is 0.3f by default)
        yield return new WaitForSeconds(1.0f);
        
        // Only capture scale if we're not currently hovering
        if (!isHovering)
        {
            originalScale = transform.localScale;
            // Ensure we have a valid scale
            if (originalScale == Vector3.zero || originalScale.magnitude < 0.1f)
            {
                originalScale = Vector3.one;
            }
        }
        scaleInitialized = true;
    }
    
    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isHovering = true;
        
        // If scale hasn't been initialized yet, capture it now
        if (!scaleInitialized || originalScale.magnitude < 0.1f)
        {
            originalScale = transform.localScale;
            if (originalScale == Vector3.zero || originalScale.magnitude < 0.1f)
            {
                originalScale = Vector3.one;
            }
            scaleInitialized = true;
        }
        
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        
        // Always scale up from current scale to avoid shrinking
        Vector3 targetScale = originalScale * hoverScale;
        // Ensure we're scaling up, not down
        if (targetScale.magnitude > transform.localScale.magnitude)
        {
            hoverCoroutine = StartCoroutine(ScaleTo(targetScale));
        }
    }
    
    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isHovering = false;
        
        if (!scaleInitialized) 
        {
            originalScale = Vector3.one;
            scaleInitialized = true;
        }
        
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(ScaleTo(originalScale));
    }
    
    IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float timer = 0f;
        
        while (timer < hoverDuration)
        {
            timer += Time.deltaTime;
            float t = timer / hoverDuration;
            t = Mathf.SmoothStep(0, 1, t); // Smooth transition
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
}

// Helper component for line flow animation
public class LineFlowAnimation : MonoBehaviour
{
    public float flowSpeed = 2f;
    public Color lineColor = Color.white;
    
    private Material lineMaterial;
    private float offset = 0f;
    
    void Start()
    {
        Image lineImage = GetComponent<Image>();
        if (lineImage != null)
        {
            // Create a material instance for animation
            lineMaterial = new Material(lineImage.material);
            lineImage.material = lineMaterial;
        }
    }
    
    void Update()
    {
        if (lineMaterial != null)
        {
            offset += Time.deltaTime * flowSpeed;
            if (offset > 1f) offset -= 1f;
            
            // If the material supports texture offset
            if (lineMaterial.HasProperty("_MainTex_ST"))
            {
                lineMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
            }
        }
    }
    
    void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
        }
    }
}