//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.SkillTree.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.SkillTree.Examples
{
    /// <summary>
    /// Status UI example.
    /// </summary>
    public class ExampleStatusUI : MonoBehaviour
    {
        [SerializeField]
        private Slider hpSlider;

        [SerializeField]
        private Slider expSlider;

        [SerializeField]
        private TextMeshProUGUI levelText;

        private void Start()
        {
            SkillTree.playerStats.onStatChanged.AddListener((x) => UpdateUI());
            UpdateUI();
        }

        /// <summary>
        /// Updates the status UI.
        /// </summary>
        public void UpdateUI()
        {
            var player = SkillTree.playerStats;
            var hp = player.GetStat("HP");
            var exp = player.GetStat("EXP");
            hpSlider.maxValue = hp.MaxValue.ToFloat();
            hpSlider.value = hp.currentValue.ToFloat();
            expSlider.maxValue = exp.MaxValue.ToFloat();
            expSlider.value = exp.currentValue.ToFloat();
            levelText.text = player.level.ToString();
        }
    }
}