//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Esper.SkillTree.Stats
{
    /// <summary>
    /// Groups stats and sets common traits.
    /// </summary>
    public class StatGroup : ScriptableObject
    {
        private const string pathInResources = "SkillTree/Stats/Groups";
        private static string directoryPath = Path.Combine("Assets", "StylishEsper", "SkillTree", "Resources", "SkillTree", "Stats", "Groups");

        /// <summary>
        /// The ID of the stat group.
        /// </summary>
        public int id;

        /// <summary>
        /// The display name of the stat group.
        /// </summary>
        public string displayName;

        /// <summary>
        /// The default numeric type that will be applied to newly created stats in this group.
        /// </summary>
        public StatValue.NumericType defaultNumericType;

        /// <summary>
        /// The default value type that will be applied to newly created stats in this group.
        /// </summary>
        public StatValue.ValueType defaultValueType;

        /// <summary>
        /// The default has minimum value that will be applied to newly created stats in this group.
        /// </summary>
        public bool defaultHasMin;

        /// <summary>
        /// The default has minimum value that will be applied to newly created stats in this group.
        /// </summary>
        public bool defaultHasMax;

        /// <summary>
        /// The default minimum float value that will be applied to newly created stats in this group.
        /// </summary>
        public float defaultMinFloatValue;

        /// <summary>
        /// The default maximum float value that will be applied to newly created stats in this group.
        /// </summary>
        public float defaultMaxFloatValue;

        /// <summary>
        /// The default minimum int value that will be applied to newly created stats in this group.
        /// </summary>
        public int defaultMinIntegerValue;

        /// <summary>
        /// The default maximum int value that will be applied to newly created stats in this group.
        /// </summary>
        public int defaultMaxIntegerValue;

        /// <summary>
        /// A list of all stat identities that are a part of this stat group.
        /// </summary>
        public List<StatIdentity> statIdentities = new();

        /// <summary>
        /// Creates a new instance of a StatGroup (editor only).
        /// </summary>
        /// <returns>The created instance.</returns>
        public static StatGroup Create()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
#endif

            var id = GetID();
            var obj = CreateInstance<StatGroup>();
            var name = "New Group";
            obj.displayName = name;
            obj.id = id;

#if UNITY_EDITOR
            var path = Path.Combine(directoryPath, $"{id}_{name}.asset");
            UnityEditor.AssetDatabase.CreateAsset(obj, path);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            return obj;
        }

        /// <summary>
        /// Updates the name of the asset (editor only).
        /// </summary>
        public void UpdateAssetName()
        {
#if UNITY_EDITOR
            string name = $"{id}_{displayName}";
            UnityEditor.EditorApplication.delayCall += () =>
            {
                UnityEditor.AssetDatabase.RenameAsset(GetFullPath(this), name);

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    this.name = name;
                };
            };
#endif
        }

        /// <summary>
        /// Gets the full path of a StatIdentity (editor only).
        /// </summary>
        /// <param name="stat">The StatIdentity.</param>
        /// <returns>The full path to the stat.</returns>
        public static string GetFullPath(StatGroup stat)
        {
            return Path.Combine(directoryPath, $"{stat.name}.asset");
        }

        /// <summary>
        /// Saves the object (editor only).
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        /// <summary>
        /// Returns an unused ID.
        /// </summary>
        /// <returns>An unused ID.</returns>
        private static int GetID()
        {
            var itemTypes = Resources.LoadAll<StatGroup>(pathInResources);
            var ids = new List<int>();

            foreach (var itemType in itemTypes)
            {
                ids.Add(itemType.id);
            }

            for (int i = 0; i < int.MaxValue; i++)
            {
                if (!ids.Contains(i))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}