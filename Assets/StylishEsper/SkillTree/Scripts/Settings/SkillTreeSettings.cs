//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;
using UnityEngine;

namespace Esper.SkillTree.Settings
{
    /// <summary>
    /// Skill tree settings.
    /// </summary>
    public class SkillTreeSettings : ScriptableObject
    {
        /// <summary>
        /// The main list of all skill types.
        /// </summary>
        public List<string> skillTypes;

        /// <summary>
        /// If downgrading skills should be allowed.
        /// </summary>
        public bool allowDowngrade;

        /// <summary>
        /// If making changes to skills should require confirmation before officially applying.
        /// </summary>
        public bool changesRequireConfirmation;

        /// <summary>
        /// If the numeric value types should match when doing math with stats.
        /// </summary>
        public bool strictStatValueTypes;

        /// <summary>
        /// The max level a unit can reach up to.
        /// </summary>
        public int maxUnitLevel;

#if UNITY_EDITOR
        /// <summary>
        /// Saves the settings asset (editor only).
        /// </summary>
        public void SaveSettings()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            UnityEditor.AssetDatabase.Refresh();
        }
#endif

        /// <summary>
        /// Gets a skill type at an index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The skill type or null if the index was out of range.</returns>
        public string GetSkillTypeAtIndex(int index)
        {
            if (index < 0 || index >= skillTypes.Count)
            {
                Debug.LogWarning("Skill Tree Settings: Skill type index out of range.");
                return null;
            }

            return skillTypes[index];
        }
    }
}