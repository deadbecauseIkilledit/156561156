//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Items displayed in the default skill menu.
//***************************************************************************************

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    public class DefaultSkillMenuItem : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TextMeshProUGUI optionTextMesh;

        /// <summary>
        /// Sets the item.
        /// </summary>
        /// <param name="text">The item text.</param>
        /// <param name="onClick">The action to perform on-click.</param>
        public void SetItem(string text, Action onClick)
        {
            optionTextMesh.text = text;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick.Invoke);
        }
    }
}