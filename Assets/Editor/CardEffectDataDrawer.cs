using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CardEffectData))]
public class CardEffectDataDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 4f;
    private const float ButtonHeight = 22f;
    private const float MiniButtonWidth = 60f;
    private const float MiniButtonGap = 4f;
    private const float MoveButtonWidth = 24f;
    private const float AddButtonWidth = 60f;
    private const float HelpBoxHeight = 60f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty triggerProp = property.FindPropertyRelative("trigger");
        SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
        SerializedProperty effectProp = property.FindPropertyRelative("cardEffect");
        SerializedProperty configProp = property.FindPropertyRelative("cardEffectConfig");

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = foldoutRect.yMax + VerticalSpacing;

            DrawTriggerSection(position, ref y, triggerProp);
            DrawConditionsSection(position, ref y, conditionsProp);
            DrawEffectSection(position, ref y, effectProp, configProp);
            DrawConfigSection(position, ref y, effectProp, configProp);
            DrawValidation(position, ref y, triggerProp, conditionsProp, effectProp, configProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
            return height;

        SerializedProperty triggerProp = property.FindPropertyRelative("trigger");
        SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
        SerializedProperty effectProp = property.FindPropertyRelative("cardEffect");
        SerializedProperty configProp = property.FindPropertyRelative("cardEffectConfig");

        height += VerticalSpacing;

        // Trigger section
        height += EditorGUI.GetPropertyHeight(triggerProp);
        height += VerticalSpacing;

        // Conditions section
        height += GetConditionsHeight(conditionsProp);

        // Effect section
        height += effectProp.managedReferenceValue == null
            ? ButtonHeight
            : EditorGUIUtility.singleLineHeight;
        height += VerticalSpacing;

        // Config section
        height += GetConfigHeight(effectProp, configProp);
        height += VerticalSpacing;

        // Validation
        List<string> validationMessages =
            BuildValidationMessages(triggerProp, conditionsProp, effectProp, configProp);

        for (int i = 0; i < validationMessages.Count; i++)
        {
            height += HelpBoxHeight + VerticalSpacing;
        }

        return height;
    }

    private static void DrawTriggerSection(
        Rect position,
        ref float y,
        SerializedProperty triggerProp)
    {
        float height = EditorGUI.GetPropertyHeight(triggerProp);
        Rect triggerRect = new Rect(position.x, y, position.width, height);

        EditorGUI.PropertyField(triggerRect, triggerProp, new GUIContent("Trigger"));
        y = triggerRect.yMax + VerticalSpacing;
    }

    private void DrawConditionsSection(
        Rect position,
        ref float y,
        SerializedProperty conditionsProp)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        Rect labelRect = new Rect(
            position.x,
            y,
            position.width - AddButtonWidth - MiniButtonGap,
            lineHeight);

        Rect addRect = new Rect(
            labelRect.xMax + MiniButtonGap,
            y,
            AddButtonWidth,
            lineHeight);

        EditorGUI.LabelField(
            labelRect,
            $"Conditions (all must pass): {conditionsProp.arraySize}");

        if (GUI.Button(addRect, "Add"))
            AddCondition(conditionsProp);

        y = labelRect.yMax + VerticalSpacing;

        for (int i = 0; i < conditionsProp.arraySize; i++)
        {
            SerializedProperty conditionProp = conditionsProp.GetArrayElementAtIndex(i);

            float conditionHeight = conditionProp.managedReferenceValue == null
                ? ButtonHeight
                : Mathf.Max(
                    lineHeight,
                    EditorGUI.GetPropertyHeight(conditionProp, true));

            float controlsWidth =
                MoveButtonWidth * 2f +
                MiniButtonWidth * 2f +
                MiniButtonGap * 4f;

            Rect conditionRect = new Rect(
                position.x,
                y,
                Mathf.Max(0f, position.width - controlsWidth),
                conditionHeight);

            Rect upRect = new Rect(
                conditionRect.xMax + MiniButtonGap,
                y,
                MoveButtonWidth,
                lineHeight);

            Rect downRect = new Rect(
                upRect.xMax + MiniButtonGap,
                y,
                MoveButtonWidth,
                lineHeight);

            Rect changeRect = new Rect(
                downRect.xMax + MiniButtonGap,
                y,
                MiniButtonWidth,
                lineHeight);

            Rect removeRect = new Rect(
                changeRect.xMax + MiniButtonGap,
                y,
                MiniButtonWidth,
                lineHeight);

            if (conditionProp.managedReferenceValue == null)
            {
                SerializeReferenceTypePicker.DrawTypePickerButton(
                    conditionRect,
                    conditionProp,
                    typeof(BaseCardCondition),
                    "Select Condition");
            }
            else
            {
                string conditionName = ObjectNames.NicifyVariableName(
                    conditionProp.managedReferenceValue.GetType().Name);

                EditorGUI.PropertyField(
                    conditionRect,
                    conditionProp,
                    new GUIContent($"{i + 1}. {conditionName}"),
                    true);
            }

            EditorGUI.BeginDisabledGroup(i == 0);
            if (GUI.Button(upRect, "↑"))
                MoveCondition(conditionsProp, i, i - 1);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(i >= conditionsProp.arraySize - 1);
            if (GUI.Button(downRect, "↓"))
                MoveCondition(conditionsProp, i, i + 1);
            EditorGUI.EndDisabledGroup();

            SerializeReferenceTypePicker.DrawTypePickerButton(
                changeRect,
                conditionProp,
                typeof(BaseCardCondition),
                "Change");

            if (GUI.Button(removeRect, "Remove"))
                RemoveCondition(conditionsProp, i);

            y = conditionRect.yMax + VerticalSpacing;
        }
    }

    private static float GetConditionsHeight(SerializedProperty conditionsProp)
    {
        float height = EditorGUIUtility.singleLineHeight + VerticalSpacing;

        for (int i = 0; i < conditionsProp.arraySize; i++)
        {
            SerializedProperty conditionProp = conditionsProp.GetArrayElementAtIndex(i);

            height += conditionProp.managedReferenceValue == null
                ? ButtonHeight
                : Mathf.Max(
                    EditorGUIUtility.singleLineHeight,
                    EditorGUI.GetPropertyHeight(conditionProp, true));

            height += VerticalSpacing;
        }

        return height;
    }

    private static void AddCondition(SerializedProperty conditionsProp)
    {
        SerializedObject serializedObject = conditionsProp.serializedObject;
        string propertyPath = conditionsProp.propertyPath;

        serializedObject.Update();

        SerializedProperty liveConditionsProp =
            serializedObject.FindProperty(propertyPath);

        int index = liveConditionsProp.arraySize;
        liveConditionsProp.InsertArrayElementAtIndex(index);
        liveConditionsProp.GetArrayElementAtIndex(index).managedReferenceValue = null;

        serializedObject.ApplyModifiedProperties();
        GUIUtility.ExitGUI();
    }

    private static void MoveCondition(
        SerializedProperty conditionsProp,
        int fromIndex,
        int toIndex)
    {
        SerializedObject serializedObject = conditionsProp.serializedObject;
        string propertyPath = conditionsProp.propertyPath;

        serializedObject.Update();

        SerializedProperty liveConditionsProp =
            serializedObject.FindProperty(propertyPath);

        liveConditionsProp.MoveArrayElement(fromIndex, toIndex);

        serializedObject.ApplyModifiedProperties();
        GUIUtility.ExitGUI();
    }

    private static void RemoveCondition(
        SerializedProperty conditionsProp,
        int index)
    {
        SerializedObject serializedObject = conditionsProp.serializedObject;
        string propertyPath = conditionsProp.propertyPath;

        serializedObject.Update();

        SerializedProperty liveConditionsProp =
            serializedObject.FindProperty(propertyPath);

        int oldSize = liveConditionsProp.arraySize;
        liveConditionsProp.DeleteArrayElementAtIndex(index);

        // Some Unity reference arrays clear an element before removing it.
        if (liveConditionsProp.arraySize == oldSize)
            liveConditionsProp.DeleteArrayElementAtIndex(index);

        serializedObject.ApplyModifiedProperties();
        GUIUtility.ExitGUI();
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

    private static float GetConfigHeight(
        SerializedProperty effectProp,
        SerializedProperty configProp)
    {
        if (effectProp.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight;

        Type requiredConfigType = GetRequiredConfigType(effectProp);
        if (requiredConfigType == null)
            return EditorGUIUtility.singleLineHeight;

        bool hasValidConfig =
            configProp.managedReferenceValue != null &&
            requiredConfigType.IsInstanceOfType(configProp.managedReferenceValue);

        return hasValidConfig
            ? EditorGUI.GetPropertyHeight(configProp, true)
            : ButtonHeight;
    }

    private static void DrawValidation(
        Rect position,
        ref float y,
        SerializedProperty triggerProp,
        SerializedProperty conditionsProp,
        SerializedProperty effectProp,
        SerializedProperty configProp)
    {
        List<string> validationMessages =
            BuildValidationMessages(triggerProp, conditionsProp, effectProp, configProp);

        for (int i = 0; i < validationMessages.Count; i++)
        {
            Rect helpRect = new Rect(
                position.x,
                y,
                position.width,
                HelpBoxHeight);

            EditorGUI.HelpBox(
                helpRect,
                validationMessages[i],
                MessageType.Warning);

            y = helpRect.yMax + VerticalSpacing;
        }
    }

    private static List<string> BuildValidationMessages(
        SerializedProperty triggerProp,
        SerializedProperty conditionsProp,
        SerializedProperty effectProp,
        SerializedProperty configProp)
    {
        var messages = new List<string>();

        if (effectProp.managedReferenceValue != null &&
            configProp.managedReferenceValue != null &&
            !IsValidPair(effectProp, configProp))
        {
            messages.Add("Effect and config types do not match.");
        }

        for (int i = 0; i < conditionsProp.arraySize; i++)
        {
            SerializedProperty conditionProp =
                conditionsProp.GetArrayElementAtIndex(i);

            if (conditionProp.managedReferenceValue == null)
                messages.Add($"Condition {i + 1} has no type selected.");
        }

        if (IsConditionalSelfExhaust(
                triggerProp,
                conditionsProp,
                effectProp,
                configProp))
        {
            messages.Add(
                "Conditional self-exhaust is not supported. The card's post-play " +
                "destination is selected before conditioned effects resolve. " +
                "Use an unconditional This Card exhaust rule.");
        }

        return messages;
    }

    private static bool IsConditionalSelfExhaust(
        SerializedProperty triggerProp,
        SerializedProperty conditionsProp,
        SerializedProperty effectProp,
        SerializedProperty configProp)
    {
        return (CardEffectTrigger)triggerProp.intValue == CardEffectTrigger.OnPlay &&
               conditionsProp.arraySize > 0 &&
               effectProp.managedReferenceValue is ExhaustCardEffect &&
               configProp.managedReferenceValue is ExhaustCardEffectConfig exhaustConfig &&
               exhaustConfig.ExhaustMode == ExhaustMode.ThisCard;
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
