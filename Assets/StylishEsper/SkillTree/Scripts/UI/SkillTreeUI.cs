//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Base skill tree UI class.
//***************************************************************************************

using UnityEngine;
using UnityEngine.Events;

namespace Esper.SkillTree.UI
{
    public abstract class SkillTreeUI : MonoBehaviour
    {
        [SerializeField]
        protected RectTransform content;

        /// <summary>
        /// If the UI object is open.
        /// </summary>
        public bool isOpen { get => content.gameObject.activeSelf; }

        /// <summary>
        /// Called when the UI object is opened.
        /// </summary>
        public UnityEvent onOpen { get; private set; } = new();

        /// <summary>
        /// Called when the UI object is closed.
        /// </summary>
        public UnityEvent onClose { get; private set; } = new();

        /// <summary>
        /// Called when the UI object is either closed or opened.
        /// </summary>
        public UnityEvent onActiveStateChanged { get; private set; } = new();

        /// <summary>
        /// Opens the window.
        /// </summary>
        public virtual void Open()
        {
            if (!isOpen)
            {
                content.gameObject.SetActive(true);
                onOpen.Invoke();
                onActiveStateChanged.Invoke();
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        public virtual void Close()
        {
            if (isOpen)
            {
                content.gameObject.SetActive(false);
                onClose.Invoke();
                onActiveStateChanged.Invoke();
            }
        }

        /// <summary>
        /// Toggles the active state of the window.
        /// </summary>
        public virtual void ToggleActive()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }
}