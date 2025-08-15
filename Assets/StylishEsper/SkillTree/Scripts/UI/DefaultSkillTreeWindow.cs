//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    /// <summary>
    /// A default in-game window that displays all created and enabled Skill Trees.
    /// </summary>
    public class DefaultSkillTreeWindow : SkillTreeUI
    {
        [Header("UI Components")]
        [SerializeField]
        protected DefaultSkillNodeSlot skillSlotPrefab;

        [SerializeField]
        protected DefaultSkillConnectionUI connectionPrefab;

        [SerializeField]
        protected GridLayoutGroup grid;

        [SerializeField]
        protected Transform connectionContainer;

        [SerializeField]
        protected Button leftArrowButton;

        [SerializeField] 
        protected Button rightArrowButton;

        [SerializeField] 
        protected Button applyButton;

        [SerializeField] 
        protected TextMeshProUGUI graphTitleTextMesh;

        [SerializeField]
        protected TextMeshProUGUI levelTextMesh;

        [SerializeField]
        protected TextMeshProUGUI skillTextMesh;

        [Header("Sounds")]
        [SerializeField]
        protected AudioSource audioSource;

        [SerializeField]
        protected AudioClip buttonClickSound;

        [SerializeField]
        protected AudioClip openSound;

        [SerializeField]
        protected AudioClip closeSound;

        [SerializeField]
        protected AudioClip upgradeSkillSound;

        [SerializeField]
        protected AudioClip downgradeSkillSound;

        /// <summary>
        /// The prompt title.
        /// </summary>
        [Header("Confirm Prompt")]
        public string confirmPromptTitle;

        /// <summary>
        /// The prompt description.
        /// </summary>
        public string confirmPromptDescription;

        /// <summary>
        /// The confirm button text.
        /// </summary>
        public string confirmButtonText;

        /// <summary>
        /// The cancel button text.
        /// </summary>
        public string cancelButtonText;

        /// <summary>
        /// A list of currently loaded skill node slots.
        /// </summary>
        [HideInInspector]
        public List<DefaultSkillNodeSlot> loadedSkillSlots = new();

        /// <summary>
        /// A list of currently loaded connection objects.
        /// </summary>
        [HideInInspector]
        public List<DefaultSkillConnectionUI> loadedConnections = new();

        protected RectTransform gridRectTransform;

        protected List<SkillGraph> playerSkillTrees = new();

        protected SavableSkillGraph tempGraph;

        protected int skillPointsBeforeRevert;

        /// <summary>
        /// If any tree changes have been made.
        /// </summary>
        [HideInInspector]
        public bool changesMade;

        /// <summary>
        /// The skill tree window instance.
        /// </summary>
        public static DefaultSkillTreeWindow instance { get; protected set; }

        /// <summary>
        /// The current graph index.
        /// </summary>
        protected int currentGraphIndex;

        private void Awake()
        {
            instance = this;
            gridRectTransform = grid.GetComponent<RectTransform>();
            leftArrowButton.onClick.AddListener(PreviousGraph);
            rightArrowButton.onClick.AddListener(NextGraph);

            // Play sound events
            onOpen.AddListener(() => PlaySound(openSound));
            onClose.AddListener(() => PlaySound(closeSound));
            leftArrowButton.onClick.AddListener(() => PlaySound(buttonClickSound));
            rightArrowButton.onClick.AddListener(() => PlaySound(buttonClickSound));
            SkillTree.onSkillUpgraded.AddListener((x) => PlaySound(upgradeSkillSound));
            SkillTree.onSkillDowngraded.AddListener((x) => PlaySound(downgradeSkillSound));

            // Detect changes events
            SkillTree.onSkillDowngraded.AddListener((x) => SetChangesMade(true));
            SkillTree.onSkillUpgraded.AddListener((x) => SetChangesMade(true));

            applyButton.onClick.AddListener(ApplyChanges);
            SetChangesMade(false);

            onClose.AddListener(() => DefaultSkillDetails.instance?.Close());
            onClose.AddListener(() => DefaultSkillMenu.instance?.Close());
        }

        private void Start()
        {
            ReloadTreeList();
        }

        public override void Open()
        {
            if (ConfirmationPrompt.instance.isOpen)
            {
                return;
            }

            base.Open();
            ReloadGraph();

            // Remember skill points before any changes
            skillPointsBeforeRevert = SkillTree.playerSkillPoints;
        }

        public override void ToggleActive()
        {
            if (ConfirmationPrompt.instance.isOpen)
            {
                return;
            }

            base.ToggleActive();
        }

        public override void Close()
        {
            if (ConfirmationPrompt.instance.isOpen)
            {
                return;
            }

            // Open the confirmation prompt if necessary
            if (!RequiresConfirmation())
            {
                SetChangesMade(false);
                base.Close();
                DefaultSkillBar.instance?.Validate();
            }
            else
            {
                ConfirmationPrompt.instance.SetPrompt(confirmPromptTitle, confirmPromptDescription, () =>
                {
                    ApplyChanges();
                    base.Close();
                    DefaultSkillBar.instance?.Validate();
                }, () =>
                {
                    RevertChanges();
                    base.Close();
                    DefaultSkillBar.instance?.Validate();
                }, confirmButtonText, cancelButtonText);
            }
        }

        /// <summary>
        /// Sets the changes made value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetChangesMade(bool value)
        {
            changesMade = value;
            applyButton.gameObject.SetActive(RequiresConfirmation());
        }

        /// <summary>
        /// If the window requires confirmation before switching to another tree or exiting.
        /// </summary>
        /// <returns>True if it requires confirmation. Otherwise, false.</returns>
        public bool RequiresConfirmation()
        {
            return SkillTree.Settings.changesRequireConfirmation && changesMade;
        }

        /// <summary>
        /// Applies the graph changes.
        /// </summary>
        public void ApplyChanges()
        {
            if (!changesMade)
            {
                return;
            }

            SetChangesMade(false);
            tempGraph = SavableSkillGraph.Create(playerSkillTrees[currentGraphIndex]);

            // Remember skill points before any changes
            skillPointsBeforeRevert = SkillTree.playerSkillPoints;
        }

        /// <summary>
        /// Reverts the graph changes.
        /// </summary>
        public void RevertChanges()
        {
            if (!changesMade)
            {
                return;
            }

            SkillTree.SetSkillGraph(tempGraph);
            SkillTree.playerSkillPoints = skillPointsBeforeRevert;
            SetChangesMade(false);
            tempGraph = null;

            ReloadTreeList();
        }

        /// <summary>
        /// Reloads the list of trees.
        /// </summary>
        public void ReloadTreeList()
        {
            playerSkillTrees = SkillTree.GetVisiblePlayerSkillTrees();
            currentGraphIndex = 0;

            if (isOpen)
            {
                ReloadGraph();
            }

            if (playerSkillTrees.Count <= 1)
            {
                leftArrowButton.gameObject.SetActive(false);
                rightArrowButton.gameObject.SetActive(false);
            }
            else
            {
                leftArrowButton.gameObject.SetActive(true);
                rightArrowButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Plays a sound.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        public void PlaySound(AudioClip clip)
        {
            if (clip && audioSource)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Displays the next graph.
        /// </summary>
        public void NextGraph()
        {
            if (!RequiresConfirmation())
            {
                SetChangesMade(false);
                IncrementGraph();
            }
            else
            {
                ConfirmationPrompt.instance.SetPrompt(confirmPromptTitle, confirmPromptDescription, () =>
                {
                    ApplyChanges();
                    IncrementGraph();
                }, () =>
                {
                    RevertChanges();
                    IncrementGraph();
                }, confirmButtonText, cancelButtonText);
            }
        }

        /// <summary>
        /// Increments the grpah index and reloads the graph.
        /// </summary>
        private void IncrementGraph()
        {
            currentGraphIndex++;

            if (currentGraphIndex >= playerSkillTrees.Count)
            {
                currentGraphIndex = 0;
            }

            ReloadGraph();
        }

        /// <summary>
        /// Displays the previous graph.
        /// </summary>
        public void PreviousGraph()
        {
            if (!RequiresConfirmation())
            {
                SetChangesMade(false);
                DecrementGraph();
            }
            else
            {
                ConfirmationPrompt.instance.SetPrompt(confirmPromptTitle, confirmPromptDescription, () =>              
                {
                    ApplyChanges();
                    DecrementGraph();
                }, () =>
                {
                    RevertChanges();
                    DecrementGraph();
                }, confirmButtonText, cancelButtonText);
            }
        }

        /// <summary>
        /// Decrements the grpah index and reloads the graph.
        /// </summary>
        private void DecrementGraph()
        {
            currentGraphIndex--;

            if (currentGraphIndex < 0)
            {
                currentGraphIndex = playerSkillTrees.Count - 1;
            }

            ReloadGraph();
        }

        /// <summary>
        /// Refreshes the title, level, and skill points texts.
        /// </summary>
        public void RefreshTexts()
        {
            levelTextMesh.text = $"Level <size=32>{SkillTree.playerLevel}</size>";
            skillTextMesh.text = $"<size=32>{SkillTree.playerSkillPoints}</size> Skill Points";

            if (playerSkillTrees.Count == 0)
            {
                return;
            }

            graphTitleTextMesh.text = playerSkillTrees[currentGraphIndex].displayName;
        }

        /// <summary>
        /// Reloads the graph view.
        /// </summary>
        public void ReloadGraph()
        {
            // Clear previous skills and connections
            foreach (Transform item in grid.transform)
            {
                Destroy(item.gameObject);
            }
            loadedSkillSlots.Clear();

            foreach (var item in loadedConnections)
            {
                Destroy(item.gameObject);
            }
            loadedConnections.Clear();

            RefreshTexts();

            if (playerSkillTrees.Count == 0)
            {
                return;
            }

            // Use a temp graph to help revert changes
            var graph = playerSkillTrees[currentGraphIndex];

            if (tempGraph == null || tempGraph.id != graph.id)
            {
                tempGraph = SavableSkillGraph.Create(playerSkillTrees[currentGraphIndex]);
            }

            // Get grid rect size
            var rectSize = gridRectTransform.rect.size;
            rectSize.x -= grid.padding.left + grid.padding.right;
            rectSize.y -= grid.padding.top + grid.padding.bottom;

            // Set cell size
            grid.cellSize = new Vector2(graph.nodeSize, graph.nodeSize);

            // Calculate spacing
            float spacingX = (rectSize.x - (graph.nodeSize * graph.gridDimensions.x)) / (graph.gridDimensions.x - 1);
            float spacingY = (rectSize.y - (graph.nodeSize * graph.gridDimensions.y)) / (graph.gridDimensions.y - 1);
            var spacing = new Vector2(spacingX, spacingY);
            grid.spacing = spacing;

            // Display nodes
            for (int i = 0; i < graph.NodeCount; i++)
            {
                var node = graph.GetNodeByPosition(i);
                if (node)
                {
                    // Instantiate the skill slot
                    var slot = Instantiate(skillSlotPrefab, grid.transform);
                    slot.SetSkill(node, graph);
                    loadedSkillSlots.Add(slot);
                }
                else
                {
                    // Create an empty space for grid formatting
                    var empty = new GameObject("EmptySpace", typeof(RectTransform));
                    empty.transform.SetParent(grid.transform);
                    empty.transform.localScale = Vector3.one;
                }
            }

            // Display connections
            StartCoroutine(LateDisplayConnectionsCoroutine());
        }

        /// <summary>
        /// Waits for the end of the frame before loading connections. This is so that
        /// the necessary UI objects are loaded and positioned correctly before any
        /// connections are positioned.
        /// </summary>
        /// <returns>Yields until the end of the frame.</returns>
        private IEnumerator LateDisplayConnectionsCoroutine()
        {
            yield return new WaitForEndOfFrame();

            foreach (var connection in playerSkillTrees[currentGraphIndex].connections)
            {
                var connectionUI = Instantiate(connectionPrefab, connectionContainer);
                connectionUI.SetConnection(connection);
                loadedConnections.Add(connectionUI);
            }
        }

        /// <summary>
        /// Gets a skill node slot by index.
        /// </summary>
        /// <param name="index">The index of the skill in the graph.</param>
        /// <returns></returns>
        public DefaultSkillNodeSlot GetSkillSlot(int index)
        {
            foreach (var slot in loadedSkillSlots)
            {
                if (slot.skill.positionIndex == index)
                {
                    return slot;
                }
            }

            return null;
        }

        /// <summary>
        /// Removes all upgrades on the active skill tree.
        /// </summary>
        public void ResetCurrentTree()
        {
            foreach (var slot in loadedSkillSlots)
            {
                slot.DepleteSkill(true, true);
            }

            tempGraph = null;
            skillPointsBeforeRevert = SkillTree.playerSkillPoints;
            ReloadGraph();
            SetChangesMade(false);
        }
    }
}