using UnityEngine;
using UnityEngine.SceneManagement;
using _Core._ScenarioSystem.Component.Group;
using _Core._ScenarioSystem.Component.Scenario;
using _Core._ScenarioSystem.Component.Step;
using _Core._ScenarioSystem;
using _Core._ScenarioSystem.HintsSystem;
using _Core._ScenarioSystem.Module.Logger;
using _Core._ScenarioSystem.Module.Mistake;
using _Core._ScenarioSystem.Module.Tracker.Core;
using _Core.SoundSystem;

namespace _Core.Editor.ScenarioValidator
{
    /// <summary>
    /// Собирает данные только из переданной сцены. Никакие Runtime-методы не вызываются.
    /// </summary>
    public sealed class SceneScanner
    {
        public ValidationContext Scan(Scene scene, ValidationReport report)
        {
            GameObject[] roots = scene.IsValid() && scene.isLoaded
                ? scene.GetRootGameObjects()
                : new GameObject[0];

            GameObject sceneTarget = roots.Length > 0 ? roots[0] : null;
            ValidationContext context = new ValidationContext(scene, report, sceneTarget);

            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];

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

            return context;
        }
    }
}