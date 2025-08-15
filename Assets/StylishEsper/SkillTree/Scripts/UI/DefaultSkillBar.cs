//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Default skill bar UI.
//***************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    public class DefaultSkillBar : SkillTreeUI
    {
        /// <summary>
        /// Skill slot prefab object that will be instanced.
        /// </summary>
        public DefaultSkillSlot skillSlotPrefab;

        /// <summary>
        /// Placement of the skill bar.
        /// </summary>
        public Placement placement;

        /// <summary>
        /// The number of slots that should be instantiated.
        /// </summary>
        public int slotCount = 14;

        /// <summary>
        /// Spacing between skill slots.
        /// </summary>
        public float slotSpacing = 10;

        /// <summary>
        /// The size of the bar.
        /// </summary>
        public float barSize = 128;

        /// <summary>
        /// Padding of the skill bar.
        /// </summary>
        public RectPadding barPadding;

        /// <summary>
        /// Padding of skill bar's content.
        /// </summary>
        public RectOffset contentPadding;

        /// <summary>
        /// Alignment of the skill bar content.
        /// </summary>
        public TextAnchor slotAlignment;

        [SerializeField]
        private bool testInEditMode;

        /// <summary>
        /// The skill slot instances.
        /// </summary>
        [HideInInspector]
        public List<DefaultSkillSlot> slots = new();

        /// <summary>
        /// If the player is able to use a skill.
        /// </summary>
        public bool canUseSkill { get; set; } = true;

        /// <summary>
        /// If the skill bar is currently locked. Locking prevents the player from using the skill bar.
        /// </summary>
        public bool isLocked { get; set; }

        private Placement prevPlacement;

        /// <summary>
        /// The active DefaultSkillBar instance. This may be null if no DefaultSkillBar is in the scene.
        /// </summary>
        public static DefaultSkillBar instance { get; private set; }

        private void Awake()
        {
            instance = this;

            // Callback to auto unassign skill if the skill is depleted from the skill tree window
            SkillTree.onSkillDepleted.AddListener(UnassignSkillFromSlot);

            // Callback to tell the UI when a skill can and can't be used
            SkillTree.onSkillUsed.AddListener((x) =>
            {
                if (x.skill.IsPlayerSkill)
                {
                    isLocked = true;
                }
            });

            SkillTree.onSkillUseCompleted.AddListener((x) =>
            {
                if (x.skill.IsPlayerSkill)
                {
                    isLocked = false;
                }
            });
        }

        private void Start()
        {
            UpdatePlacement();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying || !testInEditMode)
            {
                return;
            }

            Invoke(nameof(UpdatePlacement), 0.1f);
        }
