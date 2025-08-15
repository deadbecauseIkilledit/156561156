//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Esper.SkillTree.Settings;
using Esper.SkillTree.Stats;

namespace Esper.SkillTree
{
    /// <summary>
    /// The main class for scripting with Skill Tree (runtime only).
    /// </summary>
    public static class SkillTree
    {
        private const string settingsPath = "SkillTree/Settings/SkillTreeSettings";

        /// <summary>
        /// A list of enabled skill graphs.
        /// </summary>
        public static List<SkillGraph> skillGraphs = new();

        /// <summary>
        /// Called when any change has been made to a skill graph.
        /// </summary>
        public static UnityEvent<SkillGraph> onGraphChanged { get; private set; } = new();

        /// <summary>
        /// Called when any skill state is changed.
        /// </summary>
        public static UnityEvent<SkillNode> onSkillStateChanged { get; private set; } = new();

        /// <summary>
        /// Called when any skill is upgraded.
        /// </summary>
        public static UnityEvent<SkillNode> onSkillUpgraded { get; private set; } = new();

        /// <summary>
        /// Called when any skill is downgraded.
        /// </summary>
        public static UnityEvent<SkillNode> onSkillDowngraded { get; private set; } = new();

        /// <summary>
        /// Called when any skill is depleted.
        /// </summary>
        public static UnityEvent<SkillNode> onSkillDepleted { get; private set; } = new();

        /// <summary>
        /// Called when any skill is used.
        /// </summary>
        public static UnityEvent<UsedSkill> onSkillUsed { get; private set; } = new();

        /// <summary>
        /// Called when any used skill is completed.
        /// </summary>
        public static UnityEvent<UsedSkill> onSkillUseCompleted { get; private set; } = new();

        /// <summary>
        /// A variable Skill Tree will use to keep track of the player's level. You may be looking for
        /// SkillTree.SetPlayerLevel().
        /// </summary>
        public static int playerLevel { get; private set; }

        /// <summary>
        /// The current number of skill points that the player has.
        /// </summary>
        public static int playerSkillPoints { get; set; }

        /// <summary>
        /// A list of skills that were used and are currently in use.
        /// </summary>
        public static List<UsedSkill> usedSkills { get; private set; } = new();

        /// <summary>
        /// The player character's stats provider. This must be set manually.
        /// </summary>
        public static UnitStatsProvider playerStats { get; set; }

        private static SkillTreeSettings settings;

        /// <summary>
        /// Gets the Skill Tree settings.
        /// 
        /// This should be in a folder called Resources.
        /// </summary>
        public static SkillTreeSettings Settings
        {
            get
            {
                if (!settings)
                {
                    settings = Resources.Load<SkillTreeSettings>(settingsPath);
                }

                return settings;
            }
        }

        /// <summary>
        /// Initializes Skill Tree. This will load all enabled skill graphs. This should be called before
        /// working with Skill Tree.
        /// </summary>
        public static void Initialize()
        {
            var graphObjects = Resources.LoadAll<SkillGraph>("SkillTree/SkillGraphs");

            foreach (var graph in graphObjects)
            {
                if (graph.enabled)
                {
                    skillGraphs.Add(graph);

                    foreach (var item in graph.nodes)
                    {
                        item.ResetStats();
                    }
                }
            }
        }

        /// <summary>
        /// Clears all data stored in this class (events included). Using this will require Skill Tree
        /// to be initialized again.
        /// </summary>
        public static void ClearData()
        {
            foreach (var graph in skillGraphs)
            {
                graph.ResetNodeStates();
            }

            playerStats = null;
            skillGraphs.Clear();
            playerLevel = 0;
            playerSkillPoints = 0;
            onGraphChanged.RemoveAllListeners();
            onSkillStateChanged.RemoveAllListeners();
            onSkillUpgraded.RemoveAllListeners();
            onSkillDowngraded.RemoveAllListeners();
            onSkillUsed.RemoveAllListeners();
            onSkillUseCompleted.RemoveAllListeners();
            onSkillDepleted.RemoveAllListeners();
            usedSkills.Clear();
        }

