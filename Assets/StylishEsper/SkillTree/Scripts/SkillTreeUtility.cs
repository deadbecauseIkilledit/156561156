//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.SkillTree.Stats;
using Esper.SkillTree.UI;
using System;
using UnityEngine;

namespace Esper.SkillTree
{
    /// <summary>
    /// Skill tree utility functions.
    /// </summary>
    public static class SkillTreeUtility
    {
        private static Vector3[] objectCorners = new Vector3[4];

        /// <summary>
        /// Generates a random GUID string.
        /// </summary>
        /// <param name="maxTokenLength">The max length. Default: 24 (cannot be longer than 24).</param>
        /// <returns>A randomly generated string.</returns>
        public static string GenerateRandomGUID(int maxTokenLength = 24)
        {
            Guid guid = Guid.NewGuid();
            string guidString = Convert.ToBase64String(guid.ToByteArray());
            guidString = guidString.Replace("=", "");
            guidString = guidString.Replace("+", "");
            guidString = guidString.Replace("/", "");
            string token;

            if (guidString.Length > maxTokenLength)
            {
                token = guidString.Substring(0, maxTokenLength);
            }
            else
            {
                token = guidString;
            }

            return token;
        }

        /// <summary>
        /// Converts a string to a skill type index if possible.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>The skill type index. 0 will be returned if the string didn't match a skill type.</returns>
        public static int StringToSkillTypeIndex(string s)
        {
            for (int i = 0; i < SkillTree.Settings.skillTypes.Count; i++)
            {
                if (SkillTree.Settings.skillTypes[i] == s)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the default stat identity.
        /// </summary>
        /// <returns>The stat identity at index 0 or null if no stat identities exist.</returns>
        public static StatIdentity GetDefaultStatIdentity()
        {
            var identities = SkillTree.GetAllStatIdentities();

            if (identities.Length == 0)
            {
                return null;
            }

            return identities[0];
        }

        /// <summary>
        /// Sets rect transform padding.
        /// </summary>
        /// <param name="rt">The RectTransform.</param>
        /// <param name="padding">The padding.</param>
        public static void SetPadding(this RectTransform rt, RectPadding padding)
        {
            rt.SetLeft(padding.left);
            rt.SetTop(padding.top);
            rt.SetRight(padding.right);
            rt.SetBottom(padding.bottom);
        }

        /// <summary>
        /// Sets the left value of a RectTransform.
        /// </summary>
        /// <param name="rt">This RectTransform.</param>
        /// <param name="left">The left amount.</param>
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        /// <summary>
        /// Sets the right value of a RectTransform.
        /// </summary>
        /// <param name="rt">This RectTransform.</param>
        /// <param name="right">The right amount.</param>
        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        /// <summary>
        /// Sets the top value of a RectTransform.
        /// </summary>
        /// <param name="rt">This RectTransform.</param>
        /// <param name="top">The top amount.</param>
        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        /// <summary>
        /// Sets the bottom value of a RectTransform.
        /// </summary>
        /// <param name="rt">This RectTransform.</param>
        /// <param name="bottom">The bottom amount.</param>
        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        /// <summary>
        /// Forces the RectTransform inside of the camera's view.
        /// </summary>
        /// <param name="rectTransform">This RectTransform.</param>
        public static void ForceInsideView(this RectTransform rectTransform)
        {
            // Get the screen corners in world space
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            // Get the bounds of the screen
            Vector3 minScreenBounds = new Vector3(0, 0, 0);
            Vector3 maxScreenBounds = new Vector3(Screen.width, Screen.height, 0);

            // Calculate the offset if the RectTransform is out of bounds
            Vector3 offset = Vector3.zero;

            if (corners[0].x < minScreenBounds.x) // Left
            {
                offset.x = minScreenBounds.x - corners[0].x;
            }
            if (corners[2].x > maxScreenBounds.x) // Right
            {
                offset.x = maxScreenBounds.x - corners[2].x;
            }
            if (corners[0].y < minScreenBounds.y) // Bottom
            {
                offset.y = minScreenBounds.y - corners[0].y;
            }
            if (corners[1].y > maxScreenBounds.y) // Top
            {
                offset.y = maxScreenBounds.y - corners[1].y;
            }

            // Apply the offset
            rectTransform.position += offset;
        }

        /// <summary>
        /// Counts the bounding box corners of the given RectTransform that are visible in screen space.
        /// </summary>
        /// <returns>The amount of bounding box corners that are visible.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">The camera. Leave it null for overlay canvasses.</param>
        private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera = null)
        {
            Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
            rectTransform.GetWorldCorners(objectCorners);

            int visibleCorners = 0;
            Vector3 tempScreenSpaceCorner; // Cached
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                if (camera != null)
                    tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
                else
                {
                    tempScreenSpaceCorner = objectCorners[i]; // If no camera is provided we assume the canvas is Overlay and world space == screen space
                }

                if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                {
                    visibleCorners++;
                }
            }
            return visibleCorners;
        }

