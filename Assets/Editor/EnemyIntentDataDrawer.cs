using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyIntentData))]
public class EnemyIntentDataDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 4f;
    private const float ButtonHeight = 22f;
    private const float HelpBoxHeight = 36f;

    // These MUST match the exact variable names in EnemyIntentData.cs
    private const string IntentPropertyName = "enemyIntent";
    private const string ConfigPropertyName = "enemyIntentConfig";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty intentProp = property.FindPropertyRelative(IntentPropertyName);
        SerializedProperty configProp = property.FindPropertyRelative(ConfigPropertyName);

        // Failsafe in case the property names don't match
        if (intentProp == null || configProp == null)
        {
            EditorGUI.HelpBox(position, $"Error: Could not find '{IntentPropertyName}' or '{ConfigPropertyName}'. Check variable names.", MessageType.Error);
            EditorGUI.EndProperty();
            return;
        }

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float y = foldoutRect.yMax + VerticalSpacing;

            DrawIntentSection(position, ref y, intentProp, configProp);
            DrawConfigSection(position, ref y, intentProp, configProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawIntentSection(Rect position, ref float y, SerializedProperty intentProp, SerializedProperty configProp)
    {
        Rect labelRect = new Rect(position.x, y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, "Intent Type");

        Rect dropdownRect = new Rect(position.x + EditorGUIUtility.labelWidth, y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        
        // Safely get the name of the current intent, or "None" if null
        string currentIntentName = intentProp.managedReferenceValue != null ? intentProp.managedReferenceValue.GetType().Name : "None";

        // --- THIS IS THE FIXED LINE ---
        // We use your specific SerializeReferenceTypePicker class and its DrawTypePickerButton method.
        // This method automatically handles drawing the button and opening the dropdown menu!
        SerializeReferenceTypePicker.DrawTypePickerButton(dropdownRect, intentProp, typeof(BaseEnemyIntent), currentIntentName);

        y += EditorGUIUtility.singleLineHeight + VerticalSpacing;

        // Make sure the config matches the newly selected intent
        EnsureMatchingConfig(intentProp, configProp);
    }

    private void DrawConfigSection(Rect position, ref float y, SerializedProperty intentProp, SerializedProperty configProp)
    {
        // Only try to draw the config if the intent actually exists!
        if (intentProp.managedReferenceValue == null) return;

        Type requiredConfigType = GetRequiredConfigType(intentProp);

        if (requiredConfigType == null)
        {
            Rect helpRect = new Rect(position.x, y, position.width, HelpBoxHeight);
            EditorGUI.HelpBox(helpRect, "This Intent does not require configuration.", MessageType.Info);
            y += HelpBoxHeight + VerticalSpacing;
            return;
        }

        if (!IsValidPair(intentProp, configProp))
        {
            Rect helpRect = new Rect(position.x, y, position.width, HelpBoxHeight);
            EditorGUI.HelpBox(helpRect, $"Config mismatch! Expected {requiredConfigType.Name}.", MessageType.Warning);
            y += HelpBoxHeight + VerticalSpacing;
            return;
        }

        // Draw the config fields dynamically
        if (configProp.managedReferenceValue != null)
        {
            EditorGUI.LabelField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Configuration", EditorStyles.boldLabel);
            y += EditorGUIUtility.singleLineHeight + VerticalSpacing;

            // Loop through all fields inside the config object
            SerializedProperty iterator = configProp.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;
                float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
                Rect propRect = new Rect(position.x, y, position.width, propertyHeight);
                EditorGUI.PropertyField(propRect, iterator, true);
                y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        SerializedProperty intentProp = property.FindPropertyRelative(IntentPropertyName);
        SerializedProperty configProp = property.FindPropertyRelative(ConfigPropertyName);

        if (intentProp == null || configProp == null) return EditorGUIUtility.singleLineHeight * 2;

        float height = EditorGUIUtility.singleLineHeight + VerticalSpacing; // Foldout
        height += EditorGUIUtility.singleLineHeight + VerticalSpacing; // Dropdown

        // Only calculate config height if the intent isn't null!
        if (intentProp.managedReferenceValue != null)
        {
            Type requiredConfigType = GetRequiredConfigType(intentProp);

            if (requiredConfigType == null || !IsValidPair(intentProp, configProp))
            {
                height += HelpBoxHeight + VerticalSpacing;
            }
            else if (configProp.managedReferenceValue != null)
            {
                height += EditorGUIUtility.singleLineHeight + VerticalSpacing; // "Configuration" Label
                
                SerializedProperty iterator = configProp.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
                {
                    enterChildren = false;
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }

        return height;
    }

    private static Type GetRequiredConfigType(SerializedProperty intentProp)
    {
        if (intentProp.managedReferenceValue is not BaseEnemyIntent intent)
            return null;

        return intent.RequiredConfigType;
    }

    private static bool IsValidPair(SerializedProperty intentProp, SerializedProperty configProp)
    {
        if (intentProp.managedReferenceValue is not BaseEnemyIntent intent)
            return true;

        object configObj = configProp.managedReferenceValue;
        if (configObj == null)
            return true;

        if (intent.RequiredConfigType == null)
            return false;

        return intent.RequiredConfigType.IsInstanceOfType(configObj);
    }

    private static void EnsureMatchingConfig(SerializedProperty intentProp, SerializedProperty configProp)
    {
        if (intentProp.managedReferenceValue is not BaseEnemyIntent intent)
        {
            configProp.managedReferenceValue = null;
            return;
        }

        Type requiredConfigType = intent.RequiredConfigType;

        if (requiredConfigType == null)
        {
            configProp.managedReferenceValue = null;
            return;
        }

        if (configProp.managedReferenceValue == null ||
            !requiredConfigType.IsInstanceOfType(configProp.managedReferenceValue))
        {
            // Auto-instantiate the correct config class when the intent changes
            configProp.managedReferenceValue = Activator.CreateInstance(requiredConfigType);
        }
    }
}