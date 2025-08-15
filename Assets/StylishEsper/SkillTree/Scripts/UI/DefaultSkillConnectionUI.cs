//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Default skill connection visual for the Skill Tree window.
//***************************************************************************************

using UnityEngine;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    public class DefaultSkillConnectionUI : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private Sprite completeSprite;

        [SerializeField]
        private Sprite disabledSprite;

        [SerializeField]
        private Sprite oneWayIncompleteSprite;

        [SerializeField]
        private Sprite twoWayIncompleteSprite;

        [SerializeField]
        private GameObject oneWayArrow;

        [SerializeField]
        private GameObject twoWayArrow;

        /// <summary>
        /// The skill connection.
        /// </summary>
        [HideInInspector]
        public SkillConnection connection { get; private set; }

        private DefaultSkillNodeSlot skillA;
        private DefaultSkillNodeSlot skillB;

        /// <summary>
        /// Sets this connection. This will set the position, size, and rotation of the connection
        /// between the skills.
        /// </summary>
        /// <param name="connection">The connection reference.</param>
        public void SetConnection(SkillConnection connection)
        {
            this.connection = connection;

            skillA = DefaultSkillTreeWindow.instance.GetSkillSlot(connection.skillNodeA.positionIndex);
            skillB = DefaultSkillTreeWindow.instance.GetSkillSlot(connection.skillNodeB.positionIndex);

            var rectTransformA = skillA.GetComponent<RectTransform>();
            var rectTransformB = skillB.GetComponent<RectTransform>();

            float width = Vector2.Distance(rectTransformA.anchoredPosition, rectTransformB.anchoredPosition);
            float height = skillA.contentRectTransform.rect.height * 0.1f;

            image.rectTransform.pivot = new Vector2(0, 0.5f);
            image.rectTransform.sizeDelta = new Vector2(width, height);
            image.rectTransform.position = skillA.contentRectTransform.position;

            Vector2 dir = rectTransformA.anchoredPosition -  rectTransformB.anchoredPosition;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            image.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 180));

            Refresh();
        }

        /// <summary>
        /// Refreshes this connection. This will show the correct sprite based on the skill node
        /// connection states.
        /// </summary>
        public void Refresh()
        {
            if (connection.twoWay)
            {
                oneWayArrow.SetActive(false);
                twoWayArrow.SetActive(true);

                if (skillA.skill.state >= SkillNode.State.Obtained && skillB.skill.state >= SkillNode.State.Obtained)
                {
                    image.sprite = completeSprite;
                }
                else if (skillA.skill.state < SkillNode.State.Obtained && skillB.skill.state < SkillNode.State.Obtained)
                {
                    image.sprite = disabledSprite;
                }
                else
                {
                    image.sprite = twoWayIncompleteSprite;
                }
            }
            else
            {
                oneWayArrow.SetActive(true);
                twoWayArrow.SetActive(false);

                if (skillA.skill.state >= SkillNode.State.Obtained && skillB.skill.state >= SkillNode.State.Obtained)
                {
                    image.sprite = completeSprite;
                }
                else if (skillA.skill.state < SkillNode.State.Obtained)
                {
                    image.sprite = disabledSprite;
                }
                else
                {
                    image.sprite = oneWayIncompleteSprite;
                }
            }
        }
    }
}