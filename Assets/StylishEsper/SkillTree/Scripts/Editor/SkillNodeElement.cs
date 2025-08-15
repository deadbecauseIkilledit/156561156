//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: A UI representation of a skill node for the Skill Tree editor window.
//***************************************************************************************

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.SkillTree.Editor
{
    public class SkillNodeElement : Button
    {
        public SkillGraph skillGraphReference;

        public SkillNode node;

        public Button connectButton;

        private VisualElement completionVisual;

        private bool hovered;

        public SkillNodeElement(SkillGraph skillGraphReference, SkillNode node)
        {
            RegisterCallback<MouseEnterEvent>(evt =>
            {
                hovered = true;
                ReloadSprite();
            });

            RegisterCallback<MouseLeaveEvent>(evt =>
            {
                hovered = false;
                ReloadSprite();
            });

            this.skillGraphReference = skillGraphReference;
            this.node = node;

            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.marginTop = 0;
            style.paddingBottom = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            style.paddingTop = 0;
            style.position = Position.Absolute;
            style.overflow = Overflow.Visible;

            connectButton = new Button();
            connectButton.style.marginBottom = 0;
            connectButton.style.marginLeft = 0;
            connectButton.style.marginRight = 0;
            connectButton.style.marginTop = 0;
            connectButton.style.paddingBottom = 0;
            connectButton.style.paddingLeft = 0;
            connectButton.style.paddingRight = 0;
            connectButton.style.paddingTop = 0;
            connectButton.style.borderTopLeftRadius = 0f;
            connectButton.style.borderTopRightRadius = 0f;
            connectButton.style.borderBottomLeftRadius = 0f;
            connectButton.style.borderBottomRightRadius = 0f;
#if !UNITY_2021
            var bgSize = new StyleBackgroundSize(new BackgroundSize(Length.Percent(80), Length.Percent(80)));
            connectButton.style.backgroundSize = bgSize;
#endif
            connectButton.style.backgroundImage = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StylishEsper/SkillTree/Editor/Icons/connection_icon.png"));
            connectButton.style.position = Position.Absolute;

            connectButton.clicked += () =>
            {
                EditorWindow.GetWindow<SkillTreeEditorWindow>().StartConnectionCreation(this);
            };

            Add(connectButton);

            completionVisual = new VisualElement();
            Add(completionVisual);

            Reload();
        }

        public void Reload()
        {
            DisplayCompleteStatus();
            UpdateShape();
            ReloadSprite();
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                style.display = DisplayStyle.Flex;
                connectButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                style.display = DisplayStyle.None;
                connectButton.style.display = DisplayStyle.None;
            }
        }

        public void DisplayCompleteStatus()
        {
            completionVisual.style.backgroundColor = node.IsValid() ? Color.green : new Color(0.7f, 0, 0, 1);
        }

        public void SetPosition(Vector2 position)
        {
            style.left = position.x;
            style.top = position.y;
            UpdateConnectButtonPosition();
        }

        public void SetSize(float width, float height)
        {
            style.width = width;
            style.height = height;
            connectButton.style.width = width * 0.375f;
            connectButton.style.height = height * 0.375f;
            completionVisual.style.width = width * 0.1f;
            completionVisual.style.height = height * 0.1f;
            UpdateConnectButtonPosition();
        }

        public void UpdateConnectButtonPosition()
        {
            connectButton.style.left = style.width.value.value - ((connectButton.style.width.value.value + 1) / 2);
            connectButton.style.top = (connectButton.style.height.value.value + 1) / -2;
        }

        public void UpdateShape(SkillGraph data = null)
        {
            if (data == null)
            {
                data = skillGraphReference;
            }

            var shape = node.isUnique ? node.uniqueShape : data.nodeShape;

            switch (shape)
            {
                case SkillNode.Shape.Square:
                    style.borderTopLeftRadius = 0f;
                    style.borderTopRightRadius = 0f;
                    style.borderBottomLeftRadius = 0f;
                    style.borderBottomRightRadius = 0f;
                    break;

                case SkillNode.Shape.Squircle:
                    var size = (node.isUnique ? node.uniqueSize : data.nodeSize) * 0.125f;
                    style.borderTopLeftRadius = size;
                    style.borderTopRightRadius = size;
                    style.borderBottomLeftRadius = size;
                    style.borderBottomRightRadius = size;
                    break;

                case SkillNode.Shape.Circle:
                    size = node.isUnique ? node.uniqueSize : data.nodeSize;
                    style.borderTopLeftRadius = size;
                    style.borderTopRightRadius = size;
                    style.borderBottomLeftRadius = size;
                    style.borderBottomRightRadius = size;
                    break;

                default:
                    break;
            }

            EditorWindow.GetWindow<SkillTreeEditorWindow>()?.ReloadAllConnections();
        }

        public void ReloadSprite()
        {
#if !UNITY_2021
            var bgSize = new StyleBackgroundSize(new BackgroundSize(Length.Percent(90), Length.Percent(90)));
            style.backgroundSize = bgSize;
#endif

            Sprite sprite = null;

            if (node.unlockedSprite && !hovered)
            {
                sprite = node.unlockedSprite;
            }
            else if (node.obtainedSprite)
            {
                sprite = node.obtainedSprite;
            }
            else if (node.lockedSprite)
            {
                sprite = node.lockedSprite;
            }

            if (!sprite)
            {
                return;
            }

            schedule.Execute(() =>
            {
                var preview = AssetPreview.GetAssetPreview(sprite);
                if (preview != null)
                {
                    style.backgroundImage = new StyleBackground(AssetPreview.GetAssetPreview(sprite));
                }
            }).Until(() => sprite == null || !AssetPreview.IsLoadingAssetPreview(sprite.GetInstanceID()));
        }
    }
}
#endif