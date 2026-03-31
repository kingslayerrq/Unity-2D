using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyActionData))] // Ensure this matches your data class name
public class EnemyActionDataDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 4f;
    private const float ButtonHeight = 22f;
    private const float MiniButtonWidth = 60f;
    private const float MiniButtonGap = 4f;
    private const float HelpBoxHeight = 36f;

    // IMPORTANT: Update these strings to match the exact variable names in your EnemyActionData class
    private const string ActionPropertyName = "enemyAction";
    private const string ConfigPropertyName = "enemyActionConfig";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty actionProp = property.FindPropertyRelative(ActionPropertyName);
        SerializedProperty configProp = property.FindPropertyRelative(ConfigPropertyName);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = foldoutRect.yMax + VerticalSpacing;

            DrawActionSection(position, ref y, actionProp, configProp);
            DrawConfigSection(position, ref y, actionProp, configProp);
            DrawValidation(position, ref y, actionProp, configProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
            return height;

        SerializedProperty actionProp = property.FindPropertyRelative(ActionPropertyName);
        SerializedProperty configProp = property.FindPropertyRelative(ConfigPropertyName);

        height += VerticalSpacing;

        // Action section
        height += EditorGUIUtility.singleLineHeight;
        height += VerticalSpacing;

        // Config section
        Type requiredConfigType = GetRequiredConfigType(actionProp);

        if (actionProp.managedReferenceValue == null)
        {
            height += EditorGUIUtility.singleLineHeight;
        }
        else if (requiredConfigType == null)
        {
            height += EditorGUIUtility.singleLineHeight;
        }
        else if (configProp.managedReferenceValue == null || !requiredConfigType.IsInstanceOfType(configProp.managedReferenceValue))
        {
            height += ButtonHeight;
        }
        else
        {
            height += EditorGUI.GetPropertyHeight(configProp, true);
        }

        // Validation
        if (actionProp.managedReferenceValue != null &&
            configProp.managedReferenceValue != null &&
            !IsValidPair(actionProp, configProp))
        {
            height += VerticalSpacing + HelpBoxHeight;
        }

        return height;
    }

    private void DrawActionSection(Rect position, ref float y, SerializedProperty actionProp, SerializedProperty configProp)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        if (actionProp.managedReferenceValue == null)
        {
            Rect buttonRect = new Rect(position.x, y, position.width, ButtonHeight);
            SerializeReferenceTypePicker.DrawTypePickerButton(
                buttonRect,
                actionProp,
                typeof(BaseEnemyAction),
                "Create Action",
                (pickedActionProp, pickedType) =>
                {
                    SerializedProperty matchedConfigProp =
                        pickedActionProp.serializedObject.FindProperty(configProp.propertyPath);

                    if (matchedConfigProp != null)
                        EnsureMatchingConfig(pickedActionProp, matchedConfigProp);
                });

            y = buttonRect.yMax + VerticalSpacing;
            return;
        }

        Rect labelRect = new Rect(
            position.x,
            y,
            position.width - (MiniButtonWidth * 2f + MiniButtonGap * 2f),
            lineHeight);

        Rect changeRect = new Rect(
            labelRect.xMax + MiniButtonGap,
            y,
            MiniButtonWidth,
            lineHeight);

        Rect clearRect = new Rect(
            changeRect.xMax + MiniButtonGap,
            y,
            MiniButtonWidth,
            lineHeight);

        string actionName = actionProp.managedReferenceValue.GetType().Name;
        EditorGUI.LabelField(labelRect, "Action", actionName);

        SerializeReferenceTypePicker.DrawTypePickerButton(
            changeRect,
            actionProp,
            typeof(BaseEnemyAction),
            "Change",
            (pickedActionProp, pickedType) =>
            {
                SerializedProperty matchedConfigProp =
                    pickedActionProp.serializedObject.FindProperty(configProp.propertyPath);

                if (matchedConfigProp != null)
                    EnsureMatchingConfig(pickedActionProp, matchedConfigProp);
            });

        if (GUI.Button(clearRect, "Clear"))
        {
            actionProp.serializedObject.Update();
            actionProp.managedReferenceValue = null;
            configProp.managedReferenceValue = null;
            actionProp.serializedObject.ApplyModifiedProperties();
        }

        y = labelRect.yMax + VerticalSpacing;
    }

    private void DrawConfigSection(Rect position, ref float y, SerializedProperty actionProp, SerializedProperty configProp)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        if (actionProp.managedReferenceValue == null)
        {
            Rect infoRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.LabelField(infoRect, "Config", "Select an action first");
            y = infoRect.yMax + VerticalSpacing;
            return;
        }

        Type requiredConfigType = GetRequiredConfigType(actionProp);

        if (requiredConfigType == null)
        {
            Rect infoRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.LabelField(infoRect, "Config", "None required");
            y = infoRect.yMax + VerticalSpacing;
            return;
        }

        bool hasValidConfig =
            configProp.managedReferenceValue != null &&
            requiredConfigType.IsInstanceOfType(configProp.managedReferenceValue);

        if (!hasValidConfig)
        {
            Rect buttonRect = new Rect(position.x, y, position.width, ButtonHeight);
            string label = configProp.managedReferenceValue == null
                ? $"Create {ObjectNames.NicifyVariableName(requiredConfigType.Name)}"
                : $"Fix Config ({ObjectNames.NicifyVariableName(requiredConfigType.Name)})";

            SerializeReferenceTypePicker.DrawTypePickerButton(
                buttonRect,
                configProp,
                requiredConfigType,
                label);

            y = buttonRect.yMax + VerticalSpacing;
            return;
        }

        Rect fieldRect = new Rect(
            position.x,
            y,
            position.width - (MiniButtonWidth * 2f + MiniButtonGap * 2f),
            EditorGUI.GetPropertyHeight(configProp, true));

        Rect changeRect = new Rect(
            fieldRect.xMax + MiniButtonGap,
            y,
            MiniButtonWidth,
            lineHeight);

        Rect clearRect = new Rect(
            changeRect.xMax + MiniButtonGap,
            y,
            MiniButtonWidth,
            lineHeight);

        EditorGUI.PropertyField(fieldRect, configProp, new GUIContent("Config"), true);

        SerializeReferenceTypePicker.DrawTypePickerButton(
            changeRect,
            configProp,
            requiredConfigType,
            "Change");

        if (GUI.Button(clearRect, "Clear"))
        {
            configProp.serializedObject.Update();
            configProp.managedReferenceValue = null;
            configProp.serializedObject.ApplyModifiedProperties();
        }

        y = fieldRect.yMax + VerticalSpacing;
    }

    private void DrawValidation(Rect position, ref float y, SerializedProperty actionProp, SerializedProperty configProp)
    {
        if (actionProp.managedReferenceValue == null || configProp.managedReferenceValue == null)
            return;

        if (IsValidPair(actionProp, configProp))
            return;

        Rect helpRect = new Rect(position.x, y, position.width, HelpBoxHeight);
        EditorGUI.HelpBox(helpRect, "Action and config types do not match.", MessageType.Warning);
        y = helpRect.yMax + VerticalSpacing;
    }

    private static Type GetRequiredConfigType(SerializedProperty actionProp)
    {
        if (actionProp.managedReferenceValue is not BaseEnemyAction action)
            return null;

        return action.RequiredConfigType;
    }

    private static bool IsValidPair(SerializedProperty actionProp, SerializedProperty configProp)
    {
        if (actionProp.managedReferenceValue is not BaseEnemyAction action)
            return true;

        object configObj = configProp.managedReferenceValue;
        if (configObj == null)
            return true;

        if (action.RequiredConfigType == null)
            return false;

        return action.RequiredConfigType.IsInstanceOfType(configObj);
    }

    private static void EnsureMatchingConfig(SerializedProperty actionProp, SerializedProperty configProp)
    {
        if (actionProp.managedReferenceValue is not BaseEnemyAction action)
        {
            configProp.managedReferenceValue = null;
            return;
        }

        Type requiredConfigType = action.RequiredConfigType;

        if (requiredConfigType == null)
        {
            configProp.managedReferenceValue = null;
            return;
        }

        if (configProp.managedReferenceValue == null ||
            !requiredConfigType.IsInstanceOfType(configProp.managedReferenceValue))
        {
            configProp.managedReferenceValue = Activator.CreateInstance(requiredConfigType);
        }
    }
}