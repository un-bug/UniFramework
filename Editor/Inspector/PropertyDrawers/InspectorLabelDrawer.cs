using UnityEditor;
using UnityEngine;

namespace UniFramework.Editor
{
    [CustomPropertyDrawer(typeof(LabelAttribute))]
    public sealed class InspectorLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, CreateLabel(label), includeChildren: true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, CreateLabel(label), includeChildren: true);
        }

        private GUIContent CreateLabel(GUIContent originalLabel)
        {
            LabelAttribute labelAttribute = (LabelAttribute)attribute;
            return new GUIContent(labelAttribute.Label, originalLabel.image, originalLabel.tooltip);
        }
    }
}