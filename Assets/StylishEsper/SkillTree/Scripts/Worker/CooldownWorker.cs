//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: A worker for skill cooldowns.
//***************************************************************************************

using System;
using UnityEngine;

namespace Esper.SkillTree.Worker
{
    public class CooldownWorker : SkillTreeTimeWorker
    {
        /// <summary>
        /// The used skill reference.
        /// </summary>
        protected UsedSkill usedSkill;

        protected override void CheckForCompletion()
        {
            if (remainingTime <= 0)
            {
                usedSkill.onCooldown = false;

                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Creates and starts a time worker for skill cooldowns.
        /// </summary>
        /// <param name="usedSkill">The skill that was used.</param>
        /// <param name="onTimeUpdated">An action to perform whenever time is updated. Default: null.</param>
        /// <param name="onComplete">An action to perform when this worker is complete. Default: null.</param>
        /// <param name="instanceName">The name of the instance. Default: "CooldownWorker".</param>
        /// <returns>The created instance or null if it was not necessary.</returns>
        public static CooldownWorker CreateInstance(UsedSkill usedSkill, Action<float> onTimeUpdated = null, Action onComplete = null, string instanceName = "CooldownWorker")
        {
            // Length is 0 or nothing to do, no need for the worker
            if (usedSkill.skill.cooldown <= 0)
            {
                if (onTimeUpdated != null)
                {
                    onTimeUpdated.Invoke(0);
                }

                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return null;
            }

            var instance = new GameObject(instanceName, typeof(CooldownWorker)).GetComponent<CooldownWorker>();
            instance.onTimeUpdated = onTimeUpdated;
            instance.remainingTime = usedSkill.skill.cooldown;
            instance.usedSkill = usedSkill;
            instance.onComplete = onComplete;

            return instance;
        }
    }
}