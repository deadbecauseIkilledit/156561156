//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;
using UnityEngine;

namespace Esper.SkillTree.Stats
{
    /// <summary>
    /// Represents a list of stats for a unit.
    /// </summary>
    [System.Serializable]
    public class UnitStats
    {
        /// <summary>
        /// The name of this unit.
        /// </summary>
        public string unitName;

        /// <summary>
        /// The level of this unit. It's recommended to use UnitStats.LevelUp or UnitStats.LevelDown to change
        /// this value.
        /// </summary>
        public int unitLevel = 1;

        /// <summary>
        /// A list of upgradable stats for this unit.
        /// </summary>
        public List<Stat> stats = new();

        /// <summary>
        /// Initializes the stats.
        /// </summary>
        /// <param name="statsProviderReference">The UnitStatsProvider reference.</param>
        public void Initialize(UnitStatsProvider statsProviderReference)
        {
            foreach (var stat in stats)
            {
                stat.statsProviderReference = statsProviderReference;
                stat.SetIdentity(stat.Identity);
            }
        }

        /// <summary>
        /// Sets the unit's level.
        /// </summary>
        /// <param name="level">The level to set to.</param>
        public void SetLevel(int level)
        {
            // Preventing level from getting less than 1 and over max unit level
            if (unitLevel < 1 || unitLevel > SkillTree.Settings.maxUnitLevel)
            {
                Debug.LogWarning($"Stats: Level cannot be below 1 or above {SkillTree.Settings.maxUnitLevel}.");
                return;
            }

            unitLevel = level;
            UpdateStats();
        }

        /// <summary>
        /// Updates stat values so that they reflect the unit's level.
        /// </summary>
        public void UpdateStats()
        {
            foreach (var stat in stats)
            {
                stat.SetLevel(unitLevel);
            }
        }

        /// <summary>
        /// Resets all stats.
        /// </summary>
        public void ResetStats()
        {
            foreach (var stat in stats)
            {
                stat.Reset();
            }
        }

        /// <summary>
        /// Adds a stat.
        /// </summary>
        /// <param name="stat">The stat to add.</param>
        public void AddStat(Stat stat)
        {
            stats.Add(stat);
            UpdateStats();
        }

        /// <summary>
        /// Removes a stat by name.
        /// </summary>
        /// <param name="name">The name or short name of the stat's identity.</param>
        public void RemoveStat(string name)
        {
            foreach (Stat stat in stats)
            {
                if (stat.NameMatches(name))
                {
                    stats.Remove(stat);
                    break;
                }
            }
        }

        /// <summary>
        /// Removes an upgradable stat by index.
        /// </summary>
        /// <param name="index">The stat index.</param>
        public void RemoveStat(int index)
        {
            if (index < 0 || index >= stats.Count)
            {
                Debug.LogError("Stats: Index out of range.");
                return;
            }

            stats.RemoveAt(index);
        }
    }
}