//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Endless Smash input handler.
//***************************************************************************************

using Esper.SkillTree.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Esper.SkillTree.Examples
{
    public class ExampleInputHandler : MonoBehaviour
    {
        public Vector2 move;
        public bool jump;
        public bool attack;
        public bool run;

#if !ENABLE_INPUT_SYSTEM
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DefaultSkillTreeWindow.instance?.ToggleActive();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                DefaultSkillBar.instance?.UseSkill(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                DefaultSkillBar.instance?.UseSkill(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                DefaultSkillBar.instance?.UseSkill(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                DefaultSkillBar.instance?.UseSkill(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                DefaultSkillBar.instance?.UseSkill(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                DefaultSkillBar.instance?.UseSkill(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                DefaultSkillBar.instance?.UseSkill(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                DefaultSkillBar.instance?.UseSkill(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                DefaultSkillBar.instance?.UseSkill(8);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                move = new Vector2(move.x, 1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                move = new Vector2(move.x, -1);
            }
            else
            {
                move = new Vector2(move.x, 0);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                move = new Vector2(-1, move.y);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                move = new Vector2(1, move.y);
            }
            else
            {
                move = new Vector2(0, move.y);
            }

            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
            {
                jump = true;
            }

            if (Input.GetKeyDown(KeyCode.X) || Input.GetMouseButtonDown(0))
            {
                attack = true;
            }

            run = Input.GetKey(KeyCode.LeftShift);
        }
#else
        private void OnToggleMenu()
        {
            DefaultSkillTreeWindow.instance?.ToggleActive();
        }

        private void OnSkillSlot1()
        {
            DefaultSkillBar.instance?.UseSkill(0);
        }

        private void OnSkillSlot2()
        {
            DefaultSkillBar.instance?.UseSkill(1);
        }

        private void OnSkillSlot3()
        {
            DefaultSkillBar.instance?.UseSkill(2);
        }

        private void OnSkillSlot4()
        {
            DefaultSkillBar.instance?.UseSkill(3);
        }

        private void OnSkillSlot5()
        {
            DefaultSkillBar.instance?.UseSkill(4);
        }

        private void OnSkillSlot6()
        {
            DefaultSkillBar.instance?.UseSkill(5);
        }

        private void OnSkillSlot7()
        {
            DefaultSkillBar.instance?.UseSkill(6);
        }

        private void OnSkillSlot8()
        {
            DefaultSkillBar.instance?.UseSkill(7);
        }

        private void OnSkillSlot9()
        {
            DefaultSkillBar.instance?.UseSkill(8);
        }

        private void OnMove(InputValue value)
        {
            move = value.Get<Vector2>();
        }

        private void OnJump()
        {
            jump = true;
        }

        private void OnAttack()
        {
            attack = true;
        }

        private void OnRun(InputValue value)
        {
            run = value.isPressed;
        }
#endif
    }
}