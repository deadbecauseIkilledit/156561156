//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using TMPro;
using UnityEngine;

namespace Esper.SkillTree.UI
{
    /// <summary>
    /// Skill Tree's default skill details HUD.
    /// </summary>
    public class DefaultSkillDetails : SkillTreeUI
    {
        [SerializeField]
        protected TextMeshProUGUI nameTextMesh;

        [SerializeField]
        protected TextMeshProUGUI typeTextMesh;

        [SerializeField]
        protected TextMeshProUGUI descriptionTextMesh;

        [SerializeField]
        protected TextMeshProUGUI cooldownTextMesh;

        [SerializeField]
        protected TextMeshProUGUI costTextMesh;

        [SerializeField]
        protected Animator animator;

        /// <summary>
        /// The color of the requirements text.
        /// </summary>
        public Color requirementsInfoColor = Color.red;

        /// <summary>
        /// The color of the upgrade text.
        /// </summary>

        public Color upgradeInfoColor = new Color(1f, 0.5f, 0f);

        /// <summary>
        /// Skill details UI instance.
        /// </summary>
        public static DefaultSkillDetails instance { get; private set; }

        protected virtual void Awake()
        {
            instance = this;
        }

        public override void Open()
        {
            if (DefaultSkillMenu.instance.isOpen)
            {
                return;
            }

            content.gameObject.SetActive(true);
            onOpen.Invoke();
            onActiveStateChanged.Invoke();

            if (animator)
            {
                animator.Play("Open");
            }
        }

        public override void Close()
        {
            if (!isOpen)
            {
                return;
            }

            onClose.Invoke();
            onActiveStateChanged.Invoke();

            if (animator)
            {
                animator.Play("Close");
                Invoke(nameof(SetInactive), 0.2f);
            }
        }

        /// <summary>
        /// Sets the content inactive.
        /// </summary>
        protected void SetInactive()
        {
            content.gameObject.SetActive(false);
        }

        /// <summary>
        /// Forces the menu content inside the game viewport.
        /// </summary>
        public void ForceInsideView()
        {
            if (!content.IsFullyVisible())
            {
                content.ForceInsideView();
            }
        }

        /// <summary>
        /// Opens this UI object and populates the texts with skill details.
        /// </summary>
        /// <param name="skillSlot">The slot of the skill to display.</param>
        public virtual void ShowDetails(DefaultSkillSlot skillSlot)
        {
            CancelInvoke();
            Open();

            Vector2 position = skillSlot.rectTransform.position;
            content.position = position + (content.sizeDelta / 2);

            nameTextMesh.text = skillSlot.skill.displayName;
            typeTextMesh.text = skillSlot.skill.GetSkillType();
            descriptionTextMesh.text = skillSlot.skill.GetInterpolatedDescription();
            cooldownTextMesh.text = $"{skillSlot.skill.cooldown:0.00}s";
            costTextMesh.text = $"{skillSlot.skill.cost:0.##}";
            
            ForceInsideView();
        }

        /// <summary>
        /// Displays skill details for a skill node slot. This will display upgrade details as well.
        /// </summary>
        /// <param name="skillSlot">The slot of the skill to display.</param>
        public virtual void ShowDetails(DefaultSkillNodeSlot skillSlot)
        {
            CancelInvoke();
            Open();

            Vector2 position = skillSlot.contentRectTransform.position;
            content.position = position + (content.sizeDelta / 2);

            nameTextMesh.text = skillSlot.skill.displayName;
            typeTextMesh.text = skillSlot.skill.GetSkillType();
            descriptionTextMesh.text = skillSlot.skill.GetInterpolatedDescription();
            cooldownTextMesh.text = $"{skillSlot.skill.cooldown:0.00}s";
            costTextMesh.text = $"{skillSlot.skill.cost:0.##}";

            if (skillSlot.skill.currentLevel > 0)
            {
                if (!skillSlot.skill.IsMaxed)
                {
                    descriptionTextMesh.text += $"\n\n<color=#{ColorUtility.ToHtmlStringRGBA(upgradeInfoColor)}><u>Next Level</u></color>";

                    foreach (var stat in skillSlot.skill.stats)
                    {
                        float nextValue = stat.NextBaseValue.ToFloat();
                        if (stat.BaseValue.ToFloat() != nextValue)
                        {
                            descriptionTextMesh.text += $"\n<color=#{ColorUtility.ToHtmlStringRGBA(upgradeInfoColor)}>{stat.Identity.abbreviation} {stat.BaseValue} >> {stat.NextBaseValue}</color>";
                        }
                    }
                }
                else
                {
                    descriptionTextMesh.text += $"\n\n<color=#{ColorUtility.ToHtmlStringRGBA(upgradeInfoColor)}>MAX</color>";
                }
            }
            else
            {
                if (skillSlot.skill.IsLocked)
                {
                    descriptionTextMesh.text += $"\n\n<color=#{ColorUtility.ToHtmlStringRGBA(requirementsInfoColor)}>LOCKED</color>";
                }
                else
                {
                    if (SkillTree.playerLevel < skillSlot.skill.playerLevelRequirement)
                    {
                        descriptionTextMesh.text += $"\n\n<color=#{ColorUtility.ToHtmlStringRGBA(requirementsInfoColor)}>Requires player level {skillSlot.skill.playerLevelRequirement}</color>";
                    }

                    var graph = skillSlot.skill.graphReference;
                    if (skillSlot.skill.treeTotalPointsSpentRequirement > 0 && skillSlot.skill.treeTotalPointsSpentRequirement > graph.CountPointsSpent())
                    {
                        descriptionTextMesh.text += $"\n\n<color=#{ColorUtility.ToHtmlStringRGBA(requirementsInfoColor)}>Requires {skillSlot.skill.treeTotalPointsSpentRequirement} points in {graph.displayName}</color>";
                    }
                }
            }

            ForceInsideView();
        }
    }
}