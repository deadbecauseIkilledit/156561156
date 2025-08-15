//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Esper.SkillTree.Stats
{
    /// <summary>
    /// Represents an identity of a stat.
    /// </summary>
    public class StatIdentity : ScriptableObject
    {
        private const string pathInResources = "SkillTree/Stats/Identities";
        private static string directoryPath = Path.Combine("Assets", "StylishEsper", "SkillTree", "Resources", "SkillTree", "Stats", "Identities");

        /// <summary>
        /// The reference to the stat group.
        /// </summary>
        public StatGroup groupReference;

        /// <summary>
        /// The stat ID.
        /// </summary>
        public int id;

        /// <summary>
        /// The display name of the stat.
        /// </summary>
        public string displayName;

        /// <summary>
        /// The abbreviation of the stat name.
        /// </summary>
        public string abbreviation;

        /// <summary>
        /// The numeric type of this stat.
        /// </summary>
        public StatValue.NumericType numericType;

        /// <summary>
        /// The value type of this stat.
        /// </summary>
        public StatValue.ValueType valueType;

        /// <summary>
        /// If this stat has a min value.
        /// </summary>
        public bool hasMin;

        /// <summary>
        /// If this stat has a max value.
        /// </summary>
        public bool hasMax;

        /// <summary>
        /// The minimum float value the stat can possibly decrease to.
        /// </summary>
        public float minFloatValue;

        /// <summary>
        /// The maximum float value the stat can possibly increase to.
        /// </summary>
        public float maxFloatValue;

        /// <summary>
        /// The minimum int value the stat can possibly decrease to.
        /// </summary>
        public int minIntegerValue;

        /// <summary>
        /// The maximum int value the stat can possibly increase to.
        /// </summary>
        public int maxIntegerValue;

        public StatValue MinValue
        {
            get
            {
                switch (numericType)
                {
                    case StatValue.NumericType.Float:
                        return new StatValue(this, minFloatValue);

                    case StatValue.NumericType.Integer:
                        return new StatValue(this, minIntegerValue);

                    default:
                        return new StatValue(this);
                }
            }
        }

        public StatValue MaxValue
        {
            get
            {
                switch (numericType)
                {
                    case StatValue.NumericType.Float:
                        return new StatValue(this, maxFloatValue);

                    case StatValue.NumericType.Integer:
                        return new StatValue(this, maxIntegerValue);

                    default:
                        return new StatValue(this);
                }
            }
        }

        /// <summary>
        /// Clamps a value within the min and max range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The clamped value.</returns>
        public StatValue.Value Clamp(StatValue.Value value)
        {
            value.floatValue = Clamp(value.floatValue);
            value.integerValue = Clamp(value.integerValue);
            return value;
        }

        /// <summary>
        /// Clamps the stat within the min and max range.
        /// </summary>
        /// <param name="stat">The stat.</param>
        /// <returns>The clamped stat.</returns>
        public StatValue Clamp(StatValue stat)
        {
            stat.value.floatValue = Clamp(stat.value.floatValue);
            stat.value.integerValue = Clamp(stat.value.integerValue);
            return stat;
        }

        /// <summary>
        /// Clamps a value within the min and max range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The clamped value.</returns>
        public float Clamp(float value)
        {
            if (hasMax && value > maxFloatValue)
            {
                value = maxFloatValue;
            }
            else if (hasMin && value < minFloatValue)
            {
                value = minFloatValue;
            }

            return value;
        }

        /// <summary>
        /// Clamps a value within the min and max range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The clamped value.</returns>
        public int Clamp(int value)
        {
            if (hasMax && value > maxIntegerValue)
            {
                value = maxIntegerValue;
            }
            else if (hasMin && value < minIntegerValue)
            {
                value = minIntegerValue;
            }

            return value;
        }

        /// <summary>
        /// Creates a new instance of a StatIdentity (editor only).
        /// </summary>
        /// <param name="group">The stat group.</param>
        /// <returns>The created instance.</returns>
        public static StatIdentity Create(StatGroup group)
        {
#if UNITY_EDITOR
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
#endif

            var id = GetID();
            var obj = CreateInstance<StatIdentity>();
            var name = "New Stat";
            obj.displayName = name;
            obj.id = id;

            if (group)
            {
                obj.groupReference = group;
                group.statIdentities.Add(obj);
                group.Save();
                obj.numericType = group.defaultNumericType;
                obj.valueType = group.defaultValueType;
                obj.hasMin = group.defaultHasMin;
                obj.hasMax = group.defaultHasMax;
                obj.minFloatValue = group.defaultMinFloatValue;
                obj.maxFloatValue = group.defaultMaxFloatValue;
                obj.minIntegerValue = group.defaultMinIntegerValue;
                obj.maxIntegerValue = group.defaultMaxIntegerValue;
            }

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
        public static string GetFullPath(StatIdentity stat)
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
            var itemTypes = Resources.LoadAll<StatIdentity>(pathInResources);
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