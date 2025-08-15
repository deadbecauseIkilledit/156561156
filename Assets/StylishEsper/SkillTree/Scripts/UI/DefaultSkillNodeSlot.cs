//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Default skill node slot for displaying skills in the SkillTreeWindow.
//***************************************************************************************

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    public class DefaultSkillNodeSlot : MonoBehaviour
    {
        public RectTransform contentRectTransform;

        [SerializeField]
        private Button button;

        [SerializeField]
        protected Image maskImage;

        [SerializeField]
        protected Image iconImage;

        [SerializeField]
        protected Sprite squareSprite;

        [SerializeField]
        protected Sprite squircleSprite;

        [SerializeField]
        protected Sprite circleSprite;

        [SerializeField]
        protected TextMeshProUGUI upgradesText;

        /// <summary>
        /// The skill node that this slot should display.
        /// </summary>
        public SkillNode skill { get; protected set; }

        /// <summary>
        /// Skill graph reference.
        /// </summary>
        public SkillGraph graph { get; protected set; }

        /// <summary>
        /// Called when the skill slot is clicked.
        /// </summary>
        public UnityEvent onClick = new();

        /// <summary>
        /// Called when the pointer is over the skill slot.
        /// </summary>
        public UnityEvent onHover = new();

        /// <summary>
        /// Called when the pointer has exited the skill slot.
        /// </summary>
        public UnityEvent onExit = new();

        private void Awake()
        {
            button.onClick.AddListener(InvokeOnClick);
            onHover.AddListener(ShowDetails);
            onExit.AddListener(HideDetails);
        }

        /// <summary>
        /// Sets and displays the skill.
        /// </summary>
        /// <param name="skill">The skill node.</param>
        /// <param name="graph">The skill graph reference. Default: null.</param>
        public void SetSkill(SkillNode skill, SkillGraph graph = null)
        {
            this.skill = skill;

            if (graph == null)
            {
                this.graph = skill.graphReference;
            }
            else
            {
                this.graph = graph;
            }

            Refresh();
        }

        /// <summary>
        /// Reloads this skill slot. This will refresh the sprites, size, and text.
        /// </summary>
        public void Refresh()
        {
            if (skill.isUnique)
            {
                contentRectTransform.sizeDelta = new Vector2(skill.uniqueSize, skill.uniqueSize);
                SetBorderSprite(skill.uniqueShape);
            }
            else
            {
                contentRectTransform.sizeDelta = new Vector2(graph.nodeSize, graph.nodeSize);
                SetBorderSprite(graph.nodeShape);
            }

            RefreshIcon();
            RefreshUpgradeText();
        }

        /// <summary>
        /// Refreshes the icon. The icon will be set to a sprite depending on the skill state.
        /// </summary>
        public void RefreshIcon()
        {
            switch (skill.state)
            {
                case SkillNode.State.Locked:
                    iconImage.sprite = skill.lockedSprite;
                    break;

                case SkillNode.State.Unlocked:
                    iconImage.sprite = skill.unlockedSprite;
                    break;

                case SkillNode.State.Obtained:
                    iconImage.sprite = skill.obtainedSprite;
                    break;

                case SkillNode.State.Maxed:
                    iconImage.sprite = skill.obtainedSprite;
                    break;
            }
        }

        /// <summary>
        /// Refreshes the upgrade text.
        /// </summary>
        public void RefreshUpgradeText()
        {
            upgradesText.text = $"{skill.currentLevel}/{skill.maxLevel}";
        }

        /// <summary>
        /// Sets the image border and mask sprites.
        /// </summary>
        /// <param name="shape">The skill shape.</param>
        public void SetBorderSprite(SkillNode.Shape shape)
        {
            switch (shape)
            {
                case SkillNode.Shape.Square:
                    button.image.sprite = squareSprite;
                    maskImage.sprite = squareSprite;
                    break;

                case SkillNode.Shape.Squircle:
                    button.image.sprite = squircleSprite;
                    maskImage.sprite = squircleSprite;
                    break;

                case SkillNode.Shape.Circle:
                    button.image.sprite = circleSprite;
                    maskImage.sprite = circleSprite;
                    break;
            }
        }

        /// <summary>
        /// Upgrades this skill if possible.
        /// </summary>
        public void UpgradeSkill()
        {
            if (skill.TryUpgrade())
            {
                DefaultSkillTreeWindow.instance.ReloadGraph();
            }
        }

        /// <summary>
        /// Downgrades with skill if possible.
        /// </summary>
        public void DowngradeSkill()
        {
            if (skill.TryDowngrade())
            {
                DefaultSkillTreeWindow.instance.ReloadGraph();
            }
        }

        /// <summary>
        /// Depletes the skill if possible.
        /// </summary>
        /// <param name="withoutReload">If the skill tree window should not be reloaded.</param>
        /// <param name="forceDeplete">If the 'Allow Downgrade' value in Skill Tree's settings should be ignored.</param>
        public void DepleteSkill(bool withoutReload = false, bool forceDeplete = false)
        {
            skill.Deplete(false, forceDeplete);

            if (!withoutReload)
            {
                DefaultSkillTreeWindow.instance.ReloadGraph();
            }
        }

        /// <summary>
        /// Displays skill details.
        /// </summary>
        public void ShowDetails()
        {
            DefaultSkillDetails.instance?.ShowDetails(this);
        }

        /// <summary>
        /// Hides skill details.
        /// </summary>
        public void HideDetails()
        {
            DefaultSkillDetails.instance?.Close();
        }

        /// <summary>
        /// Opens the skill menu.
        /// </summary>
        public void OpenMenu()
        {
            HideDetails();
            DefaultSkillMenu.instance?.DisplayMenuOptions(this);
        }

        /// <summary>
        /// Closes the skill menu.
        /// </summary>
        public void CloseMenu()
        {
            DefaultSkillMenu.instance?.Close();
        }

        /// <summary>
        /// Invokes on click events.
        /// </summary>
        public void InvokeOnClick()
        {
            onClick.Invoke();
        }

        /// <summary>
        /// Invokes on hover events.
        /// </summary>
        public void InvokeOnHover()
        {
            onHover.Invoke();
        }

        /// <summary>
        /// Invokes on exit events.
        /// </summary>
        public void InvokeOnExit()
        {
            onExit.Invoke();
        }
    }
}