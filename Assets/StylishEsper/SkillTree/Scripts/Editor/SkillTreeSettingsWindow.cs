//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.SkillTree.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.SkillTree.Editor
{
    public class SkillTreeSettingsWindow : EditorWindow
    {
        private static string settingsName = "SkillTreeSettings.asset";
        private static string directoryPath = Path.Combine("Assets", "StylishEsper", "SkillTree", "Resources", "SkillTree", "Settings");
        private static string fullPath = Path.Combine(directoryPath, settingsName);

        private SkillTreeSettings settings;
        private VisualElement skillTypesContent;
        private Button addTypeButton;
        private Button deleteTypeButton;
        private IntegerField maxLevelField;
        private Toggle allowDowngradeField;
        private Toggle confirmationField;

        [MenuItem("Window/Skill Tree/Settings")]
        public static void Open()
        {
            SkillTreeSettingsWindow wnd = GetWindow<SkillTreeSettingsWindow>();
            wnd.titleContent = new GUIContent("Skill Tree Settings");
        }

        private static SkillTreeSettings GetOrCreateSettings()
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var settings = AssetDatabase.LoadAssetAtPath<SkillTreeSettings>(fullPath);

            if (settings == null)
            {
                settings = CreateInstance<SkillTreeSettings>();
                settings.allowDowngrade = true;
                settings.skillTypes = new List<string> { "Active", "Passive" };
                settings.maxUnitLevel = 100;
                AssetDatabase.CreateAsset(settings, fullPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        private void CreateGUI()
        {
            minSize = new Vector2(600, 500);

            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/StylishEsper/SkillTree/Scripts/Editor/SkillTreeSettingsWindow.uxml");
            visualTree.CloneTree(root);

            settings = GetOrCreateSettings();
            skillTypesContent = root.Q<VisualElement>("SkillTypesContent");
            addTypeButton = root.Q<Button>("AddTypeButton");
            addTypeButton.clicked += AddSkillType;
            deleteTypeButton = root.Q<Button>("DeleteTypeButton");
            deleteTypeButton.clicked += DeleteSkillType;
            maxLevelField = root.Q<IntegerField>("MaxLevelField");
            allowDowngradeField = root.Q<Toggle>("AllowDowngradeField");
            confirmationField = root.Q<Toggle>("ConfirmationField");

            ReloadContent();
        }

        private void OnDestroy()
        {
            ApplyChanges();
        }

        private void AddSkillType()
        {
            settings.skillTypes.Add(string.Empty);
            ReloadContent();
        }

        private void DeleteSkillType()
        {
            settings.skillTypes.RemoveAt(settings.skillTypes.Count - 1);
            ReloadContent();
        }

        private void ReloadContent()
        {
            skillTypesContent.Clear();

            var serializedObject = new SerializedObject(settings);
            allowDowngradeField.BindProperty(serializedObject.FindProperty("allowDowngrade"));
            confirmationField.BindProperty(serializedObject.FindProperty("changesRequireConfirmation"));

            int i = 0;
            foreach (var skillType in settings.skillTypes)
            {
                var textField = new TextField($"Element {i}");
                textField.SetValueWithoutNotify(skillType);
                textField.style.flexGrow = 1;
                textField.style.marginBottom = 4;
                int index = i;
                textField.RegisterValueChangedCallback(x =>
                {
                    settings.skillTypes[index] = x.newValue;

                    if (settings.skillTypes.Count != settings.skillTypes.Distinct().Count())
                    {
                        Debug.LogWarning("Skill Tree: Skill type names should not be identical.");
                    }
                });
                skillTypesContent.Add(textField);

                i++;
            }

            maxLevelField.BindProperty(serializedObject.FindProperty("maxUnitLevel"));

            maxLevelField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 1)
                {
                    Debug.LogWarning("Skill Tree: Max unit level cannot be less than 1.");
                }

                settings.maxUnitLevel = x.newValue;
            });
        }

        private void ApplyChanges()
        {
            settings.SaveSettings();
        }
    }
}
#endif