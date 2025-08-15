//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Stores skill node connection data.
//***************************************************************************************

#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace Esper.SkillTree.Editor
{
    public class SkillConnectionElement : Button
    {
        public SkillGraph skillGraphReference;

        public SkillConnection connection;

        public SkillNodeElement skillA;

        public SkillNodeElement skillB;

        private VisualElement element;

        private VisualElement connectionContent;

        public SkillConnectionElement(SkillGraph skillGraphReference, SkillConnection connection, SkillNodeElement skillA, SkillNodeElement skillB, Vector2 contentSize, VisualElement connectionContent)
        {
            this.skillGraphReference = skillGraphReference;
            this.connection = connection;
            this.skillA = skillA;
            this.skillB = skillB;
            this.connectionContent = connectionContent;

            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.marginTop = 0;
            style.paddingBottom = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            style.paddingTop = 0;

            element = new VisualElement();
            element.style.position = Position.Absolute;
            connectionContent.Add(element);
            element.Add(this);

            Reload(contentSize);
        }

        public void Reload(Vector2 contentSize)
        {
            if (connection.twoWay)
            {
                style.backgroundImage = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StylishEsper/SkillTree/Editor/Icons/connection_twoway_icon.png"));
            }
            else
            {
                style.backgroundImage = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StylishEsper/SkillTree/Editor/Icons/connection_a_to_b_icon.png"));
            }

            element.style.width = skillA.style.width;
            element.style.height = skillA.style.height;
            element.style.top = skillA.style.top;
            element.style.left = skillA.style.left;

            var v2A = new Vector2(skillA.style.left.value.value + skillA.style.width.value.value / 2, skillA.style.top.value.value + skillA.style.height.value.value / 2);
            var v2B = new Vector2(skillB.style.left.value.value + skillB.style.width.value.value / 2, skillB.style.top.value.value + skillB.style.height.value.value / 2); ;

            float width = Vector2.Distance(v2A, v2B);
            float height = 16f / (1920f / contentSize.x);
            style.width = width;
            style.height = height;

            style.left = skillA.style.width.value.value / 2f + (height / 2f);
            style.top = skillA.style.height.value.value / 2f - (height / 2f);

            Vector2 dir = v2A - v2B;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            element.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 180));
        }
    }
}
#endif