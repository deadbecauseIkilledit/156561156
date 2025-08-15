//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Automatically scrolls a ScrollRect.
//***************************************************************************************

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.SkillTree.UI
{
    public class ScrollRectAutoScroll : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private float scrollDelay = 1f;

        [SerializeField, Min(0.1f)]
        private float scrollSpeed = 1f;

        [SerializeField]
        private bool horizontal;

        private ScrollRect scrollRect;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        private void OnEnable()
        {
            if (!horizontal)
            {
                scrollRect.verticalNormalizedPosition = 1;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = 1;
            }

            StartCoroutine(AutoScrollCoroutine());
        }

        private IEnumerator AutoScrollCoroutine()
        {
            // Wait for scroll delay
            yield return new WaitForSeconds(scrollDelay);

            // Scroll down
            var waitForScroll = new WaitUntil(() =>
            {
                if (!horizontal)
                {
                    scrollRect.verticalNormalizedPosition -= Time.deltaTime * scrollSpeed;

                    if (scrollRect.verticalNormalizedPosition <= 0)
                    {
                        return true;
                    }
                }
                else
                {
                    scrollRect.horizontalNormalizedPosition -= Time.deltaTime * scrollSpeed;

                    if (scrollRect.horizontalNormalizedPosition <= 0)
                    {
                        return true;
                    }
                }

                return false;
            });

            yield return waitForScroll;

            // Wait for scroll delay
            yield return new WaitForSeconds(scrollDelay);

            // Scroll up
            waitForScroll = new WaitUntil(() =>
            {
                if (!horizontal)
                {
                    scrollRect.verticalNormalizedPosition += Time.deltaTime * scrollSpeed;

                    if (scrollRect.verticalNormalizedPosition >= 1)
                    {
                        return true;
                    }
                }
                else
                {
                    scrollRect.horizontalNormalizedPosition += Time.deltaTime * scrollSpeed;

                    if (scrollRect.horizontalNormalizedPosition >= 1)
                    {
                        return true;
                    }
                }

                return false;
            });

            yield return waitForScroll;

            // Repeat
            StartCoroutine(AutoScrollCoroutine());
        }
    }
}