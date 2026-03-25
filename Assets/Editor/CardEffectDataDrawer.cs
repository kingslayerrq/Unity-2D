using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CardEffectData))]
public class CardEffectDataDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 4f;
    private const float ButtonHeight = 22f;
    private const float MiniButtonWidth = 60f;
    private const float MiniButtonGap = 4f;
    private const float HelpBoxHeight = 36f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty effectProp = property.FindPropertyRelative("cardEffect");
        SerializedProperty configProp = property.FindPropertyRelative("cardEffectConfig");

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = foldoutRect.yMax + VerticalSpacing;

            DrawEffectSection(position, ref y, effectProp, configProp);
            DrawConfigSection(position, ref y, effectProp, configProp);
            DrawValidation(position, ref y, effectProp, configProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
            return height;

        SerializedProperty effectProp = property.FindPropertyRelative("cardEffect");
        SerializedProperty configProp = property.FindPropertyRelative("cardEffectConfig");

        height += VerticalSpacing;

        // Effect section
        height += EditorGUIUtility.singleLineHeight;
        height += VerticalSpacing;

        // Config section
        Type requiredConfigType = GetRequiredConfigType(effectProp);

        if (effectProp.managedReferenceValue == null)
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
        if (effectProp.managedReferenceValue != null &&
            configProp.managedReferenceValue != null &&
            !IsValidPair(effectProp, configProp))
        {
            height += VerticalSpacing + HelpBoxHeight;
        }

        return height;
    }

    private void DrawEffectSection(Rect position, ref float y, SerializedProperty effectProp, SerializedProperty configProp)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        if (effectProp.managedReferenceValue == null)
        {
            Rect buttonRect = new Rect(position.x, y, position.width, ButtonHeight);
            SerializeReferenceTypePicker.DrawTypePickerButton(
                buttonRect,
                effectProp,
                typeof(BaseCardEffect),
                "Create Effect",
                (pickedEffectProp, pickedType) =>
                {
                    SerializedProperty matchedConfigProp =
                        pickedEffectProp.serializedObject.FindProperty(configProp.propertyPath);

                    if (matchedConfigProp != null)
                        EnsureMatchingConfig(pickedEffectProp, matchedConfigProp);
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

        string effectName = effectProp.managedReferenceValue.GetType().Name;
        EditorGUI.LabelField(labelRect, "Effect", effectName);

        SerializeReferenceTypePicker.DrawTypePickerButton(
            changeRect,
            effectProp,
            typeof(BaseCardEffect),
            "Change",
            (pickedEffectProp, pickedType) =>
            {
                SerializedProperty matchedConfigProp =
                    pickedEffectProp.serializedObject.FindProperty(configProp.propertyPath);

                if (matchedConfigProp != null)
                    EnsureMatchingConfig(pickedEffectProp, matchedConfigProp);
            });

        if (GUI.Button(clearRect, "Clear"))
        {
            effectProp.serializedObject.Update();
            effectProp.managedReferenceValue = null;
            configProp.managedReferenceValue = null;
            effectProp.serializedObject.ApplyModifiedProperties();
        }

        y = labelRect.yMax + VerticalSpacing;
    }

    private void DrawConfigSection(Rect position, ref float y, SerializedProperty effectProp, SerializedProperty configProp)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        if (effectProp.managedReferenceValue == null)
        {
            Rect infoRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.LabelField(infoRect, "Config", "Select an effect first");
            y = infoRect.yMax + VerticalSpacing;
            return;
        }

        Type requiredConfigType = GetRequiredConfigType(effectProp);

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

    private void DrawValidation(Rect position, ref float y, SerializedProperty effectProp, SerializedProperty configProp)
    {
        if (effectProp.managedReferenceValue == null || configProp.managedReferenceValue == null)
            return;

        if (IsValidPair(effectProp, configProp))
            return;

        Rect helpRect = new Rect(position.x, y, position.width, HelpBoxHeight);
        EditorGUI.HelpBox(helpRect, "Effect and config types do not match.", MessageType.Warning);
        y = helpRect.yMax + VerticalSpacing;
    }

    private static Type GetRequiredConfigType(SerializedProperty effectProp)
    {
        if (effectProp.managedReferenceValue is not BaseCardEffect effect)
            return null;

        return effect.RequiredConfigType;
    }

    private static bool IsValidPair(SerializedProperty effectProp, SerializedProperty configProp)
    {
        if (effectProp.managedReferenceValue is not BaseCardEffect effect)
            return true;

        object configObj = configProp.managedReferenceValue;
        if (configObj == null)
            return true;

        if (effect.RequiredConfigType == null)
            return false;

        return effect.RequiredConfigType.IsInstanceOfType(configObj);
    }

    private static void EnsureMatchingConfig(SerializedProperty effectProp, SerializedProperty configProp)
    {
        if (effectProp.managedReferenceValue is not BaseCardEffect effect)
        {
            configProp.managedReferenceValue = null;
            return;
        }

        Type requiredConfigType = effect.RequiredConfigType;

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