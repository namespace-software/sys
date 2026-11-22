using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using _Core._ScenarioSystem;
using _Core._ScenarioSystem.Component.Condition;
using UnityEngine;

namespace _Core.Editor.ScenarioValidator
{
    /// <summary>Статические проверки сериализованных условий. Не вызывает SetupCondition/InitCondition.</summary>
    public sealed class ConditionValidator
    {
        private static readonly FieldInfo DependenciesField = typeof(ConditionBase).GetField(
            "_dependingConditions", BindingFlags.Instance | BindingFlags.NonPublic);

        public void Validate(ValidationContext context)
        {
            HashSet<ConditionBase> known = new HashSet<ConditionBase>();
            foreach (ConditionEntry entry in context.Conditions)
            {
                ConditionBase condition = entry.Condition;
                if (condition == null) continue;
                known.Add(condition);
                ValidateCommon(context, entry);
                ValidateSpecific(context, entry);
            }

            ValidateDuplicates(context);
            ValidateDependencyGraph(context, known);
        }

        private static void ValidateCommon(ValidationContext context, ConditionEntry entry)
        {
            ConditionBase condition = entry.Condition;
            UnityEngine.Object target = entry.Target;

            if (condition.startInstructions == null)
                context.Add("COND010", entry.Location + ": не задан startInstructions. InitCondition обращается к нему без null-проверки.", ValidationSeverity.Error, target);
            else if (string.IsNullOrWhiteSpace(condition.startInstructions.instructionText))
                context.Add("COND011", entry.Location + ": пустой текст стартовой инструкции.", ValidationSeverity.Warning, target);

            if (condition.mistakeInstructions == null)
                context.Add("COND012", entry.Location + ": не задан mistakeInstructions; LogSystem использует это поле.", ValidationSeverity.Error, target);
            if (!condition.isPermanent && condition.successInstructions == null)
                context.Add("COND013", entry.Location + ": не задан successInstructions.", ValidationSeverity.Warning, target);

            if (!Enum.IsDefined(typeof(_Core._ScenarioSystem.Component.ServiceType), condition.ServiceType))
                context.Add("COND014", entry.Location + ": неизвестное значение ServiceType.", ValidationSeverity.Error, target);
            if (!Enum.IsDefined(typeof(EnumMistakeType), condition.mistakeType))
                context.Add("COND015", entry.Location + ": неизвестное значение mistakeType.", ValidationSeverity.Warning, target);

            IList dependencies = GetDependencies(condition);
            if (condition.hasDependencies && (dependencies == null || dependencies.Count == 0))
                context.Add("COND016", entry.Location + ": hasDependencies включён, но список зависимостей пуст или null.", ValidationSeverity.Error, target);

            if (dependencies == null) return;
            HashSet<ConditionBase> unique = new HashSet<ConditionBase>();
            for (int i = 0; i < dependencies.Count; i++)
            {
                ConditionBase dependency = dependencies[i] as ConditionBase;
                if (dependency == null)
                    context.Add("COND017", entry.Location + ": зависимость с индексом " + i + " равна null.", ValidationSeverity.Error, target);
                else if (ReferenceEquals(dependency, condition))
                    context.Add("COND018", entry.Location + ": условие зависит само от себя.", ValidationSeverity.Error, target);
                else if (!unique.Add(dependency))
                    context.Add("COND019", entry.Location + ": одна и та же зависимость указана несколько раз.", ValidationSeverity.Warning, target);
            }
        }

