using System.Collections.Generic;
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
    public sealed class ValidationContext
    {
        public Scene Scene { get; }
        public GameObject SceneTarget { get; }

        public readonly List<ScenarioBase> Scenarios = new List<ScenarioBase>();
        public readonly List<GroupSteps> Groups = new List<GroupSteps>();
        public readonly List<StepBase> Steps = new List<StepBase>();

        public readonly List<ScenarioAudioMediator> AudioMediators = new List<ScenarioAudioMediator>();
        public readonly List<ScenarioSystem> ScenarioSystems = new List<ScenarioSystem>();
        public readonly List<HintsSystem> HintsSystems = new List<HintsSystem>();
        public readonly List<MistakeSystemBase> MistakeSystems = new List<MistakeSystemBase>();
        public readonly List<LogSystem> LogSystems = new List<LogSystem>();
        public readonly List<TrackedManager> TrackedManagers = new List<TrackedManager>();

        public ValidationReport Report { get; }

        public ValidationContext(Scene scene, ValidationReport report, GameObject sceneTarget)
        {
            Scene = scene;
            Report = report;
            SceneTarget = sceneTarget;
        }

        public void Add(string code, string message, ValidationSeverity severity, Object target = null)
        {
            Report.Add(new ValidationResult(code, message, severity, target != null ? target : SceneTarget));
        }
    }
}