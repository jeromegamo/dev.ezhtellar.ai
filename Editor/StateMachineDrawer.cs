using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Ezhtellar.AI.Editor
{
    [CustomPropertyDrawer(typeof(StateMachine))]
    public class StateMachineDrawer : PropertyDrawer
    {
        private StateMachine m_instance;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetMachineInstance(property);
            if (m_instance != null && m_instance.ActiveChild != null)
            {
                var path = m_instance.PrintActivePath();
                EditorGUILayout.LabelField(label, new GUIContent(path));
            }
            else
            {
                EditorGUILayout.LabelField(label, new GUIContent("No active state"));
            }
        }

        private void SetMachineInstance(SerializedProperty property)
        {
            if (m_instance != null)
            {
                return;
            }

            object target = property.serializedObject.targetObject;
            FieldInfo field = target.GetType().GetField(property.propertyPath,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                object value = field.GetValue(target);

                if (value is StateMachine stateMachine)
                {
                    m_instance = stateMachine;
                }
            }
        }
    }
}