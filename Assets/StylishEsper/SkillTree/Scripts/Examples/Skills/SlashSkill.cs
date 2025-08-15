//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Dark Demon's basic attack (Endless Smash).
//***************************************************************************************

using Esper.SkillTree.Stats;
using UnityEngine;

namespace Esper.SkillTree.Examples.Skills
{
    public class SlashSkill : ExampleSkill
    {
        private void Start()
        {
            AudioSource.PlayClipAtPoint(effectSound, transform.position);
            Destroy(gameObject, usedSkill.skill.duration);
        }

        public override void OnHit(UnitStatsProvider statsProvider, Vector3 hitPoint)
        {
            ApplyBasicDamage(statsProvider, hitPoint);
        }
    }
}