        /// <summary>
        /// Gets a stat group.
        /// </summary>
        /// <param name="id">The ID of the stat group.</param>
        /// <returns>The stat group or null if one with the ID doesn't exist.</returns>
        public static StatGroup GetStatGroup(int id)
        {
            var groups = Resources.LoadAll<StatGroup>("SkillTree/Stats/Groups");

            foreach (var group in groups)
            {
                if (group.id == id)
                {
                    return group;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a stat group.
        /// </summary>
        /// <param name="name">The name of the stat group.</param>
        /// <returns>The stat group or null if one with the name doesn't exist.</returns>
        public static StatGroup GetStatGroup(string name)
        {
            var groups = Resources.LoadAll<StatGroup>("SkillTree/Stats/Groups");

            foreach (var group in groups)
            {
                if (group.displayName == name)
                {
                    return group;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all stat groups.
        /// </summary>
        /// <returns>A list of all stat groups.</returns>
        public static StatGroup[] GetAllStatGroups()
        {
            var groups = Resources.LoadAll<StatGroup>("SkillTree/Stats/Groups");
            return groups;
        }

        /// <summary>
        /// Gets a stat identity.
        /// </summary>
        /// <param name="id">The ID of the stat identity.</param>
        /// <returns>The stat identity or null if one with the ID doesn't exist.</returns>
        public static StatIdentity GetStatIdentity(int id)
        {
            var identities = Resources.LoadAll<StatIdentity>("SkillTree/Stats/Identities");

            foreach (var identity in identities)
            {
                if (identity.id == id)
                {
                    return identity;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a stat identity.
        /// </summary>
        /// <param name="name">The name of the stat identity.</param>
        /// <returns>The stat identity or null if one with the name doesn't exist.</returns>
        public static StatIdentity GetStatIdentity(string name)
        {
            var identities = Resources.LoadAll<StatIdentity>("SkillTree/Stats/Identities");

            foreach (var identity in identities)
            {
                if (identity.displayName == name)
                {
                    return identity;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all stat identities.
        /// </summary>
        /// <returns>A list of all stat identities.</returns>
        public static StatIdentity[] GetAllStatIdentities()
        {
            var identities = Resources.LoadAll<StatIdentity>("SkillTree/Stats/Identities");
            return identities;
        }

        /// <summary>
        /// Updates the player's level for Skill Tree. This will unlock any locked nodes for all graphs
        /// that are possible to unlock. This should be called once at the start of the game and once 
        /// every time the player levels up.
        /// </summary>
        /// <param name="playerLevel">The player's level.</param>
        public static void SetPlayerLevel(int playerLevel)
        {
            SkillTree.playerLevel = playerLevel;

            foreach (var graph in skillGraphs)
            {
                graph.TryUnlockLockedNodes();
            }
        }

        /// <summary>
        /// Sets data for all skill graphs (or skill trees).
        /// </summary>
        /// <param name="graphs">A list of all skill graphs.</param>
        public static void SetSkillGraphs(List<SkillGraph> graphs)
        {
            for (int i = 0; i < graphs.Count; i++)
            {
                SetSkillGraph(graphs[i]);
            }
        }

        /// <summary>
        /// Sets a single skill graph. If a graph didn't match the ID of the provided graph, nothing will happen.
        /// </summary>
        /// <param name="graph">The graph data to set.</param>
        public static void SetSkillGraph(SkillGraph graph)
        {
            for (int i = 0; i < skillGraphs.Count; i++)
            {
                if (skillGraphs[i].id == graph.id)
                {
                    skillGraphs[i].CopyData(graph);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets data for all skill graphs (or skill trees).
        /// </summary>
        /// <param name="graphs">A list of all skill graphs.</param>
        public static void SetSkillGraphs(List<SavableSkillGraph> graphs)
        {
            for (int i = 0; i < graphs.Count; i++)
            {
                SetSkillGraph(graphs[i]);
            }
        }

        /// <summary>
        /// Sets a single skill graph. If a graph didn't match the ID of the provided graph, nothing will happen.
        /// </summary>
        /// <param name="graph">A savable skill graph.</param>
        public static void SetSkillGraph(SavableSkillGraph graph)
        {
            for (int i = 0; i < skillGraphs.Count; i++)
            {
                if (skillGraphs[i].id == graph.id)
                {
                    skillGraphs[i].CopyData(graph);
                    break;
                }
            }
        }

        /// <summary>
        /// Gets a list of Skill Trees that are for the player and set as visible.
        /// </summary>
        /// <returns>A list of visible player skill trees.</returns>
        public static List<SkillGraph> GetVisiblePlayerSkillTrees()
        {
            List<SkillGraph> visible = new();

            foreach (var graph in skillGraphs)
            {
                if (graph.forPlayer && graph.visibility == SkillGraph.GraphVisibility.Visible)
                {
                    visible.Add(graph);
                }
            }

            return visible;
        }

        /// <summary>
        /// Gets the skill graph by name.
        /// </summary>
        /// <param name="name">The name of the skill graph.</param>
        /// <returns>The skill graph or null if one with the name doesn't exist.</returns>
        public static SkillGraph GetSkillGraph(string name)
        {
            foreach (var graph in skillGraphs)
            {
                if (graph.displayName == name)
                {
                    return graph;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the skill graph by ID.
        /// </summary>
        /// <param name="id">The ID of the skill graph.</param>
        /// <returns>The skill graph or null if one with the ID doesn't exist.</returns>
        public static SkillGraph GetSkillGraph(int id)
        {
            foreach (var graph in skillGraphs)
            {
                if (graph.id == id)
                {
                    return graph;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches through all graphs to find a specific skill.
        /// </summary>
        /// <param name="key">The key of the skill node.</param>
        /// <returns>A skill node with a matching key or null if one doesn't exist.</returns>
        public static SkillNode GetSkill(string key)
        {
            foreach (var graph in skillGraphs)
            {
                var node = graph.GetNodeByKey(key);

                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a used skill that is currently in use.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="user">The user of the skill.</param>
        /// <returns>The used skill object or null if it doesn't exist.</returns>
        public static UsedSkill GetUsedSkill(SkillNode skill, UnitStatsProvider user)
        {
            foreach (var usedSkill in usedSkills)
            {
                if (usedSkill.skill == skill && usedSkill.user == user)
                {
                    return usedSkill;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a user is currently using a skill.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>True if the user is currently using a skill. Otherwise, false.</returns>
        public static bool IsUserSkillWindingUp(UnitStatsProvider user)
        {
            foreach (var usedSkill in usedSkills)
            {
                if (usedSkill.user == user && usedSkill.isWindingUp)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to use a skill. This does a very basic check to see if the skill can be used.
        /// </summary>
        /// <param name="skill">The skill to try to use.</param>
        /// <param name="user">The user of the skill.</param>
        /// <returns>A used skill object if successfully used. Otherwise, null.</returns>
        public static UsedSkill TryUseSkill(SkillNode skill, UnitStatsProvider user)
        {
            var usedSkill = GetUsedSkill(skill, user);
            if (skill == null || IsUserSkillWindingUp(user) || (usedSkill != null && usedSkill.onCooldown))
            {
                return null;
            }

            var newUsedSkill = new UsedSkill(skill, user);
            usedSkills.Add(newUsedSkill);

            onSkillUsed.Invoke(newUsedSkill);

            return newUsedSkill;
        }

        /// <summary>
        /// Tells Skill Tree that a usage of a skill is complete (not the skill itself).
        /// </summary>
        /// <param name="skill">The skill node.</param>
        /// <param name="user">The user of the skill.</param>
        public static void CompleteSkillUse(SkillNode skill, UnitStatsProvider user)
        {
            var usedSkill = GetUsedSkill(skill, user);

            if (usedSkill == null)
            {
                return;
            }

            // Do not remove from list until cooldown is complete
            usedSkill.OnCooldownComplete(() => usedSkills.Remove(usedSkill));
            onSkillUseCompleted.Invoke(usedSkill);
        }

        /// <summary>
        /// Tells Skill Tree that a usage of a skill is complete (not the skill itself).
        /// </summary>
        /// <param name="usedSkill">The used skill.</param>
        public static void CompleteSkillUse(UsedSkill usedSkill)
        {
            if (usedSkill == null)
            {
                return;
            }

            // Do not remove from list until cooldown is complete
            usedSkill.OnCooldownComplete(() => usedSkills.Remove(usedSkill));
            onSkillUseCompleted.Invoke(usedSkill);
        }
    }
}