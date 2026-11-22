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
using _Core._ScenarioSystem.Component.Condition;

namespace _Core.Editor.ScenarioValidator
{
    public sealed class ConditionEntry
    {
        public ConditionBase Condition { get; }
        public Object Target { get; }
        public StepBase OwnerStep { get; }
        public GroupSteps OwnerGroup { get; }
        public string Location { get; }

        public ConditionEntry(ConditionBase condition, Object target, StepBase ownerStep, GroupSteps ownerGroup, string location)
        {
            Condition = condition;
            Target = target;
            OwnerStep = ownerStep;
            OwnerGroup = ownerGroup;
            Location = location;
        }
    }

    public sealed class ValidationContext
    {
        public Scene Scene { get; }
        public GameObject SceneTarget { get; }
        public readonly List<ScenarioBase> Scenarios = new List<ScenarioBase>();
        public readonly List<GroupSteps> Groups = new List<GroupSteps>();
        public readonly List<StepBase> Steps = new List<StepBase>();
        public readonly List<ConditionEntry> Conditions = new List<ConditionEntry>();
        public readonly List<SocketFilledProvider> SocketFilledProviders = new List<SocketFilledProvider>();

        public readonly List<ScenarioAudioMediator> AudioMediators = new List<ScenarioAudioMediator>();
        public readonly List<_Core._ScenarioSystem.ScenarioSystem> ScenarioSystems = new List<_Core._ScenarioSystem.ScenarioSystem>();
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