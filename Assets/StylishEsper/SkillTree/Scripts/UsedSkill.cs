//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Contains data for a skill that was used and is currently in use.
//***************************************************************************************

using Esper.SkillTree.Stats;
using Esper.SkillTree.Worker;
using System;

namespace Esper.SkillTree
{
    public class UsedSkill
    {
        /// <summary>
        /// The used skill reference.
        /// </summary>
        public SkillNode skill;

        /// <summary>
        /// The skill user's stats reference.
        /// </summary>
        public UnitStatsProvider user;

        /// <summary>
        /// The cooldown worker of this skill. This may be null if the skill has no cooldown.
        /// </summary>
        public CooldownWorker cooldownWorker;

        /// <summary>
        /// The windup worker of this skill. This may be null if the skill has no windup time.
        /// </summary>
        public SkillTreeTimeWorker windupWorker;

        /// <summary>
        /// If the skill is currently winding up.
        /// </summary>
        public bool isWindingUp;

        /// <summary>
        /// If the skill is currently on cooldown.
        /// </summary>
        public bool onCooldown;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="skill">The used skill reference.</param>
        /// <param name="user">The skill user's stats reference.</param>
        public UsedSkill(SkillNode skill, UnitStatsProvider user)
        {
            this.skill = skill;
            this.user = user;

            if (skill.cooldown > 0)
            {
                cooldownWorker = CooldownWorker.CreateInstance(this);
                onCooldown = true;
            }

            if (skill.windupDuration > 0)
            {
                windupWorker = SkillTreeTimeWorker.CreateInstance(skill.windupDuration, null, () => isWindingUp = false);
                isWindingUp = true;
            }
        }

        /// <summary>
        /// Perform an action every time the cooldown is updated.
        /// </summary>
        /// <param name="action">The action.</param>
        public void OnCooldownUpdated(Action<float> action)
        {
            if (cooldownWorker == null)
            {
                action.Invoke(0);
                return;
            }

            cooldownWorker.onTimeUpdated += action;
        }

        /// <summary>
        /// Perform an action when cooldown is complete.
        /// </summary>
        /// <param name="action">The action.</param>
        public void OnCooldownComplete(Action action)
        {
            if (cooldownWorker == null)
            {
                action.Invoke();
                return;
            }

            cooldownWorker.onComplete += action;
        }

        /// <summary>
        /// Perform an action every time the windup time is updated.
        /// </summary>
        /// <param name="action">The action.</param>
        public void OnWindupUpdated(Action<float> action)
        {
            if (windupWorker == null)
            {
                return;
            }

            windupWorker.onTimeUpdated += action;
        }

        /// <summary>
        /// Perform an action when windup is complete.
        /// </summary>
        /// <param name="action">The action.</param>
        public void OnWindupComplete(Action action)
        {
            if (windupWorker == null)
            {
                action.Invoke();
                return;
            }

            windupWorker.onComplete += action;
        }

        /// <summary>
        /// Returns if another used skill object is equal.
        /// </summary>
        /// <param name="other">The other used skill.</param>
        /// <returns>True if the other object is equal to this object. Otherwise, false.</returns>
        public bool IsEqualTo(UsedSkill other)
        {
            return skill == other.skill && user == other.user;
        }
    }
}