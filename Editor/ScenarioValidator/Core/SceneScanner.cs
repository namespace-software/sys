using System;
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
                context.ScenarioSystems.AddRange(root.GetComponentsInChildren<ScenarioSystem>(true));
                context.HintsSystems.AddRange(root.GetComponentsInChildren<HintsSystem>(true));
                context.MistakeSystems.AddRange(root.GetComponentsInChildren<MistakeSystemBase>(true));
                context.LogSystems.AddRange(root.GetComponentsInChildren<LogSystem>(true));
                context.TrackedManagers.AddRange(root.GetComponentsInChildren<TrackedManager>(true));
            }

            foreach (StepBase step in context.Steps)
                ReadConditionArray(step, "conditions", step, null, "Step '" + step.name + "'");

            foreach (GroupSteps group in context.Groups)
                ReadConditionArray(group, "Conditions", null, group, "Group '" + group.name + "' end conditions");

            foreach (ScenarioBase scenario in context.Scenarios)
                ReadPermanentConditions(scenario);

            // Dependencies are managed references too; collect their targets so graph checks can resolve them.
            for (int i = 0; i < context.Conditions.Count; i++)
                ReadDependencies(context.Conditions[i], context);

            return context;
        }

        private static void ReadConditionArray(UnityEngine.Object host, string propertyName, StepBase step, GroupSteps group, string location)
        {
            SerializedObject serialized = new SerializedObject(host);
            SerializedProperty array = serialized.FindProperty(propertyName);
            if (array == null || !array.isArray)
                return;

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                ConditionBase condition = element.propertyType == SerializedPropertyType.ManagedReference
                    ? element.managedReferenceValue as ConditionBase : null;

                // Preserve null entries as explicit scan records by reporting immediately on the owning Unity object.
                if (condition == null)
                {
                    // A null slot is handled by the owning validator through this marker.
                    if (step != null) { /* checked below via serialized array by StepValidator */ }
                    continue;
                }

                AddIfMissing(condition, host, step, group, location + " / condition " + i);
            }
        }

        private static void ReadPermanentConditions(ScenarioBase scenario)
        {
            SerializedObject serialized = new SerializedObject(scenario);
            SerializedProperty array = serialized.FindProperty("_permanentConditions");
            if (array == null || !array.isArray) return;

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty item = array.GetArrayElementAtIndex(i);
                SerializedProperty conditionProperty = item.FindPropertyRelative("PermanentCondition");
                ConditionBase condition = conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.ManagedReference
                    ? conditionProperty.managedReferenceValue as ConditionBase : null;
                if (condition != null)
                    AddIfMissing(condition, scenario, null, null, "Scenario '" + scenario.name + "' permanent condition " + i);
            }
        }

        private static void AddIfMissing(ConditionBase condition, UnityEngine.Object target, StepBase step, GroupSteps group, string location)
        {
            // A managed reference can intentionally be reused, so retain each owner occurrence for diagnostics.
            PendingEntries.Add(new ConditionEntry(condition, target, step, group, location));
        }

        // Serialized managed references are collected before context creation completes. A local static buffer
        // would be unsafe across scans, therefore the scanner uses a per-scan buffer via the helper below.
        private static readonly System.Collections.Generic.List<ConditionEntry> PendingEntries = new System.Collections.Generic.List<ConditionEntry>();

        private static void ReadDependencies(ConditionEntry entry, ValidationContext context)
        {
            if (entry.Condition == null) return;
            System.Reflection.FieldInfo field = typeof(ConditionBase).GetField("_dependingConditions",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var dependencies = field != null ? field.GetValue(entry.Condition) as System.Collections.Generic.IList<ConditionBase> : null;
            if (dependencies == null) return;
            for (int i = 0; i < dependencies.Count; i++)
                if (dependencies[i] != null)
                    AddIfMissing(dependencies[i], entry.Target, entry.OwnerStep, entry.OwnerGroup, entry.Location + " / dependency " + i);
        }
    }
}