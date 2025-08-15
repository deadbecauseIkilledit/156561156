//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.SkillTree.Stats;
using System;
using System.Security.Principal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.SkillTree.Editor
{
    public class SkillStatsEditor : EditorWindow
    {
        private static SkillNode skill;

        private static Action onStatsChanged;

        private ListView statsList;

        private Label skillNameLabel;

        public static void Open(SkillNode skill, Action onStatsChanged)
        {
            SkillStatsEditor wnd = GetWindow<SkillStatsEditor>();
            wnd.titleContent = new GUIContent("Skill Stats");

            SkillStatsEditor.skill = skill;
            SkillStatsEditor.onStatsChanged = onStatsChanged;

            wnd.ReloadStats();
        }

        public void CreateGUI()
        {
            minSize = new Vector2(350, 450);

            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/StylishEsper/SkillTree/Scripts/Editor/SkillStatsEditor.uxml");
            visualTree.CloneTree(root);

            statsList = rootVisualElement.Q<ListView>("StatsList");
            skillNameLabel = rootVisualElement.Q<Label>("SkillNameLabel");

            statsList.makeItem = () =>
            {
                var serializedObject = new SerializedObject(skill);
                return CreateNodeStatUI(serializedObject);
            };

            statsList.bindItem = (element, index) =>
            {
                if (index < 0 || index >= skill.stats.Count)
                {
                    return;
                }

                var serializedObject = new SerializedObject(skill);
                var stat = skill.stats[index];
                var serializedProperty = serializedObject.FindProperty("stats").GetArrayElementAtIndex(index);

                var statField = element.Q<ObjectField>("StatField");
                statField.BindProperty(serializedProperty.FindPropertyRelative("identity"));
                statField.objectType = typeof(StatIdentity);

                var initialFloatField = element.Q<FloatField>("InitialFloatField");
                initialFloatField.BindProperty(serializedProperty.FindPropertyRelative("initialValue").FindPropertyRelative("value").FindPropertyRelative("floatValue"));

                var scalingFloatField = element.Q<FloatField>("ScalingFloatField");
                scalingFloatField.BindProperty(serializedProperty.FindPropertyRelative("scaling").FindPropertyRelative("value").FindPropertyRelative("floatValue"));

                var initialIntField = element.Q<IntegerField>("InitialIntField");
                initialIntField.BindProperty(serializedProperty.FindPropertyRelative("initialValue").FindPropertyRelative("value").FindPropertyRelative("integerValue"));

                var scalingIntField = element.Q<IntegerField>("ScalingIntField");
                scalingIntField.BindProperty(serializedProperty.FindPropertyRelative("scaling").FindPropertyRelative("value").FindPropertyRelative("integerValue"));

                var typeField = element.Q<EnumField>("TypeField");
                typeField.BindProperty(serializedProperty.FindPropertyRelative("combineType"));

                var operatorField = element.Q<EnumField>("OperatorField");
                operatorField.BindProperty(serializedProperty.FindPropertyRelative("combineOperator"));

                if (!stat.identity || stat.identity.numericType == StatValue.NumericType.Float)
                {
                    initialFloatField.style.display = DisplayStyle.Flex;
                    scalingFloatField.style.display = DisplayStyle.Flex;
                    initialIntField.style.display = DisplayStyle.None;
                    scalingIntField.style.display = DisplayStyle.None;
                }
                else if (stat.identity.numericType == StatValue.NumericType.Integer)
                {
                    initialFloatField.style.display = DisplayStyle.None;
                    scalingFloatField.style.display = DisplayStyle.None;
                    initialIntField.style.display = DisplayStyle.Flex;
                    scalingIntField.style.display = DisplayStyle.Flex;
                }

                statField.RegisterValueChangedCallback(x =>
                {
                    if (!x.newValue || x.previousValue == x.newValue)
                    {
                        return;
                    }

                    var identity = x.newValue as StatIdentity;
                    if (identity.numericType == StatValue.NumericType.Float)
                    {
                        initialFloatField.style.display = DisplayStyle.Flex;
                        scalingFloatField.style.display = DisplayStyle.Flex;
                        initialIntField.style.display = DisplayStyle.None;
                        scalingIntField.style.display = DisplayStyle.None;
                    }
                    else if (identity.numericType == StatValue.NumericType.Integer)
                    {
                        initialFloatField.style.display = DisplayStyle.None;
                        scalingFloatField.style.display = DisplayStyle.None;
                        initialIntField.style.display = DisplayStyle.Flex;
                        scalingIntField.style.display = DisplayStyle.Flex;
                    }

                    stat.SetIdentity(identity);
                });
            };

            statsList.itemsAdded += (x) => 
            {
                statsList.Clear();
                statsList.schedule.Execute(() => 
                { 
                    statsList.Rebuild();
                    onStatsChanged.Invoke();
                }).ExecuteLater(100);
            };

            statsList.itemsRemoved += (x) =>
            {
                statsList.Clear();
                statsList.schedule.Execute(() =>
                {
                    statsList.Rebuild();
                    onStatsChanged.Invoke();
                }).ExecuteLater(100);
            };
        }

        private void ReloadStats()
        {
            var serializedObject = new SerializedObject(skill);
            skillNameLabel.text = skill.displayName;
            statsList.BindProperty(serializedObject.FindProperty("stats"));
            statsList.Rebuild();
        }

        private VisualElement CreateNodeStatUI(SerializedObject serializedObject)
        {
            var parent = new VisualElement();

            var statField = new ObjectField();
            statField.name = "StatField";
            statField.style.flexGrow = 0.5f;
            statField.style.marginLeft = 4;
            statField.style.marginRight = 4;
            statField.style.marginBottom = 4;
            statField.style.marginTop = 4;
            parent.Add(statField);

            var initialFloatField = new FloatField("Initial Value");
            initialFloatField.name = "InitialFloatField";
            initialFloatField.style.flexGrow = 0.5f;
            initialFloatField.style.marginLeft = 4;
            initialFloatField.style.marginRight = 4;
            initialFloatField.style.marginBottom = 4;
            parent.Add(initialFloatField);

            var scalingFloatField = new FloatField("Scaling");
            scalingFloatField.name = "ScalingFloatField";
            scalingFloatField.style.flexGrow = 0.5f;
            scalingFloatField.style.marginLeft = 4;
            scalingFloatField.style.marginRight = 4;
            scalingFloatField.style.marginBottom = 4;
            parent.Add(scalingFloatField);

            var initialIntField = new IntegerField("Initial Value");
            initialIntField.name = "InitialIntField";
            initialIntField.style.flexGrow = 0.5f;
            initialIntField.style.marginLeft = 4;
            initialIntField.style.marginRight = 4;
            initialIntField.style.marginBottom = 4;
            parent.Add(initialIntField);

            var scalingIntField = new IntegerField("Scaling");
            scalingIntField.name = "ScalingIntField";
            scalingIntField.style.flexGrow = 0.5f;
            scalingIntField.style.marginLeft = 4;
            scalingIntField.style.marginRight = 4;
            scalingIntField.style.marginBottom = 4;
            parent.Add(scalingIntField);

            var typeField = new EnumField("Combine Type");
            typeField.name = "TypeField";
            typeField.style.marginLeft = 4;
            typeField.style.marginRight = 4;
            typeField.style.marginBottom = 4;
            parent.Add(typeField);

            var operatorField = new EnumField("Operator");
            operatorField.name = "OperatorField";
            operatorField.style.marginLeft = 4;
            operatorField.style.marginRight = 4;
            operatorField.style.marginBottom = 4;
            parent.Add(operatorField);

            return parent;
        }
    }
}
#endif