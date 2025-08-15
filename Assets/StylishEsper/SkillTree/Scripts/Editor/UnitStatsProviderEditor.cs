//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.SkillTree.Stats;
using System.Security.Principal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.SkillTree.Editor
{
    [CustomEditor(typeof(UnitStatsProvider))]
    public class UnitStatsProviderEditor : UnityEditor.Editor
    {
        private int selectedStat = -1;
        private VisualElement statsContainer;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new VisualElement();

            var target = (UnitStatsProvider)serializedObject.targetObject;

            TextField nameField = new TextField("Unit Name");
            nameField.bindingPath = "data.unitName";
            inspector.Add(nameField);
            
            IntegerField unitLevelField = new IntegerField("Unit Level");
            unitLevelField.bindingPath = "data.unitLevel";
            unitLevelField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue <= 0)
                {
                    unitLevelField.SetValueWithoutNotify(1);
                    target.data.unitLevel = 1;
                }
                else if (x.newValue > SkillTree.Settings.maxUnitLevel)
                {
                    unitLevelField.SetValueWithoutNotify(SkillTree.Settings.maxUnitLevel);
                    target.data.unitLevel = SkillTree.Settings.maxUnitLevel;
                }
            });
            inspector.Add(unitLevelField);

            Label label = new Label("<b>Stats</b>");
            label.style.marginTop = 4;
            inspector.Add(label);

            statsContainer = new VisualElement();
            inspector.Add(statsContainer);

            FillStatsContainer();

            VisualElement buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            inspector.Add(buttonRow);

            Button addStat = new Button();
            addStat.clicked += () =>
            {
                target.data.AddStat(new Stat());
                EditorUtility.SetDirty(target);
                FillStatsContainer();
            };
            addStat.text = "Add";
            buttonRow.Add(addStat);

            Button removeStat = new Button();
            removeStat.clicked += () =>
            {
                if (selectedStat == -1)
                {
                    target.data.RemoveStat(target.data.stats.Count - 1);
                }
                else
                {
                    target.data.RemoveStat(selectedStat);
                }

                EditorUtility.SetDirty(target);
                FillStatsContainer();
                selectedStat = -1;
            };
            removeStat.text = "Remove";
            buttonRow.Add(removeStat);

            return inspector;
        }

        private void FillStatsContainer()
        {
            statsContainer.Clear();
            var target = (UnitStatsProvider)serializedObject.targetObject;

            for (int i = 0; i < target.data.stats.Count; i++)
            {
                int index = i;
                var stat = target.data.stats[index];

                var button = new ToolbarButton();
                button.style.borderTopWidth = 1;
                button.style.borderBottomWidth = 1;
                button.style.marginBottom = 4;
                button.clicked += () =>
                {
                    selectedStat = index;
                };
                statsContainer.Add(button);

                var serializedProperty = serializedObject.FindProperty("data").FindPropertyRelative("stats").GetArrayElementAtIndex(index);

                var statField = new ObjectField();
                statField.objectType = typeof(StatIdentity);

                if (!stat.identity)
                {
                    stat.identity = SkillTreeUtility.GetDefaultStatIdentity();
                    EditorUtility.SetDirty(target);
                }

                stat.SetIdentity(stat.identity);

                statField.BindProperty(serializedProperty.FindPropertyRelative("identity"));
                button.Add(statField);

                var initialFloatField = new FloatField("Initial Value");
                initialFloatField.BindProperty(serializedProperty.FindPropertyRelative("initialValue").FindPropertyRelative("value").FindPropertyRelative("floatValue"));
                button.Add(initialFloatField);

                var scalingFloatField = new FloatField("Scaling");
                scalingFloatField.BindProperty(serializedProperty.FindPropertyRelative("scaling").FindPropertyRelative("value").FindPropertyRelative("floatValue"));
                button.Add(scalingFloatField);

                var initialIntField = new IntegerField("Initial Value");
                initialIntField.BindProperty(serializedProperty.FindPropertyRelative("initialValue").FindPropertyRelative("value").FindPropertyRelative("integerValue"));
                button.Add(initialIntField);

                var scalingIntField = new IntegerField("Scaling");
                scalingIntField.BindProperty(serializedProperty.FindPropertyRelative("scaling").FindPropertyRelative("value").FindPropertyRelative("integerValue"));
                button.Add(scalingIntField);

                var combineField = new EnumField("Combine Type", stat.combineType);
                combineField.BindProperty(serializedProperty.FindPropertyRelative("combineType"));
                button.Add(combineField);

                var operatorField = new EnumField("Operator", stat.combineOperator);
                operatorField.BindProperty(serializedProperty.FindPropertyRelative("combineOperator"));
                button.Add(operatorField);

                if (stat.identity.numericType == StatValue.NumericType.Float)
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
            }
        }
    }
}
#endif