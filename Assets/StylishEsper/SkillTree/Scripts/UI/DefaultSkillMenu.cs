//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Esper.SkillTree.UI
{
    /// <summary>
    /// Skill Tree's default skill menu.
    /// </summary>
    public class DefaultSkillMenu : SkillTreeUI
    {
        [SerializeField]
        protected DefaultSkillMenuItem itemPrefab;

        [SerializeField]
        protected Transform itemContainer;

        [SerializeField]
        protected AudioSource audioSource;

        [SerializeField]
        protected AudioClip buttonClickSound;

        protected List<DefaultSkillMenuItem> itemInstances = new();

        /// <summary>
        /// Skill details UI instance.
        /// </summary>
        public static DefaultSkillMenu instance { get; private set; }

        private void Awake()
        {
            instance = this;

            onOpen.AddListener(PlaySound);
            onClose.AddListener(PlaySound);
        }

        private void PlaySound()
        {
            if (buttonClickSound && audioSource)
            {
                audioSource.PlayOneShot(buttonClickSound);
            }
        }

        /// <summary>
        /// Displays menu options of a skill.
        /// </summary>
        /// <param name="slot">The skill slot in the DefaultSkillTreeWindow.</param>
        public virtual void DisplayMenuOptions(DefaultSkillNodeSlot slot)
        {
            if (!slot.skill.IsObtained)
            {
                return;
            }

            // Destroy existing options
            ClearMenuItems();

            // Create menu options
            for (int i = 0; i < DefaultSkillBar.instance.slotCount; i++)
            {
                int index = i + 1;

                if (!DefaultSkillBar.instance.IsAssigned(slot.skill, index - 1))
                {
                    AddMenuItem($"Asssign Slot #{index}", () => AssignToSkillBar(slot.skill, index - 1));
                }
                else
                {
                    AddMenuItem($"Remove From Slot #{index}", () => RemoveFromSkillBar(index - 1));
                }
            }

            Vector2 position = slot.contentRectTransform.position;
            content.position = position + (new Vector2(content.sizeDelta.x, -content.sizeDelta.y) / 2);

            Open();

            ForceInsideView();
        }

        /// <summary>
        /// Adds a menu item.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="onClick">The action to perform on-click.</param>
        public void AddMenuItem(string text, Action onClick)
        {
            var instance = Instantiate(itemPrefab, itemContainer);
            instance.SetItem(text, onClick);
            itemInstances.Add(instance);
        }

        /// <summary>
        /// Clears existing menu items.
        /// </summary>
        public void ClearMenuItems()
        {
            foreach (var item in itemInstances)
            {
                Destroy(item.gameObject);
            }
            itemInstances.Clear();
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
        /// Assigns a skill to a slot in the skill bar.
        /// </summary>
        /// <param name="skill">The skill to assign.</param>
        /// <param name="slotIndex">The slot index.</param>
        public void AssignToSkillBar(SkillNode skill, int slotIndex)
        {
            DefaultSkillBar.instance?.AssignSkillToSlot(skill, slotIndex);
            Close();
        }

        /// <summary>
        /// Removes a skill from a slot in the skill bar.
        /// </summary>
        /// <param name="slotIndex">The slot index.</param>
        public void RemoveFromSkillBar(int slotIndex)
        {
            DefaultSkillBar.instance?.UnassignSkillFromSlot(slotIndex);
            Close();
        }
    }
}