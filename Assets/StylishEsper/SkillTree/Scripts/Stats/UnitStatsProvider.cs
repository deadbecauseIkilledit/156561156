//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;
using UnityEngine.Events;

namespace Esper.SkillTree.Stats
{
    /// <summary>
    /// Can be used to provide stats to a GameObject.
    /// </summary>
    public class UnitStatsProvider : MonoBehaviour
    {
        /// <summary>
        /// Unit stats data.
        /// </summary>
        public UnitStats data;

        /// <summary>
        /// Called whenever a stat's current value is changed.
        /// </summary>
        public UnityEvent<Stat> onStatChanged { get; private set; } = new();

        /// <summary>
        /// Called whenever a stat's current value reaches its min value.
        /// </summary>
        public UnityEvent<Stat> onStatReachedMin { get; private set; } = new();

        /// <summary>
        /// Called whenever a stat's current value reaches its max value.
        /// </summary>
        public UnityEvent<Stat> onStatReachedMax { get; private set; } = new();

        /// <summary>
        /// Called whenever a stat's level is changed.
        /// </summary>
        public UnityEvent<Stat> onStatLevelChanged { get; private set; } = new();

        /// <summary>
        /// The level of the unit.
        /// </summary>
        public int level { get => data.unitLevel; }

        private void Awake()
        {
            Refresh();
            ResetAllStats();
        }

        /// <summary>
        /// Refreshes the stats provider.
        /// </summary>
        public void Refresh()
        {
            data.Initialize(this);
            data.UpdateStats();
        }

        /// <summary>
        /// Sets the current value of a stat.
        /// </summary>
        /// <param name="name">The name of the stat.</param>
        /// <param name="value">The value to set to.</param>
        public void SetStat(string name, float value)
        {
            var stat = GetStat(name);

            if (stat != null)
            {
                stat.currentValue = new StatValue(stat.Identity, value);
            }
        }

        /// <summary>
        /// Sets the current value of a stat.
        /// </summary>
        /// <param name="name">The name of the stat.</param>
        /// <param name="value">The value to set to.</param>
        public void SetStat(string name, int value)
        {
            var stat = GetStat(name);

            if (stat != null)
            {
                stat.currentValue = new StatValue(stat.Identity, value);
            }
        }

        /// <summary>
        /// Increases a stat's current value by an amount.
        /// </summary>
        /// <param name="name">The name of the stat.</param>
        /// <param name="value">The amount to increase.</param>
        /// <returns>The excess value.</returns>
        public StatValue IncreaseStat(string name, float value)
        {
            var stat = GetStat(name);

            if (stat != null)
            {
                stat += value;
                return stat.GetExcessValue();
            }

            return default;
        }

        /// <summary>
        /// Increases a stat's current value by an amount.
        /// </summary>
        /// <param name="name">The name of the stat.</param>
        /// <param name="value">The amount to increase.</param>
        /// <returns>The excess value.</returns>
        public StatValue IncreaseStat(string name, int value)
        {
            var stat = GetStat(name);

            if (stat != null)
            {
                stat += value;
                return stat.GetExcessValue();
            }

            return default;
        }

        /// <summary>
        /// Decreases a stat's current value by an amount.
        /// </summary>
        /// <param name="name">The name of the stat.</param>
        /// <param name="value">The amount to decrease.</param>
        /// <returns>The excess value.</returns>
        public StatValue DecreaseStat(string name, float value)
        {
            var stat = GetStat(name);

            if (stat != null)
            {
                stat -= value;
                return stat.GetExcessValue();
            }

            return default;
        }

        /// <summary>
        /// Decreases a stat's current value by an amount.
        /// </summary>
        /// <param name="name">The name of the stat.</param>
        /// <param name="value">The amount to decrease.</param>
        /// <returns>The excess value.</returns>
        public StatValue DecreaseStat(string name, int value)
        {
            var stat = GetStat(name);

            if (stat != null)
            {
                stat -= value;
                return stat.GetExcessValue();
            }

            return default;
        }

        /// <summary>
        /// Gets a stat by name.
        /// </summary>
        /// <param name="name">The name of the stat.</param>
        /// <returns>The stat or null if one with the name doesn't exist.</returns>
        public Stat GetStat(string name)
        {
            foreach (var item in data.stats)
            {
                if (item.Identity.name == name || item.Identity.abbreviation == name)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Increases the unit's level by 1.
        /// </summary>
        public void LevelUp()
        {
            SetLevel(data.unitLevel + 1);
        }

        /// <summary>
        /// Decreases the unit's level by 1. 
        /// </summary>
        public void LevelDown()
        {
            SetLevel(data.unitLevel - 1);
        }

        /// <summary>
        /// Sets the unit's level.
        /// </summary>
        /// <param name="level">The level to set to.</param>
        public void SetLevel(int level)
        {
            data.SetLevel(level);
        }

        /// <summary>
        /// Resets all stat current values.
        /// </summary>
        public void ResetAllStats()
        {
            foreach (var item in data.stats)
            {
                item.Reset();
                item.maxLevel = SkillTree.Settings.maxUnitLevel;
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (data == null)
            {
                data = new UnitStats();
            }

            var identities = SkillTree.GetAllStatIdentities();
            foreach (var item in identities)
            {
                data.AddStat(new Stat(item));
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}