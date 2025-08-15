//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Base class that helps with time-related functionality.
//***************************************************************************************

using System;
using UnityEngine;

namespace Esper.SkillTree.Worker
{
    public class SkillTreeTimeWorker : MonoBehaviour
    {
        /// <summary>
        /// The remaining time.
        /// </summary>
        protected float remainingTime;

        /// <summary>
        /// The action to execute each update.
        /// </summary>
        public Action<float> onTimeUpdated;

        /// <summary>
        /// The action to execute on complete.
        /// </summary>
        public Action onComplete;

        protected virtual void Update()
        {
            remainingTime -= Time.deltaTime;

            if (onTimeUpdated != null)
            {
                onTimeUpdated.Invoke(remainingTime);
            }

            CheckForCompletion();
        }

        /// <summary>
        /// Checks if this worker is complete. If so, invoke the on complete action and destroy the object.
        /// </summary>
        protected virtual void CheckForCompletion()
        {
            if (remainingTime <= 0)
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Creates and starts a time worker.
        /// </summary>
        /// <param name="totalLength">The total length of time.</param>
        /// <param name="onTimeUpdated">An action to perform whenever time is updated. Default: null.</param>
        /// <param name="onComplete">An action to perform when this worker is complete. Default: null.</param>
        /// <param name="instanceName">The name of the instance. Default: "SkillTreeTimeWorker".</param>
        /// <returns>The created instance or null if it was not necessary.</returns>
        public static SkillTreeTimeWorker CreateInstance(float totalLength, Action<float> onTimeUpdated = null, Action onComplete = null, string instanceName = "SkillTreeTimeWorker")
        {
            // Length is 0 or nothing to do, no need for the worker
            if (totalLength <= 0)
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return null;
            }

            var instance = new GameObject(instanceName, typeof(SkillTreeTimeWorker)).GetComponent<SkillTreeTimeWorker>();
            instance.remainingTime = totalLength;
            instance.onTimeUpdated = onTimeUpdated;
            instance.onComplete = onComplete;
            return instance;
        }
    }
}