//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Sorcerer's death bolt skill (Endless Smash).
//***************************************************************************************

using Esper.SkillTree.Stats;
using UnityEngine;

namespace Esper.SkillTree.Examples.Skills
{
    public class DeathBoltSkill : ExampleSkill
    {
        private Vector2 direction;

        private void Start()
        {
            AudioSource.PlayClipAtPoint(effectSound, transform.position);
            Destroy(gameObject, usedSkill.skill.duration);
        }

        private void FixedUpdate()
        {
            // Move in direction
            transform.Translate(direction.normalized * usedSkill.skill.speed * Time.fixedDeltaTime, Space.World);
        }

        /// <summary>
        /// Sets the direction.
        /// </summary>
        /// <param name="newDirection">The new direction.</param>
        public void SetDirection(Vector2 newDirection)
        {
            direction = newDirection;
        }

        public override void OnHit(UnitStatsProvider statsProvider, Vector3 hitPoint)
        {
            ApplyBasicDamage(statsProvider, hitPoint);
        }
    }
}