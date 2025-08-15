//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: TMP text content size fitter.
//***************************************************************************************

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    public class AutoSizeTextMeshProUGUI : MonoBehaviour
    {
        [SerializeField]
        private FitterDirection direction;

        private TextMeshProUGUI textMesh;

        private List<RectTransform> rectTransforms = new();

        public UnityEvent onTextChanged { get; private set; } = new();

        private void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            var layouts = GetComponentsInParent<LayoutGroup>();

            foreach (var layout in layouts)
            {
                rectTransforms.Add(layout.GetComponent<RectTransform>());
            }
        }

        private void Start()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            OnTextChanged(null);
        }

        private void OnDestroy()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void OnApplicationQuit()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        /// <summary>
        /// Starts the resize coroutine.
        /// </summary>
        /// <param name="obj">The object that was changed.</param>
        private void OnTextChanged(Object obj)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            StartCoroutine(LateResize());
        }

        private IEnumerator LateResize()
        {
            yield return new WaitForFixedUpdate();

            var size = textMesh.GetPreferredValues();

            switch (direction)
            {
                case FitterDirection.Vertical:
                    textMesh.rectTransform.sizeDelta = new Vector2(textMesh.rectTransform.sizeDelta.x, size.y);
                    break;

                case FitterDirection.Horizontal:
                    textMesh.rectTransform.sizeDelta = new Vector2(size.x, textMesh.rectTransform.sizeDelta.y);
                    break;
            }

            foreach (var item in rectTransforms)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            }

            onTextChanged.Invoke();
        }

        private enum FitterDirection
        {
            Vertical,
            Horizontal
        }
    }
}
