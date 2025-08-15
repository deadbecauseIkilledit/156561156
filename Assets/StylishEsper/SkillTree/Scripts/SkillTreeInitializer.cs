//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Initializes Skill Tree.
//***************************************************************************************

using Esper.SkillTree.Stats;
using UnityEngine;

namespace Esper.SkillTree
{
    [DefaultExecutionOrder(-1)]
    public class SkillTreeInitializer : MonoBehaviour
    {
        [SerializeField, Tooltip("The player's stats provider. Not required.")]
        private UnitStatsProvider playerStatsProvider;

        [SerializeField, Tooltip("If this is checked, this component will be singleton and won't be destroyed on load.")]
        private bool singleton;

        private static SkillTreeInitializer instance;

        private void Awake()
        {
            if (singleton)
            { 
                if (instance)
                {
                    Destroy(gameObject);
                    return;
                }

                instance = this;
                DontDestroyOnLoad(gameObject);
            }

            SkillTree.Initialize();
            SkillTree.playerStats = playerStatsProvider;
        }
    }
}