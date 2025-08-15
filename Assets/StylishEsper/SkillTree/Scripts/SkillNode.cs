//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.SkillTree.Stats;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Esper.SkillTree
{
    /// <summary>
    /// A container that stores skill data and connections.
    /// </summary>
    public class SkillNode : ScriptableObject
    {
        /// <summary>
        /// The skill graphs directory. Works in the editor only.
        /// </summary>
        public static string directoryPath = Path.Combine("Assets", "StylishEsper", "SkillTree", "Resources", "SkillTree", "SkillNodes");

        /// <summary>
        /// The position index of the node.
        /// </summary>
        public int positionIndex;

        /// <summary>
        /// The reference of the graph that this connection is a part of.
        /// </summary>
        public SkillGraph graphReference;

        /// <summary>
        /// A key that can be used to get the node in-game.
        /// </summary>
        public string key;

        /// <summary>
        /// The name of the skill that should be displayed in-game.
        /// </summary>
        public string displayName;

        /// <summary>
        /// The description of the skill. This should explain what the skill does.
        /// </summary>
        public string description;

        /// <summary>
        /// The index of the skill type.
        /// </summary>
        public int skillTypeIndex;

        /// <summary>
        /// The sprite to use when the skill is locked.
        /// </summary>
        public Sprite lockedSprite;

        /// <summary>
        /// The sprite to use when the skill is unlocked.
        /// </summary>
        public Sprite unlockedSprite;

        /// <summary>
        /// The sprite to use when the skill is obtained.
        /// </summary>
        public Sprite obtainedSprite;

        /// <summary>
        /// The current skill level.
        /// </summary>
        public int currentLevel;

        /// <summary>
        /// The max skill level.
        /// </summary>
        public int maxLevel;

        /// <summary>
        /// The level of the player required for this skill node to become unlocked.
        /// </summary>
        public int playerLevelRequirement;

        /// <summary>
        /// The number of points required to be spent on this skill's skill tree for this skill
        /// to become unlocked.
        /// </summary>
        public int treeTotalPointsSpentRequirement;

        /// <summary>
        /// The cooldown of this skill.
        /// </summary>
        public float cooldown;

        /// <summary>
        /// How long this skill should last.
        /// </summary>
        public float duration;

        /// <summary>
        /// The skill windup length.
        /// </summary>
        public float windupDuration;

        /// <summary>
        /// If this skill runs an effect per second.
        /// </summary>
        public bool isEffectOverTime;

        /// <summary>
        /// The length of the effect.
        /// </summary>
        public float effectDuration;

        /// <summary>
        /// The range of this skill.
        /// </summary>
        public float range;

        /// <summary>
        /// The speed of this skill.
        /// </summary>
        public float speed;

        /// <summary>
        /// The cost (mana/energy) of the skill.
        /// </summary>
        public float cost;

        /// <summary>
        /// If this skill is single target.
        /// </summary>
        public bool isSingleTarget;

        /// <summary>
        /// If this skill can be canceled.
        /// </summary>
        public bool isCancelable;

        /// <summary>
        /// The unique shape type of this node.
        /// </summary>
        public Shape uniqueShape;

        /// <summary>
        /// The unique size of this node.
        /// </summary>
        public float uniqueSize;

        /// <summary>
        /// If this node is unique.
        /// </summary>
        public bool isUnique;

        /// <summary>
        /// A custom object for any purpose.
        /// </summary>
        public Object customObject;

        /// <summary>
        /// The current state of the node.
        /// </summary>
        public State state;

        /// <summary>
        /// If this skill belongs to the player.
        /// </summary>
        public bool IsPlayerSkill { get => graphReference.forPlayer; }

        /// <summary>
        /// A list of skill stats.
        /// </summary>
        public List<Stat> stats = new();

        /// <summary>
        /// If this skill is locked.
        /// </summary>
        public bool IsLocked { get => state == State.Locked; }

        /// <summary>
        /// If this skill is unlocked.
        /// </summary>
        public bool IsUnlocked { get => state == State.Unlocked; }

        /// <summary>
        /// If this skill has been obtained.
        /// </summary>
        public bool IsObtained { get => state == State.Obtained || state == State.Maxed; }

        /// <summary>
        /// If this skill is maxed out.
        /// </summary>
        public bool IsMaxed { get => state == State.Maxed; }

        /// <summary>
        /// A simple string that displays the current and max level.
        /// </summary>
        public string LevelDisplayString
        {
            get
            {
                string s = $"{currentLevel}/{maxLevel}";
                return s;
            }
        }

        /// <summary>
        /// Gets the skill type.
        /// </summary>
        /// <returns>The skill type.</returns>
        public string GetSkillType()
        {
            return SkillTree.Settings.skillTypes[skillTypeIndex];
        }

        /// <summary>
        /// Gets the sprite based on the skill state.
        /// </summary>
        /// <returns>The sprite.</returns>
        public Sprite GetSprite()
        {
            switch (state)
            {
                case State.Locked:
                    return lockedSprite;
                case State.Unlocked:
                    return unlockedSprite;
                case State.Obtained:
                    return obtainedSprite;
                case State.Maxed:
                    return obtainedSprite;
                default:
                    return lockedSprite;
            }
        }

        /// <summary>
        /// Gets a stat by name.
        /// </summary>
        /// <param name="name">The name of the stat.</param>
        /// <returns>The stat or a new empty stat object if one with the name doesn't exist.</returns>
        public Stat GetStat(string name)
        {
            foreach (var stat in stats)
            {
                if (stat.Identity.name == name || stat.Identity.abbreviation == name)
                {
                    return stat;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a stat by index.
        /// </summary>
        /// <param name="index">The index of the stat.</param>
        /// <returns>The stat at the index or null if the index was out of range.</returns>
        public Stat GetStat(int index)
        {
            if (index < 0 || index > stats.Count)
            {
                return null;
            }

            return stats[index];
        }

        /// <summary>
        /// Creates and returns a SavableSkillNode from this skill.
        /// </summary>
        /// <returns>The created SavableSkillNode.</returns>
        public SavableSkillGraph.SavableSkillNode CreateSavableNode()
        {
            var savable = new SavableSkillGraph.SavableSkillNode()
            {
                positionIndex = positionIndex,
                currentLevel = currentLevel,
                state = state
            };

            return savable;
        }

        /// <summary>
        /// Checks if the node data has been correctly set.
        /// </summary>
        /// <returns>True if all minimum data has been correctly set. Otherwise, false.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(displayName) &&
                !string.IsNullOrEmpty(description) && maxLevel > 0 && stats.Count > 0 && range >= 0
                && speed >= 0 && playerLevelRequirement >= 0 && treeTotalPointsSpentRequirement >= 0
                && cooldown >= 0 && duration >= 0 && windupDuration >= 0 && uniqueSize >= 1;
        }

        /// <summary>
        /// Gets the description with variable interpolation.
        /// </summary>
        /// <returns>Interpolated description.</returns>
        public string GetInterpolatedDescription()
        {
            return GetInterpolatedText(description);
        }

        /// <summary>
        /// Gets the text with variable interpolation related to this skill.
        /// </summary>
        /// <param name="original">The original text.</param>
        /// <returns>Interpolated version of the string.</returns>
        public string GetInterpolatedText(string original)
        {
            string interpolated = original;

            for (int i = 0; i < stats.Count; i++)
            {
                interpolated = interpolated.Replace($"{{stat{i + 1}_value}}", stats[i].currentLevel == 0 ? stats[i].NextBaseValue.ToString() : stats[i].BaseValue.ToString());
                interpolated = interpolated.Replace($"{{stat{i + 1}_name}}", stats[i].Identity.name);
                interpolated = interpolated.Replace($"{{stat{i + 1}_abbr}}", stats[i].Identity.abbreviation);
            }

            interpolated = interpolated.Replace("{display_name}", displayName);
            interpolated = interpolated.Replace("{cooldown}", cooldown.ToString());
            interpolated = interpolated.Replace("{duration}", duration.ToString());
            interpolated = interpolated.Replace("{windup}", windupDuration.ToString());
            interpolated = interpolated.Replace("{effect_duration}", effectDuration.ToString());
            interpolated = interpolated.Replace("{range}", range.ToString());
            interpolated = interpolated.Replace("{speed}", speed.ToString());

            return interpolated;
        }

        /// <summary>
        /// Checks whether this skill has connection unlock requirements.
        /// </summary>
        /// <returns>True if this skill has connection unlock requirements. Otherwise, false.</returns>
        public bool RequiresConnectionsToUnlock()
        {
            var connections = graphReference.GetConnectionsOfNode(this);

            foreach (var connection in connections)
            {
                if (!connection.twoWay && connection.skillNodeB == this)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If this skill has any connection to other skills that are obtained.
        /// </summary>
        /// <returns>True if this skill has any connection to skills that are obtained. Otherwise, false.</returns>
        public bool HasObtainedConnections()
        {
            var connections = graphReference.GetConnectionsOfNode(this);

            foreach (var connection in connections)
            {
                if (connection.twoWay)
                {
                    if (connection.skillNodeA == this)
                    {
                        var skillB = connection.skillNodeB;

                        if (skillB.IsObtained)
                        {
                            return true;
                        }
                    }
                    else if (connection.skillNodeB == this)
                    {
                        var skillA = connection.skillNodeA;

                        if (skillA.IsObtained)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (connection.skillNodeB == this)
                    {
                        var skillA = connection.skillNodeA;

                        if (skillA.IsObtained)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to unlock this node. Unlocked nodes are upgradable.
        /// 
        /// Skills cannot be unlocked if the player doesn't meet the level requirement, no obtained
        /// skill leads to this skill through a connection, or if enough required skill points
        /// haven't been spent on skills in a specific skill tree.
        /// </summary>
        /// <param name="withoutNotify">If events should not be invoked. Default: false.</param>
        /// <returns>True if successfully unlocked. Otherwise, false.</returns>
        public bool TryUnlock(bool withoutNotify = false)
        {
            if (!IsLocked)
            {
                return false;
            }

            bool requiresConnections = RequiresConnectionsToUnlock();
            bool hasObtainedConnections = HasObtainedConnections();

            if (!requiresConnections || (requiresConnections && hasObtainedConnections))
            {
                state = State.Unlocked;

                if (withoutNotify)
                {
                    SkillTree.onSkillStateChanged.Invoke(this);
                    SkillTree.onGraphChanged.Invoke(graphReference);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to unlock all skills that are connected to this skill. Nothing will happen if this
        /// skill is not obtained.
        /// </summary>
        public void TryUnlockAllThroughConnections()
        {
            if (!IsObtained)
            {
                return;
            }

            var connections = graphReference.GetConnectionsOfNode(this);

            foreach (var connection in connections)
            {
                if (connection.twoWay)
                {
                    SkillNode other = null;

                    if (connection.skillNodeA == this)
                    {
                        other = connection.skillNodeB;
                    }
                    else if (connection.skillNodeB == this)
                    {
                        other = connection.skillNodeA;
                    }

                    if (other != null)
                    {
                        other.TryUnlock();
                    }
                }
                else if (connection.skillNodeA == this)
                {
                    connection.skillNodeB.TryUnlock();
                }
            }
        }

        /// <summary>
        /// Increases the skill level by an amount if possible.
        /// </summary>
        /// <param name="amount">The amount to increase by. Default: 1.</param>
        /// <param name="withoutNotify">If events should not be invoked. Default: false.</param>
        /// <returns>True if successfully upgraded. Otherwise, false.</returns>
        public bool TryUpgrade(int amount = 1, bool withoutNotify = false)
        {
            if (amount <= 0 || IsLocked || IsMaxed || SkillTree.playerSkillPoints <= 0 || SkillTree.playerLevel < playerLevelRequirement || treeTotalPointsSpentRequirement > graphReference.CountPointsSpent())
            {
                return false;
            }

            currentLevel += amount;

            if (currentLevel > maxLevel)
            {
                amount -= currentLevel - maxLevel;
                currentLevel = maxLevel;
            }

            SetStatLevels();

            SkillTree.playerSkillPoints -= amount;

            if (currentLevel == maxLevel)
            {
                state = State.Maxed;

                if (!withoutNotify)
                {
                    SkillTree.onSkillStateChanged.Invoke(this);
                }

                TryUnlockAllThroughConnections();
            }
            else if (currentLevel == 1)
            {
                state = State.Obtained;

                if (!withoutNotify)
                {
                    SkillTree.onSkillStateChanged.Invoke(this);
                }

                TryUnlockAllThroughConnections();
            }

            if (!withoutNotify)
            {
                SkillTree.onSkillUpgraded.Invoke(this);
                SkillTree.onGraphChanged.Invoke(graphReference);
            }

            return true;
        }

        /// <summary>
        /// Decreases the skill level by an amount if possible. Please check Skill Tree's setting to ensure
        /// downgrading is enabled.
        /// </summary>
        /// <param name="amount">The amount to decrease by. Default: 1.</param>
        /// <param name="withoutNotify">If events should not be invoked.</param>
        /// <param name="forceDowngrade">If the 'Allow Downgrade' value in Skill Tree's settings should be ignored.</param>
        /// <returns>True if successfully downgraded. Otherwise, false.</returns>
        public bool TryDowngrade(int amount = 1, bool withoutNotify = false, bool forceDowngrade = false)
        {
            if (amount <= 0 || IsLocked || currentLevel == 0 || (!forceDowngrade && !SkillTree.Settings.allowDowngrade))
            {
                return false;
            }

            currentLevel -= amount;

            if (currentLevel < 0)
            {
                amount += currentLevel;
                currentLevel = 0;
            }

            SetStatLevels();

            SkillTree.playerSkillPoints += amount;

            if (!withoutNotify)
            {
                SkillTree.onSkillDowngraded.Invoke(this);
            }

            var graph = graphReference;

            if (currentLevel == 0)
            {
                state = State.Unlocked;

                if (!withoutNotify)
                {
                    SkillTree.onSkillStateChanged.Invoke(this);
                    SkillTree.onSkillDepleted.Invoke(this);
                }

                var connections = graph.GetConnectionsOfNode(this);

                // Lock and deplete all possible skills through connections since this node lost its obtained state
                foreach (var connection in connections)
                {
                    SkillNode other = null;

                    if (connection.twoWay)
                    {
                        if (connection.skillNodeA == this)
                        {
                            other = connection.skillNodeB;
                        }
                        else if (connection.skillNodeB == this)
                        {
                            other = connection.skillNodeA;
                        }
                    }
                    else if (connection.skillNodeA == this)
                    {
                        other = connection.skillNodeB;
                    }

                    if (other != null && !other.HasObtainedConnections() && other.RequiresConnectionsToUnlock())
                    {
                        other.Deplete(true);
                        other.state = State.Locked;
                        SkillTree.onSkillStateChanged.Invoke(other);
                    }
                }
            }
            else if (currentLevel < maxLevel)
            {
                state = State.Obtained;

                if (!withoutNotify)
                {
                    SkillTree.onSkillStateChanged.Invoke(this);
                }
            }

            graph.DepleteNodesWhereNecessary();

            if (!withoutNotify)
            {
                SkillTree.onGraphChanged.Invoke(graphReference);
            }

            return true;
        }

        /// <summary>
        /// Completely removes all upgrades.
        /// </summary>
        /// <param name="withoutNotify">If events should not be invoked. Default: false.</param>
        /// <param name="forceDeplete">If the 'Allow Downgrade' value in Skill Tree's settings should be ignored.</param>
        public void Deplete(bool withoutNotify = false, bool forceDeplete = false)
        {
            TryDowngrade(currentLevel, withoutNotify, forceDeplete);
        }

        /// <summary>
        /// Sets the level of all stats to the current skill level.
        /// </summary>
        private void SetStatLevels()
        {
            foreach (var item in stats)
            {
                item.currentLevel = currentLevel;
            }
        }

        /// <summary>
        /// Resets all stats.
        /// </summary>
        public void ResetStats()
        {
            foreach (var item in stats)
            {
                item.Reset();
                item.maxLevel = maxLevel;
            }
        }

        /// <summary>
        /// Entirely copies the data of another skill node.
        /// </summary>
        /// <param name="other">The other skill node.</param>
        public void CopyData(SkillNode other)
        {
            positionIndex = other.positionIndex;
            key = other.key + " (Copy)";
            displayName = other.displayName;
            description = other.description;
            lockedSprite = other.lockedSprite;
            unlockedSprite = other.unlockedSprite;
            obtainedSprite = other.obtainedSprite;
            currentLevel = other.currentLevel;
            maxLevel = other.maxLevel;
            playerLevelRequirement = other.playerLevelRequirement;
            effectDuration = other.effectDuration;
            isUnique = other.isUnique;
            uniqueShape = other.uniqueShape;
            uniqueSize = other.uniqueSize;
            state = other.state;
            cooldown = other.cooldown;
            duration = other.duration;
            skillTypeIndex = other.skillTypeIndex;
            isSingleTarget = other.isSingleTarget;
            treeTotalPointsSpentRequirement = other.treeTotalPointsSpentRequirement;
            isCancelable = other.isCancelable;
            isEffectOverTime = other.isEffectOverTime;
            windupDuration = other.windupDuration;
            range = other.range;
            speed = other.speed;
            stats = other.stats;
            cost = other.cost;
            customObject = other.customObject;
        }

        /// <summary>
        /// Copies the data from a savable skill node.
        /// </summary>
        /// <param name="savable">The other skill node.</param>
        public void CopyData(SavableSkillGraph.SavableSkillNode savable)
        {
            currentLevel = savable.currentLevel;
            state = savable.state;
            SetStatLevels();
        }

        /// <summary>
        /// Saves the skill node (editor only).
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        /// <summary>
        /// Creates a new instance of a skill node (editor only).
        /// </summary>
        /// <param name="name">The name of the skill node object.</param>
        /// <param name="positionIndex">The position index.</param>
        /// <param name="graph">The skill graph that this node is a part of.</param>
        /// <returns>The created skill node instance.</returns>
        public static SkillNode Create(string name, int positionIndex, SkillGraph graph)
        {
            var path = Path.Combine(directoryPath, graph.id.ToString());

#if UNITY_EDITOR
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
#endif

            var obj = CreateInstance<SkillNode>();
            obj.positionIndex = positionIndex;
            obj.graphReference = graph;
            obj.key = name;
            obj.name = name;

#if UNITY_EDITOR
            var fullPath = Path.Combine(path, $"{name}.asset");
            UnityEditor.AssetDatabase.CreateAsset(obj, fullPath);
#endif

            return Resources.Load<SkillNode>($"SkillTree/SkillNodes/{graph.id}/{obj.name}");
        }

        /// <summary>
        /// Updates the name of the asset (editor only).
        /// </summary>
        public void UpdateAssetName()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                UnityEditor.AssetDatabase.RenameAsset(GetFullPath(this), key);

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    name = key;
                };
            };
#endif
        }

        /// <summary>
        /// Creates a copy of a skill node (editor only).
        /// </summary>
        /// <param name="original">The skill node to copy.</param>
        /// <param name="newGraph">The new skill graph.</param>
        /// <returns>The created copy.</returns>
        public static SkillNode CreateCopy(SkillNode original, SkillGraph newGraph)
        {
            var path = Path.Combine(directoryPath, newGraph.id.ToString());

#if UNITY_EDITOR
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
#endif

            var obj = CreateInstance<SkillNode>();
            obj.graphReference = newGraph;
            obj.positionIndex = original.positionIndex;
            obj.key = original.key + " (Copy)";
            obj.displayName = original.displayName;
            obj.description = original.description;
            obj.lockedSprite = original.lockedSprite;
            obj.unlockedSprite = original.unlockedSprite;
            obj.obtainedSprite = original.obtainedSprite;
            obj.currentLevel = original.currentLevel;
            obj.maxLevel = original.maxLevel;
            obj.playerLevelRequirement = original.playerLevelRequirement;
            obj.effectDuration = original.effectDuration;
            obj.isUnique = original.isUnique;
            obj.uniqueShape = original.uniqueShape;
            obj.uniqueSize = original.uniqueSize;
            obj.state = original.state;
            obj.cooldown = original.cooldown;
            obj.duration = original.duration;
            obj.skillTypeIndex = original.skillTypeIndex;
            obj.isSingleTarget = original.isSingleTarget;
            obj.treeTotalPointsSpentRequirement = original.treeTotalPointsSpentRequirement;
            obj.isCancelable = original.isCancelable;
            obj.isEffectOverTime = original.isEffectOverTime;
            obj.windupDuration = original.windupDuration;
            obj.range = original.range;
            obj.speed = original.speed;
            obj.cost = original.cost;
            obj.customObject = original.customObject;

            foreach (var item in original.stats)
            {
                obj.stats.Add(new Stat(item.Identity, item.initialValue, item.currentValue, item.scaling, item.maxLevel));
            }

#if UNITY_EDITOR
            var fullPath = Path.Combine(path, $"{obj.key}.asset");
            UnityEditor.AssetDatabase.CreateAsset(obj, fullPath);
#endif

            return Resources.Load<SkillNode>($"SkillTree/SkillNodes/{newGraph.id}/{obj.name}");
        }

        /// <summary>
        /// Gets the full path of a skill node. Works in the editor only.
        /// </summary>
        /// <param name="skillNode">The skill node.</param>
        /// <returns>The full path to the skill node.</returns>
        public static string GetFullPath(SkillNode skillNode)
        {
            var path = Path.Combine(directoryPath, skillNode.graphReference.id.ToString());
            return Path.Combine(path, $"{skillNode.name}.asset");
        }

        /// <summary>
        /// Supported skill node shape types.
        /// </summary>
        public enum Shape
        {
            Square,
            Squircle,
            Circle
        }

        /// <summary>
        /// Skill node states.
        /// </summary>
        public enum State
        {
            Locked,
            Unlocked,
            Obtained,
            Maxed
        }
    }
}