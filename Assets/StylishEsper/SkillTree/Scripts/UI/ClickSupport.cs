//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Provides object mouse click support.
//***************************************************************************************

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Esper.SkillTree.UI
{
    public class ClickSupport : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Callback for when the mouse left click button is performed on the object.
        /// </summary>
        public UnityEvent onLeftClick;

        /// <summary>
        /// Callback for when the mouse middle click button is performed on the object.
        /// </summary>
        public UnityEvent onMiddleClick;

        /// <summary>
        /// Callback for when the mouse right click button is performed on the object.
        /// </summary>
        public UnityEvent onRightClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                onLeftClick.Invoke();
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
                onMiddleClick.Invoke();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                onRightClick.Invoke();
            }
        }
    }
}