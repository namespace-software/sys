using System;
using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Scenario.Tactic;
using _Core._ScenarioSystem.Module.Logger;
using _Core._ScenarioSystem.Module.Mistake;
using _Core._ScenarioSystem.Module.Tracker.Core;
using _Core.SoundSystem;
using UnityEngine;

namespace _Core._ScenarioSystem.Component.Scenario
{
    [System.Serializable]
    public class Scenario : ScenarioBase
    {
        // [BoxGroup("Debug")]
        // [Tooltip(
        //     "�� ����� ������ ���������� ���� ������ ����������� �������������. ����������� ������� ����� �� �������� � ������� ������ � ������������.")]
        // [SerializeField, ReadOnly]
        // private Step.Step[] _steps;

        private Action _OnStepCompleted;

        private ScenarioSystem _scenarioSystem;
        private ScenarioAudioMediator _soundSystem;
        private HintsSystem.HintsSystem _hintsSystem;
        private MistakeSystemBase _mistakeSystem;
        private LogSystem _logSystem;
        private TrackedManager _trackedManager;

        private List<TrackedObject> _trackedObjects;

        private Vector3 _stepStartPlayerPos;
        private Quaternion _stepStartPlayerRot;
        private bool _skipProcess;

        private void OnValidate()
        {
            for (int x = 0; x < _permanentConditions.Count; x++)
            {
                if (_permanentConditions[x].PermanentCondition == null)
                    continue;
                
                _permanentConditions[x].PermanentCondition.isPermanent = true;
                _permanentConditions[x].PermanentCondition.mistakeType = 
                    _permanentConditions[x].mistakeType;
            }
        }

        private void Awake()
        {
            _soundSystem = FindFirstObjectByType<ScenarioAudioMediator>();
            _scenarioSystem = FindFirstObjectByType<ScenarioSystem>();
            _hintsSystem = FindFirstObjectByType<HintsSystem.HintsSystem>();
            _mistakeSystem = FindFirstObjectByType<MistakeSystemBase>();
            _logSystem = FindFirstObjectByType<LogSystem>();
            _trackedManager = FindFirstObjectByType<TrackedManager>();

            if (_soundSystem == null)
                throw new System.ArgumentNullException(nameof(_soundSystem), "������ ����� �� ����� ���� null");
            if (_scenarioSystem == null)
                throw new System.ArgumentNullException(nameof(_scenarioSystem), "������ ������� ��������� �� ����� ���� null");
            if (_hintsSystem == null)
                throw new System.ArgumentNullException(nameof(_hintsSystem), "������ ��������� �� ����� ���� null");
            if (_mistakeSystem == null)
                throw new System.ArgumentNullException(nameof(_mistakeSystem), "������ ������ �� ����� ���� null");
            if (_logSystem == null)
                throw new System.ArgumentNullException(nameof(_logSystem), "������ ����� �� ����� ���� null");
            if (_trackedManager == null)
                throw new System.ArgumentNullException(nameof(_trackedManager), "������ ������� �� ����� ���� null");
            
            IsInited = true;
        }

        public override void StartScenario(ScenarioLogicBase startLogic)
        {
            OnScenarioInit?.Invoke();

            logic = startLogic;
            logic.Init(transform, _soundSystem, _hintsSystem, _mistakeSystem, _permanentConditions, _logSystem,
                _trackedManager);
            logic.EndScenario += EndScenario;
        }

        public override void StopScenario()
        {
            logic.EndLogic();
        }

        public override void BackStage()
        {
            logic.Back();
        }

        public override void RestartStage()
        {
            logic.Restart();
        }

        public override void SkipStage()
        {
            logic.Skip();
        }

        public void AddTrackedObject(TrackedObject trackedObject)
        {
            if (!_trackedObjects.Contains(trackedObject))
            {
                Debug.Log("TrackedObject Manual added " + trackedObject.transform.name);
                _trackedObjects.Add(trackedObject);
            }
        }

        public void RemoveTrackedObject(TrackedObject trackedObject)
        {
            if (_trackedObjects.Contains(trackedObject))
            {
                Debug.Log("TrackedObject Manual removed " + trackedObject.transform.name);
                _trackedObjects.Remove(trackedObject);
            }
        }

        public override void Mistake(ConditionBase conditionBase, bool isEndScenario)
        {
            logic.Mistake(conditionBase, isEndScenario);
        }

        private void EndScenario()
        {
            _scenarioSystem.EndScenario();
            logic.EndScenario -= EndScenario;
        }
    }
}
