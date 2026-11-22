using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using _Core._ScenarioSystem.Component.Scenario;
using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Group;
using _Core._ScenarioSystem.Component.Step;

namespace _Core.Editor.ScenarioValidator
{
    public sealed class SceneValidator
    {
        public void Validate(ValidationContext context)
        {
            ValidateRequired(context, "SCENE001", "Scenario", context.Scenarios);
            ValidateRequired(context, "SCENE002", "ScenarioAudioMediator", context.AudioMediators);
            ValidateRequired(context, "SCENE003", "ScenarioSystem", context.ScenarioSystems);
            ValidateRequired(context, "SCENE004", "HintsSystem", context.HintsSystems);
            ValidateRequired(context, "SCENE005", "MistakeSystemBase", context.MistakeSystems);
            ValidateRequired(context, "SCENE006", "LogSystem", context.LogSystems);
            ValidateRequired(context, "SCENE007", "TrackedManager", context.TrackedManagers);

            foreach (SocketFilledProvider provider in context.SocketFilledProviders)
            {
                FieldInfo socketField = provider.GetType().GetField("_socket", BindingFlags.Instance | BindingFlags.NonPublic);
                if (socketField == null || socketField.GetValue(provider) == null)
                    context.Add("PROVIDER001", "SocketFilledProvider: не назначен HVRSocket (_socket). Компонент обращается к нему в Awake().", ValidationSeverity.Error, provider);
            }

            foreach (ScenarioBase scenario in context.Scenarios)
            {
                FieldInfo permanentField = typeof(ScenarioBase).GetField("_permanentConditions",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (permanentField != null && permanentField.GetValue(scenario) == null)
                    context.Add("SCENE008", "Список _permanentConditions равен null; ScenarioBase.OnValidate обращается к нему без null-проверки.", ValidationSeverity.Error, scenario);
            }

            foreach (GroupSteps group in context.Groups)
            {
                if (string.IsNullOrWhiteSpace(group.Description))
                    context.Add("GROUP000", "Не задано описание группы (Description).", ValidationSeverity.Info, group);

                List<StepBase> directSteps = new List<StepBase>();
                int nestedSteps = 0;
                for (int i = 0; i < group.transform.childCount; i++)
                {
                    Transform child = group.transform.GetChild(i);
                    StepBase direct = child.GetComponent<StepBase>();
                    if (direct != null) directSteps.Add(direct);
                    nestedSteps += child.GetComponentsInChildren<StepBase>(true).Length - (direct != null ? 1 : 0);
                }

                if (directSteps.Count == 0)
                    context.Add("GROUP001", "Группа не содержит StepBase среди непосредственных дочерних объектов. Runtime GroupSteps собирает только прямых детей.", ValidationSeverity.Error, group);
                if (nestedSteps > 0)
                    context.Add("GROUP002", "Внутри группы найдены вложенные StepBase, которые Runtime GroupSteps не добавит в Steps.", ValidationSeverity.Warning, group);

                HashSet<int> indexes = new HashSet<int>();
                foreach (StepBase step in directSteps)
                    if (!indexes.Add(step.StepIndex))
                        context.Add("GROUP003", "В группе повторяется индекс шага " + step.StepIndex + ".", ValidationSeverity.Warning, step);

                if (group.Conditions == null)
                    context.Add("GROUP004", "Массив завершающих Conditions равен null; методы группы обращаются к нему без null-проверки.", ValidationSeverity.Error, group);
            }

            foreach (StepBase step in context.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepName))
                    context.Add("STEP001", "Не задано имя шага (StepName).", ValidationSeverity.Warning, step);
                if (!System.Enum.IsDefined(typeof(ServiceType), step.ServiceType))
                    context.Add("STEP008", "Неизвестное значение ServiceType.", ValidationSeverity.Error, step);

                if (step.startInstructions == null)
                    context.Add("STEP002", "Не задан объект startInstructions. Runtime-код может обратиться к нему без проверки null.", ValidationSeverity.Error, step);
                else if (string.IsNullOrWhiteSpace(step.startInstructions.instructionText))
                    context.Add("STEP003", "Не задан текст стартовой инструкции шага.", ValidationSeverity.Warning, step);
                if (step.successInstructions == null)
                    context.Add("STEP004", "Не задан объект successInstructions. LogSystem обращается к нему без проверки null.", ValidationSeverity.Error, step);
                if (step.mistakeInstructions == null)
                    context.Add("STEP005", "Не задан объект mistakeInstructions. LogSystem обращается к нему без проверки null.", ValidationSeverity.Error, step);
                if (step.conditions == null)
                    context.Add("STEP006", "Список conditions равен null.", ValidationSeverity.Error, step);
                else if (step.conditions.Count == 0)
                    context.Add("STEP007", "У шага нет условий. Проверьте, предусмотрено ли завершение шага без ConditionBase.", ValidationSeverity.Warning, step);
            }
        }

        private static void ValidateRequired<T>(ValidationContext context, string code, string name, IList<T> components)
            where T : Component
        {
            int activeCount = 0;
            for (int i = 0; i < components.Count; i++)
                if (components[i] != null && components[i].gameObject.activeInHierarchy)
                    activeCount++;

            if (activeCount == 0)
            {
                string detail = components.Count == 0
                    ? "Компонент отсутствует в активной сцене."
                    : "Компонент найден только на неактивных GameObject; FindFirstObjectByType по умолчанию такие объекты не находит.";
                context.Add(code, "Не найден доступный обязательный компонент " + name + ". " + detail, ValidationSeverity.Error);
            }
            else if (activeCount > 1)
                context.Add(code, "В активной иерархии найдено несколько компонентов " + name + " (" + activeCount + "). Runtime использует FindFirstObjectByType, поэтому выбор неоднозначен.", ValidationSeverity.Warning);
        }
    }
}