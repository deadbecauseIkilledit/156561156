//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: A middleman to help run your own methods when a skill is used with Skill
// Tree.
//***************************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Esper.SkillTree
{
    public class SkillInterpreter : MonoBehaviour
    {
        /// <summary>
        /// A list of interpreters.
        /// </summary>
        public List<SkillInterpretation> interpreters = new();

        private void Start()
        {
            // Register all interpreters
            foreach (var interpreter in interpreters)
            {
                RegisterInterpreter(interpreter);
            }
        }

        /// <summary>
        /// Registers an interpreter.
        /// </summary>
        /// <param name="interpreter">The interpreter.</param>
        public void RegisterInterpreter(SkillInterpretation interpreter)
        {
            SkillTree.onSkillUsed.AddListener((x) =>
            {
                if (x != null && x.skill.key == interpreter.skillKey && (!x.user || (x.user && x.user.gameObject == gameObject)))
                {
                    interpreter.onSkillUsed.Invoke(x);
                    x.OnWindupComplete(() => interpreter.onWindupComplete.Invoke(x));
                }
            });
        }

        /// <summary>
        /// Used skill interpreter data.
        /// </summary>
        [Serializable]
        public struct SkillInterpretation
        {
            /// <summary>
            /// The skill key.
            /// </summary>
            public string skillKey;

            /// <summary>
            /// Called when a skill is used.
            /// </summary>
            public UnityEvent<UsedSkill> onSkillUsed;

            /// <summary>
            /// Called when a used skill's windup is complete.
            /// </summary>
            public UnityEvent<UsedSkill> onWindupComplete;
        }
    }
}