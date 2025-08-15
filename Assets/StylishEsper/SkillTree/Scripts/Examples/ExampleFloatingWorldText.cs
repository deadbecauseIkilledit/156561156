//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Example floating world text used in combat.
//***************************************************************************************

using TMPro;
using UnityEngine;

namespace Esper.SkillTree.Examples
{
    public class ExampleFloatingWorldText : MonoBehaviour
    {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private TextMeshProUGUI textMesh;

        [SerializeField]
        private float length = 1;

        [SerializeField]
        private float speed = 1;

        private float timePassed;

        /// <summary>
        /// Sets the text and its position.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="startPosition">The world start position.</param>
        public void SetTextAndPosition(string text, Vector3 startPosition)
        {
            textMesh.text = text;
            transform.position = startPosition;
            timePassed = 0;

            if (!canvas.worldCamera)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        private void FixedUpdate()
        {
            transform.position += Vector3.up * Time.deltaTime * speed;
            transform.rotation = canvas.worldCamera.transform.rotation;
            timePassed += Time.deltaTime;
            textMesh.alpha = 1 - (timePassed / length);

            if (timePassed >= length)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Creates a new floating text instance.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="startPosition">The world start position.</param>
        public static void CreateInstance(string text, Vector3 startPosition, Color color)
        {
            var instance = Instantiate(Resources.Load<ExampleFloatingWorldText>("Prefabs/FloatingWorldText"));
            instance.textMesh.color = color;
            instance.SetTextAndPosition(text, startPosition);
        }
    }
}