using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SerializeReferenceTypePicker
{
    public static void DrawTypePickerButton(
        Rect rect,
        SerializedProperty property,
        Type baseType,
        string buttonLabel,
        Action<SerializedProperty, Type> onTypePicked = null,
        bool allowNull = false)
    {
        if (!GUI.Button(rect, buttonLabel))
            return;

        GenericMenu menu = new GenericMenu();

        SerializedObject serializedObject = property.serializedObject;
        string propertyPath = property.propertyPath;
        object currentValue = property.managedReferenceValue;

        if (allowNull)
        {
            bool isCurrentNull = currentValue == null;
            menu.AddItem(new GUIContent("None"), isCurrentNull, () =>
            {
                serializedObject.Update();

                SerializedProperty targetProp = serializedObject.FindProperty(propertyPath);
                if (targetProp == null)
                    return;

                targetProp.managedReferenceValue = null;
                onTypePicked?.Invoke(targetProp, null);

                serializedObject.ApplyModifiedProperties();
            });

            menu.AddSeparator(string.Empty);
        }

        foreach (Type type in GetConcreteSubtypes(baseType))
        {
            Type pickedType = type;
            bool isCurrentType = currentValue != null && currentValue.GetType() == pickedType;

            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(pickedType.Name)), isCurrentType, () =>
            {
                serializedObject.Update();

                SerializedProperty targetProp = serializedObject.FindProperty(propertyPath);
                if (targetProp == null)
                    return;

                targetProp.managedReferenceValue = Activator.CreateInstance(pickedType);
                onTypePicked?.Invoke(targetProp, pickedType);

                serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }

    private static IEnumerable<Type> GetConcreteSubtypes(Type baseType)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(type =>
                baseType.IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsInterface)
            .OrderBy(type => type.Name);
    }
}