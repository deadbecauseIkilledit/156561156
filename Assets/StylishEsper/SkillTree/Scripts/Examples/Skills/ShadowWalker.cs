//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;

namespace Esper.SkillTree.Examples.Skills
{
    /// <summary>
    /// Sorcerer's Shadow Walker skill (Endless Smash).
    /// </summary>
    public class ShadowWalker : ExampleSkill
    {
        private SpriteRenderer spriteRenderer;

        private Transform follow;

        private void FixedUpdate()
        {
            // Follow the transform
            transform.position = follow.position;
        }

        /// <summary>
        /// Apples the skill to the user.
        /// </summary>
        /// <param name="spriteRenderer">The sprite renderer of the user.</param>
        /// <param name="follow">A transform to follow.</param>
        public void ApplySkill(SpriteRenderer spriteRenderer, Transform follow)
        {
            // Heal
            var healAmount = usedSkill.skill.GetStat("HP");
            var userHP = usedSkill.user.GetStat("HP");
            userHP += healAmount.BaseValue;

            // Play sound
            AudioSource.PlayClipAtPoint(effectSound, follow.position);

            // Show heal amount
            ExampleFloatingWorldText.CreateInstance(healAmount.BaseValue.ToString(), follow.position, Color.green);

            // Make invincible
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);
            this.spriteRenderer = spriteRenderer;
            ExampleGameManager.instance.playerController.invincible = true;

            this.follow = follow;

            // End skill when the duration is complete
            Invoke(nameof(EndSkill), usedSkill.skill.duration);
        }

        /// <summary>
        /// Ends the skill.
        /// </summary>
        public void EndSkill()
        {
            ExampleGameManager.instance.playerController.invincible = false;
            spriteRenderer.color = Color.white;
            Destroy(gameObject);
        }
    }
}