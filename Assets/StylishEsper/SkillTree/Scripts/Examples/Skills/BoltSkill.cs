//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Sorcerer's basic attack (Endless Smash).
//***************************************************************************************

using Esper.SkillTree.Stats;
using UnityEngine;

namespace Esper.SkillTree.Examples.Skills
{
    public class BoltSkill : ExampleSkill
    {
        private Vector2 direction;

        private Vector2 startPos;

        private float traveledDistance;

        private void Start()
        {
            AudioSource.PlayClipAtPoint(effectSound, transform.position);
            startPos = transform.position;
            RotateTowardsDirection();
        }

        private void FixedUpdate()
        {
            // Move in direction
            transform.Translate(direction.normalized * usedSkill.skill.speed * Time.fixedDeltaTime, Space.World);

            // Calculate distance traveled
            traveledDistance = Vector2.Distance(startPos, transform.position);

            // If range reached, destroy the object (making this skill is range-limited)
            if (traveledDistance >= usedSkill.skill.range)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Sets the direction.
        /// </summary>
        /// <param name="newDirection">The new direction.</param>
        public void SetDirection(Vector2 newDirection)
        {
            direction = newDirection;
            RotateTowardsDirection();
        }

        /// <summary>
        /// Calculates and rotates the object towards the movement direction.
        /// </summary>
        private void RotateTowardsDirection()
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 270);
        }

        public override void OnHit(UnitStatsProvider statsProvider, Vector3 hitPoint)
        {
            ApplyBasicDamage(statsProvider, hitPoint);
        }
    }
}