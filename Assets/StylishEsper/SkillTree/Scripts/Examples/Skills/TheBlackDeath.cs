//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Sorcerer's The Black Death skill (Endless Smash).
//***************************************************************************************

using UnityEngine;

namespace Esper.SkillTree.Examples.Skills
{
    public class TheBlackDeath : ExampleSkill
    {
        [SerializeField]
        private ParticleSystem particle;

        private ExampleHitBox hitBox;

        private Vector2 direction;

        private void Start()
        {
            hitBox = GetComponent<ExampleHitBox>();

            AudioSource.PlayClipAtPoint(effectSound, transform.position);

            // Apply damage every second
            InvokeRepeating(nameof(ApplyDamage), 0, 1);

            // End skill
            Invoke(nameof(EndSkill), usedSkill.skill.effectDuration);
        }

        private void FixedUpdate()
        {
            // Move in direction
            transform.Translate(direction.normalized * usedSkill.skill.speed * Time.deltaTime, Space.World);
        }

        /// <summary>
        /// Applies damage to all targets in range.
        /// </summary>
        public void ApplyDamage()
        {
            foreach (var target in hitBox.hitList)
            {
                ApplyBasicDamage(target, (Vector2)target.transform.position + target.GetComponent<Collider2D>().offset);
            }
        }

        /// <summary>
        /// Sets the direction.
        /// </summary>
        /// <param name="newDirection">The new direction.</param>
        public void SetDirection(Vector2 newDirection)
        {
            direction = newDirection;
        }

        /// <summary>
        /// Ends this skill.
        /// </summary>
        public void EndSkill()
        {
            particle.Stop();
            hitBox.trackExitAndEnter = false;
            Destroy(gameObject, 0.5f);
        }
    }
}
