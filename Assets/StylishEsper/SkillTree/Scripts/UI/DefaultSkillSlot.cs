//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Default skill slot for the skill bar.
//***************************************************************************************

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    public class DefaultSkillSlot : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private Image iconImage;

        [SerializeField] 
        private Image cooldownImage;

        [SerializeField]
        private TextMeshProUGUI timeTextMesh;

        [SerializeField]
        private TextMeshProUGUI keyTextMesh;

        /// <summary>
        /// The skill reference.
        /// </summary>
        [HideInInspector]
        public SkillNode skill;

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

        /// <summary>
        /// RectTransform component of this skill slot.
        /// </summary>
        public RectTransform rectTransform { get => button.image.rectTransform; }

        private void Awake()
        {
            button.onClick.AddListener(InvokeOnClick);
            button.onClick.AddListener(UseSkill);
            onHover.AddListener(ShowDetails);
            onExit.AddListener(HideDetails);
            cooldownImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Displays the details.
        /// </summary>
        public void ShowDetails()
        {
            if (skill == null || !skill.IsValid())
            {
                return;
            }

            DefaultSkillDetails.instance?.ShowDetails(this);
        }

        /// <summary>
        /// Hides the details.
        /// </summary>
        public void HideDetails()
        {
            if (skill == null || !skill.IsValid())
            {
                return;
            }

            DefaultSkillDetails.instance?.Close();
        }

        /// <summary>
        /// Uses the skill. The skill will be put on cooldown.
        /// 
        /// To make something happen when a skill is used, use the SkillTree.onSkillUsed callback.
        /// </summary>
        public void UseSkill()
        {
            if (!DefaultSkillBar.instance.canUseSkill || DefaultSkillBar.instance.isLocked)
            {
                return;
            }

            var usedSkill = SkillTree.TryUseSkill(skill, SkillTree.playerStats);

            if (usedSkill == null)
            {
                return;
            }

            usedSkill.OnCooldownUpdated(SkillCooldownHandler);

            if (skill.cooldown > 0)
            {
                cooldownImage.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Handles skill cooldown visually.
        /// </summary>
        /// <param name="remainingTime">The amount of time remaining.</param>
        private void SkillCooldownHandler(float remainingTime)
        {
            if (remainingTime <= 0 || skill == null)
            {
                cooldownImage.gameObject.SetActive(false);
                return;
            }

            timeTextMesh.text = $"{remainingTime:0.00}s";
            float normalizedTime = remainingTime / skill.cooldown;
            SetCooldownFillAmount(normalizedTime);
        }

        /// <summary>
        /// Sets the cooldown image fill amount.
        /// </summary>
        /// <param name="amount">The amount out of 1.</param>
        private void SetCooldownFillAmount(float amount)
        {
            if (skill != null)
            {
                cooldownImage.fillAmount = amount;
            }
            else
            {
                cooldownImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the slot.
        /// </summary>
        /// <param name="skill">The skill reference.</param>
        /// <param name="keyText">The key text displayed on the slot.</param>
        public void SetSlot(SkillNode skill, string keyText)
        {
            this.skill = skill;
            ReloadSprite();
            SetKeyText(keyText);
        }

        /// <summary>
        /// Reloads the sprite.
        /// </summary>
        public void ReloadSprite()
        {
            iconImage.sprite = skill?.GetSprite();
            iconImage.gameObject.SetActive(iconImage.sprite);
        }

        /// <summary>
        /// Sets the key text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetKeyText(string text)
        {
            keyTextMesh.text = text;
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