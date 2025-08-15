//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Sorcerer's Dark Born skill (Endless Smash).
//***************************************************************************************

using Esper.SkillTree.Stats;
using UnityEngine;

namespace Esper.SkillTree.Examples.Skills
{
    public class DarkBorn : ExampleSkill
    {
        [SerializeField]
        private ParticleSystem particle;

        private Transform follow;

        private AudioSource audioSource;

        private bool isComplete;

        private void FixedUpdate()
        {
            // Follow the transform
            transform.position = follow.position;
        }

        /// <summary>
        /// Apples the skill to the user.
        /// </summary>
        /// <param name="follow">A transform to follow.</param>
        public void ApplySkill(Transform follow)
        {
            this.follow = follow;

            audioSource = GetComponent<AudioSource>();
            audioSource.clip = effectSound;
            audioSource.Play();

            // End skill when the duration is complete
            Invoke(nameof(EndSkill), usedSkill.skill.duration);
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
            particle.Stop();
            Destroy(gameObject, 0.5f);
        }
    }
}