        /// <summary>
        /// Determines if this RectTransform is fully visible.
        /// Works by checking if each bounding box corner of this RectTransform is inside the screen space view frustrum.
        /// </summary>
        /// <returns>True if is fully visible. Otherwise, false.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera. Leave it null for overlay canvasses.</param>
        public static bool IsFullyVisible(this RectTransform rectTransform, Camera camera = null)
        {
            if (!rectTransform.gameObject.activeInHierarchy)
                return false;

            return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
        }

        /// <summary>
        /// Determines if this RectTransform is at least partially visible.
        /// Works by checking if any bounding box corner of this RectTransform is inside the screen space view frustrum.
        /// </summary>
        /// <returns>True if it is at least partially visible. Otherwise, false.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera. Leave it null for overlay canvasses.</param>
        public static bool IsPartiallyVisible(this RectTransform rectTransform, Camera camera = null)
        {
            if (!rectTransform.gameObject.activeInHierarchy)
                return false;

            return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
        }

        /// <summary>
        /// Counts the number of corners this RectTransform has inside of another RectTransform.
        /// </summary>
        /// <param name="rectTransform">This RectTransform.</param>
        /// <param name="other">The other RectTransform.</param>
        /// <param name="endAsap">If it should stop counting corners after the first corner found inside of the other RectTransform.</param>
        /// <returns>The amount of corners inside the rect</returns>
        public static int CountCornersInsideOf(this RectTransform rectTransform, RectTransform other, bool endAsap = false)
        {
            // Get corners
            rectTransform.GetWorldCorners(objectCorners);

            int cornersInside = 0;

            // Find corners inside the rect
            for (var i = 0; i < objectCorners.Length; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(other, objectCorners[i]))
                {
                    cornersInside++;
                    if (endAsap) break;
                }
            }

            return cornersInside;
        }

        /// <summary>
        /// If the RectTransform is fully inside another.
        /// </summary>
        /// <param name="rectTransform">This RectTransform.</param>
        /// <param name="other">The other RectTransform.</param>
        /// <returns>True if it's fully inside the other RectTransform. Otherwise, false.</returns>
        public static bool IsFullyInside(this RectTransform rectTransform, RectTransform other)
        {
            if (!rectTransform.gameObject.activeInHierarchy) return false;

            return CountCornersInsideOf(rectTransform, other) == 4;
        }

        /// <summary>
        /// If the RectTransform is partially inside another.
        /// </summary>
        /// <param name="rectTransform">This RectTransform.</param>
        /// <param name="other">The other RectTransform.</param>
        /// <returns>True if it's partially inside the other RectTransform. Otherwise, false.</returns>
        public static bool IsPartiallyInside(this RectTransform rectTransform, RectTransform other)
        {
            if (!rectTransform.gameObject.activeInHierarchy) return false;

            return CountCornersInsideOf(rectTransform, other, true) > 0;
        }

        /// <summary>
        /// If the RectTransform is fully outside another.
        /// </summary>
        /// <param name="rectTransform">This RectTransform.</param>
        /// <param name="other">The other RectTransform.</param>
        /// <returns>True if it's fully outside the other RectTransform. Otherwise, false.</returns>
        public static bool IsFullyOutside(this RectTransform rectTransform, RectTransform other)
        {
            if (!rectTransform.gameObject.activeInHierarchy) return false;

            return CountCornersInsideOf(rectTransform, other) == 0;
        }

        /// <summary>
        /// If the RectTransform is partially outside another.
        /// </summary>
        /// <param name="rectTransform">This RectTransform.</param>
        /// <param name="other">The other RectTransform.</param>
        /// <returns>True if it's partially outside the other RectTransform. Otherwise, false.</returns>
        public static bool IsPartiallyOutside(this RectTransform rectTransform, RectTransform other)
        {
            if (!rectTransform.gameObject.activeInHierarchy) return false;

            return CountCornersInsideOf(rectTransform, other) < 4;
        }
    }
}