        private static void ValidateSpecific(ValidationContext context, ConditionEntry entry)
        {
            ConditionBase condition = entry.Condition;
            UnityEngine.Object target = entry.Target;
            Type type = condition.GetType();

            if (type.Name == "ButtonCondition")
                RequireReference(context, entry, "_button", "Button", ValidationSeverity.Error);
            else if (type.Name == "AwaitCondition")
                RequireReference(context, entry, "_step", "StepBase для ожидания", ValidationSeverity.Error);
            else if (type.Name == "GrabCondition")
                RequireReference(context, entry, "_hVR_InteractionHelper", "HVR_InteractionHelper", ValidationSeverity.Error);
            else if (type.Name == "TriggerCondition")
            {
                RequireReference(context, entry, "_collider", "Collider", ValidationSeverity.Error);
                IList tags = GetFieldValue(condition, "_tags") as IList;
                if (tags == null || tags.Count == 0)
                    context.Add("COND030", entry.Location + ": TriggerCondition не содержит ни одного тега; условие не сможет совпасть с объектом.", ValidationSeverity.Warning, target);
            }
            else if (type.Name == "PositionCondition")
            {
                RequireReference(context, entry, "itemTransform", "Transform объекта", ValidationSeverity.Error);
                RequireReference(context, entry, "targetTransform", "Transform цели", ValidationSeverity.Error);
                object distance = GetFieldValue(condition, "_distance");
                if (distance is float value && value < 0f)
                    context.Add("COND031", entry.Location + ": дистанция PositionCondition отрицательная.", ValidationSeverity.Error, target);
            }
            else if (type.Name == "EventCondition")
            {
                object provider = GetFieldValue(condition, "_stateProvider");
                if (!(provider is MonoBehaviour behaviour))
                    context.Add("COND032", entry.Location + ": не назначен MonoBehaviour-поставщик события.", ValidationSeverity.Error, target);
                else
                {
                    bool implementsProvider = false;
                    foreach (Type implemented in behaviour.GetType().GetInterfaces())
                        if (implemented.Name == "IBooleanStateProvider") { implementsProvider = true; break; }
                    if (!implementsProvider)
                        context.Add("COND033", entry.Location + ": компонент _stateProvider не реализует IBooleanStateProvider.", ValidationSeverity.Error, target);
                }
            }
        }

        private static void RequireReference(ValidationContext context, ConditionEntry entry, string fieldName,
            string readableName, ValidationSeverity severity)
        {
            if (GetFieldValue(entry.Condition, fieldName) == null)
                context.Add("COND034", entry.Location + ": не назначена ссылка " + readableName + ".", severity, entry.Target);
        }

        private static object GetFieldValue(object instance, string name)
        {
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance);
            }
            return null;
        }

        private static IList GetDependencies(ConditionBase condition)
        {
            return DependenciesField != null ? DependenciesField.GetValue(condition) as IList : null;
        }

        private static void ValidateDuplicates(ValidationContext context)
        {
            Dictionary<ConditionBase, ConditionEntry> first = new Dictionary<ConditionBase, ConditionEntry>();
            foreach (ConditionEntry entry in context.Conditions)
            {
                if (first.TryGetValue(entry.Condition, out ConditionEntry previous))
                {
                    if (previous.Target == entry.Target)
                        context.Add("COND040", entry.Location + ": один экземпляр ConditionBase добавлен в список повторно.", ValidationSeverity.Warning, entry.Target);
                }
                else first.Add(entry.Condition, entry);
            }
        }

        private static void ValidateDependencyGraph(ValidationContext context, HashSet<ConditionBase> known)
        {
            Dictionary<ConditionBase, int> state = new Dictionary<ConditionBase, int>();
            HashSet<ConditionBase> reported = new HashSet<ConditionBase>();

            foreach (ConditionBase condition in known)
                Visit(condition, context, state, reported);
        }

        private static void Visit(ConditionBase condition, ValidationContext context,
            Dictionary<ConditionBase, int> state, HashSet<ConditionBase> reported)
        {
            if (condition == null) return;
            if (state.TryGetValue(condition, out int current))
            {
                if (current == 1 && reported.Add(condition))
                {
                    ConditionEntry entry = FindEntry(context, condition);
                    context.Add("COND041", "Обнаружен цикл в графе зависимостей ConditionBase.", ValidationSeverity.Error,
                        entry != null ? entry.Target : null);
                }
                return;
            }

            state[condition] = 1;
            IList dependencies = GetDependencies(condition);
            if (dependencies != null)
                foreach (object item in dependencies)
                    if (item is ConditionBase dependency)
                    {
                        if (FindEntry(context, dependency) == null)
                        {
                            ConditionEntry owner = FindEntry(context, condition);
                            context.Add("COND042", "Зависимость ссылается на ConditionBase, который не найден в списках условий текущей активной сцены.", ValidationSeverity.Warning,
                                owner != null ? owner.Target : null);
                        }
                        Visit(dependency, context, state, reported);
                    }
            state[condition] = 2;
        }

        private static ConditionEntry FindEntry(ValidationContext context, ConditionBase condition)
        {
            foreach (ConditionEntry entry in context.Conditions)
                if (ReferenceEquals(entry.Condition, condition)) return entry;
            return null;
        }
    }
}