#endif

        /// <summary>
        /// Validates the skills in each slot to ensure the player has obtained them.
        /// </summary>
        public void Validate()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].skill != null && !slots[i].skill.IsObtained)
                {
                    UnassignSkillFromSlot(i);
                }
            }
        }

        /// <summary>
        /// Checks if a skill is assigned to a specific slot.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="slotIndex">The index of the slot.</param>
        /// <returns>True if the skill is assigned to the slot. Otherwise, false.</returns>
        public bool IsAssigned(SkillNode skill, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCount)
            {
                Debug.LogError("Skill Bar: Slot index out of range.");
                return false;
            }

            return slots[slotIndex].skill == skill;
        }

        /// <summary>
        /// Assigns a skill to a slot.
        /// </summary>
        /// <param name="skill">The skill to assign.</param>
        /// <param name="slotIndex">The index of the slot.</param>
        public void AssignSkillToSlot(SkillNode skill, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCount)
            {
                Debug.LogError("Skill Bar: Slot index out of range.");
                return;
            }

            slots[slotIndex].skill = skill;
            slots[slotIndex].ReloadSprite();
        }

        /// <summary>
        /// Unassigns a skill if it's assigned to a slot.
        /// </summary>
        /// <param name="skill">The skill to unassign.</param>
        public void UnassignSkillFromSlot(SkillNode skill)
        {
            foreach (var slot in slots)
            {
                if (slot.skill == skill)
                {
                    slot.skill = null;
                    slot.ReloadSprite();
                    break;
                }
            }
        }

        /// <summary>
        /// Unassigns the skill at a slot index.
        /// </summary>
        /// <param name="slotIndex">The index of the slot.</param>
        public void UnassignSkillFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCount)
            {
                Debug.LogError("Skill Bar: Slot index out of range.");
                return;
            }

            slots[slotIndex].skill = null;
            slots[slotIndex].ReloadSprite();
        }

        /// <summary>
        /// Uses a skill by triggering its slot.
        /// </summary>
        /// <param name="slotIndex">The slot index.</param>
        public void UseSkill(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count)
            {
                Debug.LogError("Skill Bar: Slot index out of range.");
                return;
            }

            slots[slotIndex].UseSkill();
        }

        /// <summary>
        /// Updates the placement of the skill bar.
        /// </summary>
        public void UpdatePlacement()
        {
            if (!content)
            {
                return;
            }

            // Reset padding if placement is changed
            if (prevPlacement != placement)
            {
                barPadding = new RectPadding();
                barPadding.left = 0;
                barPadding.right = 0;
                barPadding.top = 0;
                barPadding.bottom = 0;
            }

            // Set rect transform values to position the skill bar based on the placement
            switch (placement)
            {
                case Placement.Bottom:
                    content.anchorMin = new Vector2(0, 0);
                    content.anchorMax = new Vector2(1, 0);
                    content.sizeDelta = new Vector2(0, barSize);
                    content.anchoredPosition = new Vector2(0, barSize / 2f);
                    barPadding.top = -barSize;
                    break;

                case Placement.Top:
                    content.anchorMin = new Vector2(0, 1);
                    content.anchorMax = new Vector2(1, 1);
                    content.sizeDelta = new Vector2(0, barSize);
                    content.anchoredPosition = new Vector2(0, barSize / -2f);
                    barPadding.bottom = -barSize;
                    break;

                case Placement.Left:
                    content.anchorMin = new Vector2(0, 0);
                    content.anchorMax = new Vector2(0, 1);
                    content.sizeDelta = new Vector2(barSize, 0);
                    content.anchoredPosition = new Vector2(barSize / 2f, 0);
                    barPadding.right = -barSize;
                    break;

                case Placement.Right:
                    content.anchorMin = new Vector2(1, 0);
                    content.anchorMax = new Vector2(1, 1);
                    content.sizeDelta = new Vector2(barSize, 0);
                    content.anchoredPosition = new Vector2(barSize / -2f, 0);
                    barPadding.left = -barSize;
                    break;
            }

            content.SetPadding(barPadding);

            // Switch layout group based on placement
            if (content.TryGetComponent(out LayoutGroup layout))
            {
                DestroyImmediate(layout);
            }

            if (placement == Placement.Bottom || placement == Placement.Top)
            {
                layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
                var horizontal = layout as HorizontalLayoutGroup;
                horizontal.childControlWidth = false;
                horizontal.childControlHeight = false;
                horizontal.spacing = slotSpacing;
                horizontal.childAlignment = slotAlignment;
                horizontal.padding = contentPadding;
            }
            else
            {
                layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
                var vertical = layout as VerticalLayoutGroup;
                vertical.childControlWidth = false;
                vertical.childControlHeight = false;
                vertical.spacing = slotSpacing;
                vertical.childAlignment = slotAlignment;
                vertical.padding = contentPadding;
            }

            // Clear existing slots
            while (content.childCount > 0)
            {
                DestroyImmediate(content.GetChild(0).gameObject);
            }
            slots.Clear();

            // Instantiate new slots
            for (int i = 0; i < slotCount; i++)
            {
                var slot = Instantiate(skillSlotPrefab, content);
                int index = i + 1;
                slot.SetKeyText(index.ToString());
                slot.ReloadSprite();
                slots.Add(slot);
            }

            prevPlacement = placement;
        }

        /// <summary>
        /// Supported skill bar placements.
        /// </summary>
        public enum Placement
        {
            Bottom,
            Top,
            Left,
            Right
        }
    }
}