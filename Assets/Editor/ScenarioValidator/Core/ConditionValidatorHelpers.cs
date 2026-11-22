using System;
using System.Reflection;
using UnityEngine;

namespace _Core.Editor.ScenarioValidator
{
    internal static class ConditionValidatorHelpers
    {
        public static object GetFieldValue(object instance, string name)
        {
            if (instance == null) return null;
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance);
            }
            return null;
        }

        public static void RequireReference(ValidationContext context, ConditionEntry entry,
            string fieldName, string readableName, ValidationSeverity severity)
        {
            if (GetFieldValue(entry.Condition, fieldName) == null)
                context.Add("COND034", entry.Location + ": не назначена ссылка " + readableName + ".", severity, entry.Target);
        }

        public static void ValidateEnumField(ValidationContext context, ConditionEntry entry,
            string fieldName, string readableName, Type enumType, string code)
        {
            object value = GetFieldValue(entry.Condition, fieldName);
            if (value == null || !Enum.IsDefined(enumType, value))
                context.Add(code, entry.Location + ": неизвестное значение поля " + readableName + ".", ValidationSeverity.Error, entry.Target);
        }

        public static void ValidateNonNegativeNumber(ValidationContext context, ConditionEntry entry,
            string fieldName, string readableName, string code)
        {
            object value = GetFieldValue(entry.Condition, fieldName);
            if (value is IConvertible convertible)
            {
                try
                {
                    if (convertible.ToDouble(null) < 0d)
                        context.Add(code, entry.Location + ": " + readableName + " не может быть отрицательным.", ValidationSeverity.Error, entry.Target);
                }
                catch (Exception) { }
            }
        }
    }
}
