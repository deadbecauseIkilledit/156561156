//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Skill interpreter editor.
//***************************************************************************************

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.SkillTree.Editor
{
    [CustomEditor(typeof(SkillInterpreter))]
    public class SkillInterpreterEditor : UnityEditor.Editor
    {
        private VisualElement listContainer;
        private int selectedItem = -1;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new VisualElement();

            var target = (SkillInterpreter)serializedObject.targetObject;

            Label label = new Label("<b>Interpreters</b>");
            label.style.marginTop = 4;
            inspector.Add(label);

            listContainer = new VisualElement();
            inspector.Add(listContainer);

            FillStatsContainer();

            VisualElement buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            inspector.Add(buttonRow);

            Button addStat = new Button();
            addStat.clicked += () =>
            {
                var interpreter = new SkillInterpreter.SkillInterpretation();
                interpreter.onSkillUsed = new();
                interpreter.onWindupComplete = new();
                target.interpreters.Add(interpreter);
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                listContainer.Clear();
                FillStatsContainer();
            };
            addStat.text = "Add";
            buttonRow.Add(addStat);

            Button removeStat = new Button();
            removeStat.clicked += () =>
            {
                if (selectedItem == -1)
                {
                    target.interpreters.RemoveAt(target.interpreters.Count - 1);
                }
                else
                {
                    target.interpreters.RemoveAt(selectedItem);
                }

                EditorUtility.SetDirty(target);
                listContainer.Clear();
                FillStatsContainer();
                selectedItem = -1;
            };
            removeStat.text = "Remove";
            buttonRow.Add(removeStat);

            return inspector;
        }

        private void FillStatsContainer()
        {
            var target = (SkillInterpreter)serializedObject.targetObject;
            var interpretersProperty = serializedObject.FindProperty("interpreters");

            for (int i = 0; i < target.interpreters.Count; i++)
            {
                int index = i;

                var button = new ToolbarButton();
                button.style.borderTopWidth = 1;
                button.style.borderBottomWidth = 1;
                button.style.marginBottom = 4;
                button.style.paddingTop = 4;
                button.style.paddingBottom = 4;
                button.style.paddingLeft = 4;
                button.style.paddingRight = 4;
                button.clicked += () =>
                {
                    selectedItem = index;
                };
                listContainer.Add(button);

                var interpreter = target.interpreters[index];

                var skillKeyField = new TextField("Skill Key");
                skillKeyField.style.paddingBottom = 4;
                skillKeyField.SetValueWithoutNotify(target.interpreters[index].skillKey);
                skillKeyField.RegisterValueChangedCallback(x =>
                {
                    interpreter.skillKey = x.newValue;
                    target.interpreters[index] = interpreter;
                    EditorUtility.SetDirty(target);
                });
                button.Add(skillKeyField);

                var arrayElement = interpretersProperty.GetArrayElementAtIndex(index);
                var onUsedField = IMGUIField(arrayElement.FindPropertyRelative("onSkillUsed"));
                onUsedField.style.paddingBottom = 4;
                button.Add(onUsedField);

                var onWindupField = IMGUIField(arrayElement.FindPropertyRelative("onWindupComplete"));
                onWindupField.style.paddingBottom = 4;
                button.Add(onWindupField);
            }
        }

        private VisualElement IMGUIField(SerializedProperty property)
        {
            return new IMGUIContainer(() =>
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            });
        }
    }
}
#endif