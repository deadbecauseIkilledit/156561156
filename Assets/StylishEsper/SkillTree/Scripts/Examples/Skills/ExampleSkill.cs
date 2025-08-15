//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Used for Endless Smash to help create executable skills. This is the
// base skill class.
//***************************************************************************************

using Esper.SkillTree.Stats;
using UnityEngine;

namespace Esper.SkillTree.Examples.Skills
{
    public abstract class ExampleSkill : MonoBehaviour
    {
        protected UsedSkill usedSkill;

        protected float damage;

        [SerializeField]
        protected AudioClip hitSound;

        [SerializeField]
        protected AudioClip effectSound;

        /// <summary>
        /// What to do when the hit box hits a target.
        /// </summary>
        /// <param name="statsProvider">Stats provider object of the hit target.</param>
        /// <param name="hitPoint">The point of the hit.</param>
        public virtual void OnHit(UnitStatsProvider statsProvider, Vector3 hitPoint)
        {
            
        }

        /// <summary>
        /// Simply applies basic damage with no other functionality.
        /// </summary>
        /// <param name="statsProvider">Stats provider object of the hit target.</param>
        /// <param name="hitPoint">The point of the hit.</param>
        public void ApplyBasicDamage(UnitStatsProvider statsProvider, Vector3 hitPoint)
        {
            if (hitSound)
            {
                AudioSource.PlayClipAtPoint(hitSound, hitPoint);
            }

            if (statsProvider != SkillTree.playerStats || (statsProvider == SkillTree.playerStats && !ExampleGameManager.instance.playerController.invincible))
            {
                statsProvider.DecreaseStat("HP", damage);
                ExampleFloatingWorldText.CreateInstance(damage.ToString(), hitPoint, Color.red);
            }
        }

        /// <summary>
        /// Instantiates a skill instance of a skill prefab.
        /// </summary>
        /// <param name="usedSkill">The used skill. The skill node's key will be used to find the prefab.</param>
        /// <param name="startPos">The start position of the skill.</param>
        /// <returns>The skill instance.</returns>
        public static T InstantiateSkill<T>(UsedSkill usedSkill, Vector3 startPos = default) where T : ExampleSkill
        {
            var skill = Instantiate(Resources.Load<T>($"Skills/{usedSkill.skill.key}"));
            skill.usedSkill = usedSkill;
            skill.transform.position = startPos;

            var skillDmg = usedSkill.skill.GetStat("DMG");
            var userDmg = usedSkill.user.GetStat("DMG");
            skill.damage = (skillDmg.BaseValue + userDmg.currentValue).ToFloat();

            return skill;
        }
    }
}