using System;
using System.Collections;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Scenario;
using _Core._ScenarioSystem.Component.Scenario.Tactic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
// using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


namespace _Core._ScenarioSystem
{
    public class ScenarioSystem : MonoBehaviour
    {
        public Action OnStepReset;
        public Action OnNextStep;
        public Action OnRestartStep;

        [BoxGroup("Parameters")]
        [Tooltip("Тип сценария (Обучение, Тренировка и Экзамен)." +
                 "Выбирается автоматически через запуск сценария (через кнопку или публичный метод этого скрипта)." +
                 "При автостарте информацию использует текущую.")]
        [SerializeReference] private ScenarioLogicBase _startLogic;
        [BoxGroup("Parameters")]
        [Tooltip("Текущий сценарий который будет запущен, если не переопределён публичным методом")]
        [SerializeField] private ScenarioBase _scenario;
        [BoxGroup("Parameters")]
        [Tooltip("Задержка инициализации сценария перед запуском")]
        [SerializeField] private float _startDelay = 0.3f;
        [BoxGroup("Parameters")]
        [Tooltip("Автозапуск сценария при запуске сцены")]
        [SerializeField] private bool _autostart = false;

        [FoldoutGroup("Events")]
        [Tooltip("Список эвентов запускаемых при инициализации сценария")]
        public UnityEvent OnScenarioStart;
        [FoldoutGroup("Events")]
        [Tooltip("Список эвентов запускаемых при завершении сценария")]
        public UnityEvent OnScenarioEnd;
        //TODO не работает, потому что не прокинуты ссылки
        // [FoldoutGroup("Events")]
        // [Tooltip("Список эвентов запускаемых при совершении критической ошибки")]
        // public UnityEvent OnStepCriticalMistake;
        
        [FoldoutGroup("Debug")]
        [Tooltip("Сценарий уже запущен?")]
        [SerializeField, ReadOnly] private bool _isScenarioStarted;
        
        public bool AutoStart => _autostart;

        public EnumScenarioMode CurrentMode => _currentMode;
        [SerializeField, ReadOnly] private EnumScenarioMode _currentMode;

        private void Start()
        {
            if (ScenarioLaunchRequest.TryConsume(out EnumScenarioMode selectedMode))
            {
                SelectedScenario(_scenario, selectedMode);
                return;
            }

            if (_autostart)
                StartCoroutine(StartScenario(_scenario));
        }

        private IEnumerator StartScenario(ScenarioBase scenarioToStart)
        {
            _isScenarioStarted = true;
            _scenario = scenarioToStart;
            _scenario.gameObject.SetActive(true);
            yield return new WaitUntil(() => _scenario.IsInited);
            yield return new WaitForSeconds(_startDelay);
            _scenario.StartScenario(_startLogic);
            OnScenarioStart?.Invoke();
        }

        public void StartTeachingScenario(ScenarioBase scenario = null)
        {
            SelectedScenario(scenario, EnumScenarioMode.Teaching);
        }

        public void StartTrainingScenario(ScenarioBase scenario = null)
        {
            SelectedScenario(scenario, EnumScenarioMode.Training);
        }

        public void StartExaminationScenario(ScenarioBase scenario = null)
        {
            SelectedScenario(scenario, EnumScenarioMode.Examination);
        }

        private void SelectedScenario(ScenarioBase scenario, EnumScenarioMode scenarioMode)
        {
            scenario = scenario == null ? _scenario : scenario;
            
            if (_isScenarioStarted)
            {
                Debug.LogError($"Сценарий уже инициализирован, повторная инициализация невозможна!");
                return;
            }

            if (scenario == null)
            {
                Debug.LogError($"Сценарий не получен, проверьте наличие ссылок на сценарий в методах запуска!");
                return;
            }

            _currentMode = scenarioMode;
            
            switch (scenarioMode)
            {
                case EnumScenarioMode.Teaching: _startLogic = new ScenarioLogicStandard(); break;
                case EnumScenarioMode.Training: _startLogic = new ScenarioLogicTraining(); break;
                case EnumScenarioMode.Examination: _startLogic = new ScenarioLogicExamination(); break;
            }
            StartCoroutine(StartScenario(scenario));
        }

        public void BackStage()
        {
            _scenario.BackStage();
        }
        
        public void RestartStage()
        {
            _scenario.RestartStage();
        }

        public void SkipStage()
        {
            _scenario.SkipStage();
        }

        public void RestartScene()
        {
            SceneManager.LoadScene(0);
        }

        public void Mistake(ConditionBase conditionBase, bool isEndScenario)
        {
            _scenario.Mistake(conditionBase, isEndScenario);
        }

        public void CloseApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }

        public void EndScenario()
        {
            OnScenarioEnd?.Invoke();
        }
    }
}
