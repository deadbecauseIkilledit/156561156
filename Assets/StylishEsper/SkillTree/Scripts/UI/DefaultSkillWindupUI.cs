//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Skill windup visualizer.
//***************************************************************************************

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    public class DefaultSkillWindupUI : SkillTreeUI
    {
        [SerializeField]
        private Slider slider;

        [SerializeField]
        private TextMeshProUGUI timeTextMesh;

        private UsedSkill usedSkill;

        /// <summary>
        /// Windup UI instance. This may be null if a DefaultSkillWindupUI object doesn't exist in the scene.
        /// </summary>
        public static DefaultSkillWindupUI instance;

        private void Awake()
        {
            // Callback to display the windup whenever player skill is used
            SkillTree.onSkillUsed.AddListener(DisplayWindup);

            instance = this;
            Close();
        }

        /// <summary>
        /// Displays the windup UI.
        /// </summary>
        /// <param name="usedSkill">The used skill.</param>
        public void DisplayWindup(UsedSkill usedSkill)
        {
            if (!usedSkill.skill.IsPlayerSkill || !usedSkill.isWindingUp)
            {
                return;
            }

            this.usedSkill = usedSkill;
            slider.maxValue = usedSkill.skill.windupDuration;
            usedSkill.OnWindupUpdated(WindupHandler);
            Open();
        }

        /// <summary>
        /// Handles windup visuals.
        /// </summary>
        /// <param name="remainingTime">The windup time remaining.</param>
        private void WindupHandler(float remainingTime)
        {
            float timePassed = usedSkill.skill.windupDuration - remainingTime;
            slider.value = timePassed;
            string formatted = $"{timePassed:0.00}s";
            timeTextMesh.text = formatted;

            if (timePassed >= usedSkill.skill.windupDuration)
            {
                Close();
            }
        }
    }
}