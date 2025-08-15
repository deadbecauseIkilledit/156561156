//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.SkillTree.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.SkillTree.Editor
{
    public class StatsEditorWindow : EditorWindow
    {
        private ToolbarSearchField searchField;
        private EnumField numericTypeFilter;
        private EnumField valueTypeFilter;
        private VisualElement statList;
        private Button createGroupButton;
        private VisualElement inspectorContent;
        private Label inspectorLabel;
        private IntegerField idField;
        private TextField displayNameField;
        private TextField abbreviationField;
        private EnumField numericTypeField;
        private EnumField valueTypeField;
        private Toggle hasMinToggle;
        private Toggle hasMaxToggle;
        private FloatField minFloatField;
        private FloatField maxFloatField;
        private IntegerField minIntegerField;
        private IntegerField maxIntegerField;
        private Button deleteButton;
        private Label defaultsLabel;

        private StatIdentity selectedStat;
        private StatGroup selectedGroup;
        private Dictionary<StatIdentity, StatElement> statElements = new();
        private Dictionary<StatGroup, GroupElement> groupElements = new();

        [MenuItem("Window/Skill Tree/Stats")]
        public static void Open()
        {
            StatsEditorWindow wnd = GetWindow<StatsEditorWindow>();
            wnd.titleContent = new GUIContent("Stats");
        }

        public void CreateGUI()
        {
            minSize = new Vector2(750, 450);

            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/StylishEsper/SkillTree/Scripts/Editor/StatsEditorWindow.uxml");
            visualTree.CloneTree(root);

            searchField = root.Q<ToolbarSearchField>("SearchField");
            numericTypeFilter = root.Q<EnumField>("NumericTypeFilter");
            valueTypeFilter = root.Q<EnumField>("ValueTypeFilter");
            statList = root.Q<VisualElement>("StatList");
            createGroupButton = root.Q<Button>("CreateGroupButton");
            inspectorContent = root.Q<VisualElement>("InspectorContent");
            inspectorLabel = root.Q<Label>("InspectorLabel");
            idField = root.Q<IntegerField>("IDField");
            displayNameField = root.Q<TextField>("DisplayNameField");
            abbreviationField = root.Q<TextField>("AbbreviationField");
            numericTypeField = root.Q<EnumField>("NumericTypeField");
            valueTypeField = root.Q<EnumField>("ValueTypeField");
            hasMinToggle = root.Q<Toggle>("HasMinToggle");
            hasMaxToggle = root.Q<Toggle>("HasMaxToggle");
            minFloatField = root.Q<FloatField>("MinFloatField");
            maxFloatField = root.Q<FloatField>("MaxFloatField");
            minIntegerField = root.Q<IntegerField>("MinIntegerField");
            maxIntegerField = root.Q<IntegerField>("MaxIntegerField");
            deleteButton = root.Q<Button>("DeleteButton");
            defaultsLabel = root.Q<Label>("DefaultsLabel");

            idField.SetEnabled(false);

            displayNameField.RegisterValueChangedCallback(x =>
            {
                if (selectedStat)
                {
                    selectedStat.UpdateAssetName();
                    statElements[selectedStat].button.text = selectedStat.displayName;
                }
                else if (selectedGroup)
                {
                    selectedGroup.UpdateAssetName();
                    groupElements[selectedGroup].button.text = selectedGroup.displayName;
                }
            });

            numericTypeField.RegisterValueChangedCallback(x =>
            {
                UpdateElements();
            });

            hasMinToggle.RegisterValueChangedCallback(x =>
            {
                UpdateElements();

                if (x.newValue)
                {
                    if (selectedGroup && selectedGroup.defaultHasMax)
                    {
                        if (selectedGroup.defaultMaxFloatValue < selectedGroup.defaultMinFloatValue)
                        {
                            selectedGroup.defaultMaxFloatValue = selectedGroup.defaultMinFloatValue;
                        }
                    }
                    else if (selectedStat && selectedStat.hasMax)
                    {
                        if (selectedStat.maxFloatValue < selectedStat.minFloatValue)
                        {
                            selectedStat.maxFloatValue = selectedStat.minFloatValue;
                        }
                    }
                }
            });

            hasMaxToggle.RegisterValueChangedCallback(x =>
            {
                UpdateElements();

                if (x.newValue)
                {
                    if (selectedGroup && selectedGroup.defaultHasMin)
                    {
                        if (selectedGroup.defaultMinFloatValue > selectedGroup.defaultMaxFloatValue)
                        {
                            selectedGroup.defaultMinFloatValue = selectedGroup.defaultMaxFloatValue;
                        }
                    }
                    else if (selectedStat && selectedStat.hasMin)
                    {
                        if (selectedStat.minFloatValue > selectedStat.maxFloatValue)
                        {
                            selectedStat.minFloatValue = selectedStat.maxFloatValue;
                        }
                    }
                }
            });

            searchField.RegisterValueChangedCallback(x =>
            {
                ApplySearchFilters();
            });

            numericTypeFilter.RegisterValueChangedCallback(x =>
            {
                ApplySearchFilters();
            });

            valueTypeFilter.RegisterValueChangedCallback(x =>
            {
                ApplySearchFilters();
            });

            minFloatField.RegisterValueChangedCallback(x =>
            {
                if (selectedGroup && selectedGroup.defaultHasMax)
                {
                    if (selectedGroup.defaultMinFloatValue > selectedGroup.defaultMaxFloatValue)
                    {
                        selectedGroup.defaultMinFloatValue = selectedGroup.defaultMaxFloatValue;
                    }
                }
                else if (selectedStat && selectedStat.hasMax)
                {
                    if (selectedStat.minFloatValue > selectedStat.maxFloatValue)
                    {
                        selectedStat.minFloatValue = selectedStat.maxFloatValue;
                    }
                }
            });

            maxFloatField.RegisterValueChangedCallback(x =>
            {
                if (selectedGroup && selectedGroup.defaultHasMin)
                {
                    if (selectedGroup.defaultMaxFloatValue < selectedGroup.defaultMinFloatValue)
                    {
                        selectedGroup.defaultMaxFloatValue = selectedGroup.defaultMinFloatValue;
                    }
                }
                else if (selectedStat && selectedStat.hasMin)
                {
                    if (selectedStat.maxFloatValue > selectedStat.minFloatValue)
                    {
                        selectedStat.maxFloatValue = selectedStat.minFloatValue;
                    }
                }
            });

            minIntegerField.RegisterValueChangedCallback(x =>
            {
                if (selectedGroup && selectedGroup.defaultHasMax)
                {
                    if (selectedGroup.defaultMinIntegerValue > selectedGroup.defaultMaxIntegerValue)
                    {
                        selectedGroup.defaultMinIntegerValue = selectedGroup.defaultMaxIntegerValue;
                    }
                }
                else if (selectedStat && selectedStat.hasMax)
                {
                    if (selectedStat.minIntegerValue > selectedStat.maxIntegerValue)
                    {
                        selectedStat.minIntegerValue = selectedStat.maxIntegerValue;
                    }
                }
            });

            maxIntegerField.RegisterValueChangedCallback(x =>
            {
                if (selectedGroup && selectedGroup.defaultHasMin)
                {
                    if (selectedGroup.defaultMaxIntegerValue < selectedGroup.defaultMinIntegerValue)
                    {
                        selectedGroup.defaultMaxIntegerValue = selectedGroup.defaultMinIntegerValue;
                    }
                }
                else if (selectedStat && selectedStat.hasMin)
                {
                    if (selectedStat.maxIntegerValue > selectedStat.minIntegerValue)
                    {
                        selectedStat.maxIntegerValue = selectedStat.minIntegerValue;
                    }
                }
            });

            createGroupButton.clicked += CreateNewGroup;
            deleteButton.clicked += DeleteSelection;

            Deselect();
            ReloadList();
        }

        private void CreateNewGroup()
        {
            var group = StatGroup.Create();
            CreateGroupElement(group);
            SelectGroup(group);
        }

        private void CreateNewStat(StatGroup group)
        {
            var stat = StatIdentity.Create(group);
            CreateStatElement(stat, group);
            SelectStat(stat);
            group.Save();
        }

        private void DeleteSelection()
        {
            if (selectedStat)
            {
                selectedStat.groupReference.statIdentities.Remove(selectedStat);
                selectedStat.groupReference.Save();
                AssetDatabase.DeleteAsset(StatIdentity.GetFullPath(selectedStat));

                Deselect();
                ReloadList();
            }
            else if (selectedGroup)
            {
                if (EditorUtility.DisplayDialog("Delete Stat Group?", $"Are you sure you want to delete the '{selectedGroup.displayName}' stat group? This cannot be undone.", "Delete", "Cancel"))
                {
                    foreach (var stat in selectedGroup.statIdentities)
                    {
                        AssetDatabase.DeleteAsset(StatIdentity.GetFullPath(stat));
                    }

                    AssetDatabase.DeleteAsset(StatGroup.GetFullPath(selectedGroup));

                    Deselect();
                    ReloadList();
                }
            }
        }

        private void ReloadList()
        {
            statElements.Clear();
            groupElements.Clear();
            statList.Clear();

            var groups = Resources.LoadAll<StatGroup>("SkillTree/Stats/Groups");
            groups = groups.OrderBy(x => x.id).ToArray();
            foreach (var group in groups)
            {
                CreateGroupElement(group);

                foreach (var stat in group.statIdentities)
                {
                    CreateStatElement(stat, group);
                }
            }
        }

        private void CreateGroupElement(StatGroup group)
        {
            var groupElement = new VisualElement();
            groupElement.style.flexShrink = 1;
            groupElement.style.flexGrow = 1;

            var foldout = new Foldout();
            foldout.style.flexGrow = 1;
            foldout.style.marginBottom = 2.5f;
            foldout.style.marginTop = 2.5f;
            groupElement.Add(foldout);

            VisualElement checkmarkElement = foldout.Query<VisualElement>("unity-checkmark");
            var buttonArea = checkmarkElement.parent;

            var foldoutToggle = buttonArea.parent;
            foldoutToggle.style.marginLeft = 0;

            var button = new ToolbarButton();
            button.text = group.displayName;
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.style.flexGrow = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.paddingTop = 2;
            button.style.paddingBottom = 2;
            button.style.paddingLeft = 4;
            button.style.paddingRight = 4;
            button.style.marginRight = 8;
            button.style.height = 20;

            groupElements.Add(group, new GroupElement()
            {
                button = button,
                foldout = foldout
            });

            button.clicked += () => SelectGroup(group);

            buttonArea.Add(button);

            var addStatButton = new ToolbarButton();
            addStatButton.text = "+";
            addStatButton.tooltip = "Create Stat";
            addStatButton.AddToClassList("addStatButton");

            addStatButton.clicked += () => CreateNewStat(group);

            buttonArea.Add(addStatButton);

            statList.Add(groupElement);
        }

        private void CreateStatElement(StatIdentity stat, StatGroup group)
        {
            var button = new ToolbarButton();
            button.text = stat.displayName;
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.style.flexGrow = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.marginBottom = 2.5f;
            button.style.paddingTop = 2;
            button.style.paddingBottom = 2;
            button.style.paddingLeft = 4;
            button.style.paddingRight = 4;
            button.style.marginLeft = 8;
            button.style.height = 20;
            button.clicked += () => SelectStat(stat);
            groupElements[group].foldout.Add(button);

            statElements.Add(stat, new StatElement()
            {
                button = button
            });
        }

        private void SelectGroup(StatGroup group)
        {
            Deselect();
            selectedGroup = group;

            var serializedObject = new SerializedObject(group);
            idField.BindProperty(serializedObject.FindProperty("id"));
            displayNameField.BindProperty(serializedObject.FindProperty("displayName"));
            numericTypeField.BindProperty(serializedObject.FindProperty("defaultNumericType"));
            valueTypeField.BindProperty(serializedObject.FindProperty("defaultValueType"));
            hasMinToggle.BindProperty(serializedObject.FindProperty("defaultHasMin"));
            hasMaxToggle.BindProperty(serializedObject.FindProperty("defaultHasMax"));
            minFloatField.BindProperty(serializedObject.FindProperty("defaultMinFloatValue"));
            maxFloatField.BindProperty(serializedObject.FindProperty("defaultMaxFloatValue"));
            minIntegerField.BindProperty(serializedObject.FindProperty("defaultMinIntegerValue"));
            maxIntegerField.BindProperty(serializedObject.FindProperty("defaultMaxIntegerValue"));

            UpdateElements();

            inspectorContent.style.display = DisplayStyle.Flex;
        }

        private void SelectStat(StatIdentity stat)
        {
            Deselect();
            selectedStat = stat;

            var serializedObject = new SerializedObject(stat);
            idField.BindProperty(serializedObject.FindProperty("id"));
            displayNameField.BindProperty(serializedObject.FindProperty("displayName"));
            abbreviationField.BindProperty(serializedObject.FindProperty("abbreviation"));
            numericTypeField.BindProperty(serializedObject.FindProperty("numericType"));
            valueTypeField.BindProperty(serializedObject.FindProperty("valueType"));
            hasMinToggle.BindProperty(serializedObject.FindProperty("hasMin"));
            hasMaxToggle.BindProperty(serializedObject.FindProperty("hasMax"));
            minFloatField.BindProperty(serializedObject.FindProperty("minFloatValue"));
            maxFloatField.BindProperty(serializedObject.FindProperty("maxFloatValue"));
            minIntegerField.BindProperty(serializedObject.FindProperty("minIntegerValue"));
            maxIntegerField.BindProperty(serializedObject.FindProperty("maxIntegerValue"));

            UpdateElements();

            inspectorContent.style.display = DisplayStyle.Flex;
        }

        private void UpdateElements()
        {
            if (selectedGroup)
            {
                inspectorLabel.text = "Stat Group";
                defaultsLabel.style.display = DisplayStyle.Flex;
                abbreviationField.style.display = DisplayStyle.None;

                if (selectedGroup.defaultHasMin)
                {
                    if (selectedGroup.defaultNumericType == StatValue.NumericType.Float)
                    {
                        minFloatField.style.display = DisplayStyle.Flex;
                        minIntegerField.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        minFloatField.style.display = DisplayStyle.None;
                        minIntegerField.style.display = DisplayStyle.Flex;
                    }
                }
                else
                {
                    minFloatField.style.display = DisplayStyle.None;
                    minIntegerField.style.display = DisplayStyle.None;
                }

                if (selectedGroup.defaultHasMax)
                {
                    if (selectedGroup.defaultNumericType == StatValue.NumericType.Float)
                    {
                        maxFloatField.style.display = DisplayStyle.Flex;
                        maxIntegerField.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        maxFloatField.style.display = DisplayStyle.None;
                        maxIntegerField.style.display = DisplayStyle.Flex;
                    }
                }
                else
                {
                    maxFloatField.style.display = DisplayStyle.None;
                    maxIntegerField.style.display = DisplayStyle.None;
                }
            }
            else if (selectedStat)
            {
                inspectorLabel.text = "Stat";
                defaultsLabel.style.display = DisplayStyle.None;
                abbreviationField.style.display = DisplayStyle.Flex;

                if (selectedStat.hasMin)
                {
                    if (selectedStat.numericType == StatValue.NumericType.Float)
                    {
                        minFloatField.style.display = DisplayStyle.Flex;
                        minIntegerField.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        minFloatField.style.display = DisplayStyle.None;
                        minIntegerField.style.display = DisplayStyle.Flex;
                    }
                }
                else
                {
                    minFloatField.style.display = DisplayStyle.None;
                    minIntegerField.style.display = DisplayStyle.None;
                }

                if (selectedStat.hasMax)
                {
                    if (selectedStat.numericType == StatValue.NumericType.Float)
                    {
                        maxFloatField.style.display = DisplayStyle.Flex;
                        maxIntegerField.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        maxFloatField.style.display = DisplayStyle.None;
                        maxIntegerField.style.display = DisplayStyle.Flex;
                    }
                }
                else
                {
                    maxFloatField.style.display = DisplayStyle.None;
                    maxIntegerField.style.display = DisplayStyle.None;
                }
            }
        }

        private void Deselect()
        {
            selectedStat = null;
            selectedGroup = null;
            inspectorContent.style.display = DisplayStyle.None;
        }

        private void ApplySearchFilters()
        {
            foreach (var item in groupElements.Values)
            {
                item.button.style.opacity = 1;
                item.foldout.style.display = DisplayStyle.Flex;
            }

            foreach (var item in statElements.Values)
            {
                item.button.style.display = DisplayStyle.Flex;
            }

            foreach (var groupElement in groupElements)
            {
                bool nameMatch = string.IsNullOrEmpty(searchField.value) || groupElement.Key.displayName.ToLower().Contains(searchField.value.ToLower());
                bool numericTypeMatch = (NumericTypeFilter)numericTypeFilter.value == NumericTypeFilter.All || groupElement.Key.defaultNumericType == Enum.Parse<StatValue.NumericType>(numericTypeFilter.value.ToString());
                bool valueTypeMatch = (ValueTypeFilter)valueTypeFilter.value == ValueTypeFilter.All || groupElement.Key.defaultValueType == Enum.Parse<StatValue.ValueType>(valueTypeFilter.value.ToString());

                bool groupDoesntMatch = !nameMatch || !numericTypeMatch || !valueTypeMatch;

                if (groupDoesntMatch)
                {
                    groupElement.Value.button.style.opacity = 0.5f;
                }

                bool hasAnyRelevantChildren = false;

                foreach (var stat in groupElement.Key.statIdentities)
                {
                    var statElement = statElements[stat];
                    nameMatch = string.IsNullOrEmpty(searchField.value) || stat.displayName.ToLower().Contains(searchField.value.ToLower());
                    numericTypeMatch = (NumericTypeFilter)numericTypeFilter.value == NumericTypeFilter.All || stat.numericType == Enum.Parse<StatValue.NumericType>(numericTypeFilter.value.ToString());
                    valueTypeMatch = (ValueTypeFilter)valueTypeFilter.value == ValueTypeFilter.All || stat.valueType == Enum.Parse<StatValue.ValueType>(valueTypeFilter.value.ToString());

                    if (!nameMatch || !numericTypeMatch || !valueTypeMatch)
                    {
                        statElement.button.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        hasAnyRelevantChildren = true;
                    }
                }

                if (groupDoesntMatch && !hasAnyRelevantChildren && groupElement.Key.statIdentities.Count > 0 || groupDoesntMatch && groupElement.Key.statIdentities.Count == 0)
                {
                    groupElement.Value.foldout.style.display = DisplayStyle.None;
                }
            }
        }

        private struct StatElement
        {
            public Button button;
        }

        private struct GroupElement
        {
            public Button button;
            public Foldout foldout;
        }

        private enum NumericTypeFilter
        {
            All,
            Float,
            Integer
        }

        private enum ValueTypeFilter
        {
            All,
            Value,
            Percent
        }
    }
}
#endif