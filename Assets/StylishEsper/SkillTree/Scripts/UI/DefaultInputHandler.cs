//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Skill Tree's default UI input handler.
//***************************************************************************************

using UnityEngine;

namespace Esper.SkillTree.UI
{
    public class DefaultInputHandler : MonoBehaviour
    {
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
#endif
    }
}