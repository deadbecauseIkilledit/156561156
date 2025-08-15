//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    /// <summary>
    /// Asks to confirm a specific action.
    /// </summary>
    public class ConfirmationPrompt : SkillTreeUI
    {
        [SerializeField]
        private TextMeshProUGUI titleTextMesh;

        [SerializeField]
        private TextMeshProUGUI descriptionTextMesh;

        [SerializeField]
        private TextMeshProUGUI confirmTextMesh;

        [SerializeField]
        private TextMeshProUGUI cancelTextMesh;

        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button cancelButton;

        /// <summary>
        /// The active ConfirmationPrompt instance. This may be null if no ConfirmationPrompt is in the scene.
        /// </summary>

        public static ConfirmationPrompt instance;

        private void Awake()
        {
            // Singleton
            if (instance)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        /// <summary>
        /// Sets and opens the prompt.
        /// </summary>
        /// <param name="title">The prompt title.</param>
        /// <param name="description">The prompt description.</param>
        /// <param name="onConfirm">The action to execute on confirm.</param>
        /// <param name="onCancel">The action to execute on cancel. Default: null.</param>
        /// <param name="confirmButtonText">The text of the confirm button. Default: "Confirm".</param>
        /// <param name="cancelButtonText">The text of the cancel button. Default: "Cancel".</param>
        public void SetPrompt(string title, string description, Action onConfirm, Action onCancel = null, string confirmButtonText = "Confirm", string cancelButtonText = "Cancel")
        {
            titleTextMesh.text = title;
            descriptionTextMesh.text = description;

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                if (onConfirm != null)
                {
                    onConfirm.Invoke();
                }

                Close();
            });

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() =>
            {
                if (onCancel != null)
                {
                    onCancel.Invoke();
                }

                Close();
            });

            confirmTextMesh.text = confirmButtonText;
            cancelTextMesh.text = cancelButtonText;

            Open();
        }
    }
}