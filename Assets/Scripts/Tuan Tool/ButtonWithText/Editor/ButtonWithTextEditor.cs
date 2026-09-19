#if UNITY_EDITOR
using TuanTool;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ButtonSyncWithText), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Button Component.
    ///   Extend this class to write a custom editor for a component derived from Button.
    /// </summary>
    public class ButtonWithTextEditor : SelectableEditor
    {
        SerializedProperty m_OnClickProperty;
        SerializedProperty m_TextProperty;
        SerializedProperty m_colorText;
        SerializedProperty m_CanvasGroup;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            m_TextProperty = serializedObject.FindProperty("m_Text");
            //m_colorText = serializedObject.FindProperty("m_colorText");
            m_CanvasGroup = serializedObject.FindProperty("m_CanvasGroup");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_OnClickProperty);
            //EditorGUILayout.PropertyField(m_TextProperty);
            //EditorGUILayout.PropertyField(m_colorText);
            EditorGUILayout.PropertyField(m_CanvasGroup);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif