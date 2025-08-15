//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Sorcerer's Darkness Falls skill (Endless Smash).
//***************************************************************************************

using Esper.SkillTree.Stats;
using UnityEngine;

namespace Esper.SkillTree.Examples.Skills
{
    public class DarknessFalls : ExampleSkill
    {
        [SerializeField]
        private ParticleSystem particle1;

        [SerializeField]
        private ParticleSystem particle2;

        [SerializeField]
        private CircleCollider2D circleCollider;

        private float range;

        private bool isComplete;

        private void Start()
        {
            AudioSource.PlayClipAtPoint(effectSound, transform.position);
        }

        private void FixedUpdate()
        {
            if (isComplete)
            {
                return;
            }

            // Range increase amount
            var increase = Time.fixedDeltaTime * usedSkill.skill.speed;
            range += increase;

            // Set particle shape
            var shape = particle1.shape;
            shape.radius = range;

            var shape2 = particle2.shape;
            shape2.radius = range;

            // Apply range
            circleCollider.radius = range;

            if (range >= usedSkill.skill.range)
            {
                EndSkill();
            }
        }

        public override void OnHit(UnitStatsProvider statsProvider, Vector3 hitPoint)
        {
            if (isComplete)
            {
                return;
            }

            ApplyBasicDamage(statsProvider, hitPoint);
        }

        /// <summary>
        /// Ends this skill.
        /// </summary>
        public void EndSkill()
        {
            isComplete = true;
            particle1.Stop();
            Destroy(gameObject, 0.5f);
        }
    }
}