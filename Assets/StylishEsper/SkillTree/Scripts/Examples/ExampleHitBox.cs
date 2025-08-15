//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Used for all hit boxes for Endless Smash. This is for 2D only.
//***************************************************************************************

using Esper.SkillTree.Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Esper.SkillTree.Examples
{
    public class ExampleHitBox : MonoBehaviour
    {
        public bool trackExitAndEnter;

        public bool friendly;

        [HideInInspector]
        public List<UnitStatsProvider> hitList = new();

        public UnityEvent<UnitStatsProvider, Vector3> onHit = new();

        private void OnTriggerEnter2D(Collider2D collision)
        {
            OnEnter(collision.gameObject, collision.ClosestPoint(transform.position));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnEnter(collision.gameObject, collision.contacts[0].point);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            OnExit(collision.gameObject);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            OnExit(collision.gameObject);
        }

        /// <summary>
        /// Applies damage to either the player or enemy. Called on enter.
        /// </summary>
        /// <param name="gameObject">The hit GameObject.</param>
        /// <param name="hitPoint">The hit point.</param>
        private void OnEnter(GameObject gameObject, Vector3 hitPoint)
        {
            if (gameObject.TryGetComponent(out UnitStatsProvider target))
            {
                if (hitList.Contains(target) || (friendly && target == SkillTree.playerStats) || (!friendly && target != SkillTree.playerStats))
                {
                    return;
                }

                hitList.Add(target);
                onHit.Invoke(target, hitPoint);
            }
        }

        /// <summary>
        /// Removes a unit from the hit list if track exit and enter is true.
        /// </summary>
        /// <param name="gameObject">The exited GameObject.</param>
        private void OnExit(GameObject gameObject)
        {
            if (trackExitAndEnter && gameObject.TryGetComponent(out UnitStatsProvider target))
            {
                if (hitList.Contains(target))
                {
                    StartCoroutine(RemoveTargetCoroutine(target));
                }
            }
        }

        /// <summary>
        /// Removes a target at the end of a frame.
        /// </summary>
        /// <returns>Yields until the end of the frame.</returns>
        private IEnumerator RemoveTargetCoroutine(UnitStatsProvider target)
        {
            yield return new WaitForEndOfFrame();
            hitList.Remove(target);
        }
    }
}
