using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PromVR.ScenarioSystem
{
    public class ScenarioSystemHotKeys : MonoBehaviour
    {
        [SerializeField] private global::_Core._ScenarioSystem.ScenarioSystem _scenarioSystem;
        private bool _isAutoStart;
        
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        [BoxGroup("Control Settings")][DisableIf(nameof(_isAutoStart))][Tooltip("Клавиша запуска сценария обучения")]
        [SerializeField] private InputActionReference _startTeachingAction;
        [BoxGroup("Control Settings")][DisableIf(nameof(_isAutoStart))][Tooltip("Клавиша запуска сценария тренировки")]
        [SerializeField] private InputActionReference _startTrainingAction;
        [BoxGroup("Control Settings")] [DisableIf(nameof(_isAutoStart))] [Tooltip("Клавиша запуска сценария экзамена")]
        [SerializeField] private InputActionReference _startExaminationAction;
        [BoxGroup("Control Settings")] [Tooltip("Клавиша перехода на шаг/группу шагов назад")] 
        [SerializeField] private InputActionReference _backStepAction;
        [BoxGroup("Control Settings")] [Tooltip("Клавиша перезапуска шага/группы шагов")]
        [SerializeField] private InputActionReference _restartCurrentStepAction;
        [BoxGroup("Control Settings")] [Tooltip("Клавиша перехода на шаг/группу шагов вперёд")]
        [SerializeField] private InputActionReference _skipStepAction;
        //TODO необходимо вынести в отдельный скрипт саппорт, не относиться к логике текущего скрипта
        [BoxGroup("Control Settings")] [Tooltip("Клавиша перезагрузки сцены")]
        [SerializeField] private InputActionReference _restartSceneAction;
#else
        [BoxGroup("Control Settings")][DisableIf(nameof(_isAutoStart))][Tooltip("Клавиша запуска сценария обучения")]
        [SerializeField] private KeyCode _startTeachingKey = KeyCode.T;
        [BoxGroup("Control Settings")][DisableIf(nameof(_isAutoStart))][Tooltip("Клавиша запуска сценария тренировки")]
        [SerializeField] private KeyCode _startTrainingKey = KeyCode.Y;
        [BoxGroup("Control Settings")][DisableIf(nameof(_isAutoStart))][Tooltip("Клавиша запуска сценария экзамена")]
        [SerializeField] private KeyCode _startExaminationKey = KeyCode.E;
        [BoxGroup("Control Settings")][Tooltip("Клавиша перехода на шаг/группу шагов назад")]
        [SerializeField] private KeyCode _backStepKey = KeyCode.Z;
        [BoxGroup("Control Settings")][Tooltip("Клавиша перезапуска шага/группы шагов")]
        [SerializeField] private KeyCode _restartCurrentStepKey = KeyCode.X;
        [BoxGroup("Control Settings")][Tooltip("Клавиша перехода на шаг/группу шагов вперёд")]
        [SerializeField] private KeyCode _skipStepKey = KeyCode.C;
        //TODO необходимо вынести в отдельный скрипт саппорт, не относиться к логике текущего скрипта
        [BoxGroup("Control Settings")][Tooltip("Клавиша перезагрузки сцены")]
        [SerializeField] private KeyCode _restartSceneKey = KeyCode.R;
#endif

        private void Start()
        {
            if (_scenarioSystem == null) 
                _scenarioSystem = FindFirstObjectByType<global::_Core._ScenarioSystem.ScenarioSystem>();

            _isAutoStart = _scenarioSystem.AutoStart;
        }
        
#if UNITY_EDITOR
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (_startTeachingAction != null && _startTeachingAction.action.triggered && !_isAutoStart) _scenarioSystem.StartTeachingScenario();
            if (_startTrainingAction != null && _startTrainingAction.action.triggered && !_isAutoStart) _scenarioSystem.StartTrainingScenario();
            if (_startExaminationAction != null && _startExaminationAction.action.triggered && !_isAutoStart) _scenarioSystem.StartExaminationScenario();
            if (_backStepAction != null && _backStepAction.action.triggered) _scenarioSystem.BackStage();
            if (_restartCurrentStepAction != null && _restartCurrentStepAction.action.triggered) _scenarioSystem.RestartStage();
            if (_skipStepAction != null && _skipStepAction.action.triggered) _scenarioSystem.SkipStage();
            if (_restartSceneAction != null && _restartSceneAction.action.triggered) _scenarioSystem.RestartScene();
#else
            if (Input.GetKeyDown(_startTeachingKey) && !_isAutoStart) _scenarioSystem.StartTeachingScenario();
            if (Input.GetKeyDown(_startTrainingKey) && !_isAutoStart) _scenarioSystem.StartTrainingScenario();
            if (Input.GetKeyDown(_startExaminationKey) && !_isAutoStart) _scenarioSystem.StartExaminationScenario();
            if (Input.GetKeyDown(_backStepKey)) _scenarioSystem.BackStage();
            if (Input.GetKeyDown(_restartCurrentStepKey)) _scenarioSystem.RestartStage();
            if (Input.GetKeyDown(_skipStepKey)) _scenarioSystem.SkipStage();
            if (Input.GetKeyDown(_restartSceneKey)) _scenarioSystem.RestartScene();
#endif
        }

#endif
    }
}
