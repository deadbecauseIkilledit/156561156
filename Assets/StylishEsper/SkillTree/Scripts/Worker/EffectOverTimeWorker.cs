//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: A worker for skill EOT.
//***************************************************************************************

using System;
using UnityEngine;

namespace Esper.SkillTree.Worker
{
    public class EffectOverTimeWorker : SkillTreeTimeWorker
    {
        /// <summary>
        /// The skill node.
        /// </summary>
        protected SkillNode skill;

        /// <summary>
        /// An action to execute whenever a second passes.
        /// </summary>
        protected Action<SkillNode> onSecondPassed;

        /// <summary>
        /// The max number of times to repeat the effect.
        /// </summary>
        protected int repeatAmount;

        /// <summary>
        /// The number of times currently repeated.
        /// </summary>
        protected int repeatCount;

        protected override void Update()
        {
            remainingTime -= Time.deltaTime;
            onTimeUpdated.Invoke(remainingTime);

            if (remainingTime <= 0)
            {
                repeatCount++;
                onSecondPassed.Invoke(skill);
                CheckForCompletion();
            }
        }

        protected override void CheckForCompletion()
        {
            if (repeatCount >= repeatAmount)
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Creates and starts a time worker for skill EOT (effect over time).
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="onSecondPassed">An action to perform whenever a second is passed.</param>
        /// <param name="onTimeUpdated">An action to perform whenever time is updated. Default: null.</param>
        /// <param name="onComplete">An action to perform when this worker is complete. Default: null.</param>
        /// <param name="instanceName">The name of the instance. Default: "EOTWorker".</param>
        /// <returns>The created instance or null if it was not necessary.</returns>
        public static EffectOverTimeWorker CreateInstance(SkillNode skill, Action<SkillNode> onSecondPassed, Action<float> onTimeUpdated = null, Action onComplete = null, string instanceName = "EOTWorker")
        {
            // Length is 0 or nothing to do, no need for the worker
            if (!skill.isEffectOverTime || onSecondPassed == null)
            {
                return null;
            }
            
            int repeatAmount = Mathf.FloorToInt(skill.effectDuration);

            // Nothing to do, no need for the worker
            if (skill.isEffectOverTime && (skill.effectDuration <= 0 || repeatAmount == 0))
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return null;
            }

            var instance = new GameObject(instanceName, typeof(EffectOverTimeWorker)).GetComponent<EffectOverTimeWorker>();
            instance.onTimeUpdated = onTimeUpdated;
            instance.onSecondPassed = onSecondPassed;
            instance.remainingTime = 1f;
            instance.repeatAmount = repeatAmount;
            instance.skill = skill;
            instance.onComplete = onComplete;

            return instance;
        }
    }
}