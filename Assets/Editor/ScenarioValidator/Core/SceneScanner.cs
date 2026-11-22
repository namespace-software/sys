using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Core._ScenarioSystem.Component.Group;
using _Core._ScenarioSystem.Component.Scenario;
using _Core._ScenarioSystem.Component.Step;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem;
using _Core._ScenarioSystem.HintsSystem;
using _Core._ScenarioSystem.Module.Logger;
using _Core._ScenarioSystem.Module.Mistake;
using _Core._ScenarioSystem.Module.Tracker.Core;
using _Core.SoundSystem;

namespace _Core.Editor.ScenarioValidator
{
    /// <summary>Сканирует только переданную сцену, включая неактивные объекты. Runtime-код не вызывается.</summary>
    public sealed class SceneScanner
    {
        public ValidationContext Scan(Scene scene, ValidationReport report)
        {
            GameObject[] roots = scene.IsValid() && scene.isLoaded ? scene.GetRootGameObjects() : new GameObject[0];
            ValidationContext context = new ValidationContext(scene, report, roots.Length > 0 ? roots[0] : null);

            foreach (GameObject root in roots)
            {
                context.Scenarios.AddRange(root.GetComponentsInChildren<ScenarioBase>(true));
                context.Groups.AddRange(root.GetComponentsInChildren<GroupSteps>(true));
                context.Steps.AddRange(root.GetComponentsInChildren<StepBase>(true));
                context.AudioMediators.AddRange(root.GetComponentsInChildren<ScenarioAudioMediator>(true));
                context.ScenarioSystems.AddRange(root.GetComponentsInChildren<_Core._ScenarioSystem.ScenarioSystem>(true));
                context.HintsSystems.AddRange(root.GetComponentsInChildren<HintsSystem>(true));
                context.MistakeSystems.AddRange(root.GetComponentsInChildren<MistakeSystemBase>(true));
                context.LogSystems.AddRange(root.GetComponentsInChildren<LogSystem>(true));
                context.TrackedManagers.AddRange(root.GetComponentsInChildren<TrackedManager>(true));
            }

            foreach (StepBase step in context.Steps)
                ReadConditionArray(context, step, "conditions", step, null, "Step '" + step.name + "'");

            foreach (GroupSteps group in context.Groups)
                ReadConditionArray(context, group, "Conditions", null, group, "Group '" + group.name + "' end conditions");

            foreach (ScenarioBase scenario in context.Scenarios)
                ReadPermanentConditions(context, scenario);

            return context;
        }

        private static void ReadConditionArray(ValidationContext context, Object host, string propertyName,
            StepBase step, GroupSteps group, string location)
        {
            SerializedObject serialized = new SerializedObject(host);
            SerializedProperty array = serialized.FindProperty(propertyName);
            if (array == null || !array.isArray) return;

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                ConditionBase condition = element.propertyType == SerializedPropertyType.ManagedReference
                    ? element.managedReferenceValue as ConditionBase : null;

                if (condition == null)
                {
                    context.Add("COND001", location + ": элемент conditions с индексом " + i + " равен null.", ValidationSeverity.Error, host);
                    continue;
                }

                context.Conditions.Add(new ConditionEntry(condition, host, step, group, location + " / condition " + i));
            }
        }

        private static void ReadPermanentConditions(ValidationContext context, ScenarioBase scenario)
        {
            SerializedObject serialized = new SerializedObject(scenario);
            SerializedProperty array = serialized.FindProperty("_permanentConditions");
            if (array == null || !array.isArray) return;

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty item = array.GetArrayElementAtIndex(i);
                SerializedProperty property = item.FindPropertyRelative("PermanentCondition");
                ConditionBase condition = property != null && property.propertyType == SerializedPropertyType.ManagedReference
                    ? property.managedReferenceValue as ConditionBase : null;

                if (condition == null)
                {
                    context.Add("COND002", "Scenario '" + scenario.name + "': permanent condition с индексом " + i + " не назначен.", ValidationSeverity.Error, scenario);
                    continue;
                }

                context.Conditions.Add(new ConditionEntry(condition, scenario, null, null,
                    "Scenario '" + scenario.name + "' permanent condition " + i));
            }
        }
    }
}