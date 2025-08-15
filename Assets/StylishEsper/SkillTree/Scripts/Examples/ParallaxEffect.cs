//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Simple parallax effect.
//***************************************************************************************

using UnityEngine;

namespace Esper.SkillTree.Examples
{
    public class ParallaxEffect : MonoBehaviour
    {
        public GameObject cam;

        public float parallaxEffectMultiplier;

        private float length;

        private float startPos;

        private void Start()
        {
            startPos = transform.position.x;
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        private void FixedUpdate()
        {
            float temp = cam.transform.position.x * (1 - parallaxEffectMultiplier);
            float distance = cam.transform.position.x * parallaxEffectMultiplier;

            transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

            if (temp > startPos + length)
            {
                startPos += length;
            }
            else if (temp < startPos - length)
            {
                startPos -= length;
            }
        }
    }
}