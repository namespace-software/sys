using _Core._ScenarioSystem.Component.Scenario;
using _Core.SoundSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Initialization
{
    /// <summary>
    /// Модуль для запуска предварительного сообщения
    /// </summary>
    public class InitializationSystem : MonoBehaviour
    {
        private ScenarioSystem _scenarioSystem;
        private HintsSystem.HintsSystem _hintsSystem;
        private ScenarioAudioMediator _audioMediator;
        
        /// <summary>
        /// Стартовое сообщение озвучки до выбора сценария
        /// </summary>
        [BoxGroup("Приветственные настройки")]
        [Tooltip("Стартовая инструкция до выбора сценария")]
        [SerializeReference] private InstructionDataCondition _startInstructions = new InstructionDataCondition();
        [BoxGroup("Приветственные настройки")]
        [Tooltip("Стартовая инструкция после выбора сценария обучения, до запуска самого сценария")]
        [SerializeReference] private InstructionDataCondition _education = new InstructionDataCondition();
        [BoxGroup("Приветственные настройки")]
        [Tooltip("Стартовая инструкция после выбора сценария тренировки, до запуска самого сценария")]
        [SerializeReference] private InstructionDataCondition _training = new InstructionDataCondition();
        [BoxGroup("Приветственные настройки")]
        [Tooltip("Стартовая инструкция после выбора сценария экзамена, до запуска самого сценария")]
        [SerializeReference] private InstructionDataCondition _examination = new InstructionDataCondition();
        
        [BoxGroup("Debug")]
        [Tooltip("Для тестового пропуска всех озвучек и переходу сразу к выполнению сценария\n" +
                 "В сборке параметр выставляется в значение false")]
        [SerializeField] private bool _isFastStart;
        
        private void Awake()
        {
            _scenarioSystem = FindFirstObjectByType<ScenarioSystem>();
            _audioMediator = FindFirstObjectByType<ScenarioAudioMediator>();
            _hintsSystem = FindFirstObjectByType<HintsSystem.HintsSystem>();
            
#if !UNITY_EDITOR
            _isFastStart = false;
#endif
        }

        private void Start()
        {
            _hintsSystem.FillInfoPanel(_startInstructions.instructionText);
            _audioMediator.Play(_startInstructions.instructionClip, 2);
        }

        /// <summary>
        /// Запуск озвучки и текста до запуска сценария обучения
        /// </summary>
        /// <param name="scenario">Выбранный сценарий</param>
        public void StartScenarioEducation(ScenarioBase scenario)
        {
            if (_isFastStart)
            {
                _scenarioSystem.StartTeachingScenario(scenario);
                return;
            }
            
            _hintsSystem.FillInfoPanel(_education.instructionText);
            _audioMediator.Play(_education.instructionClip, 1, () =>
            {
                _scenarioSystem.StartTeachingScenario(scenario);
            });
        }
        
        /// <summary>
        /// Запуск озвучки и текста до запуска сценария тренировки
        /// </summary>
        /// <param name="scenario">Выбранный сценарий</param>
        public void StartScenarioTraining(ScenarioBase scenario)
        {
            if (_isFastStart)
            {
                _scenarioSystem.StartTrainingScenario(scenario);
                return;
            }
            
            _hintsSystem.FillInfoPanel(_training.instructionText);
            _audioMediator.Play(_training.instructionClip, 1, () =>
            {
                _scenarioSystem.StartTrainingScenario(scenario);
            });
        }
        
        /// <summary>
        /// Запуск озвучки и текста до запуска сценария экзамена
        /// </summary>
        /// <param name="scenario">Выбранный сценарий</param>
        public void StartScenarioExamination(ScenarioBase scenario)
        {
            if (_isFastStart)
            {
                _scenarioSystem.StartExaminationScenario(scenario);
                return;
            }
            
            _hintsSystem.FillInfoPanel(_examination.instructionText);
            _audioMediator.Play(_examination.instructionClip, 1, () =>
            {
                _scenarioSystem.StartExaminationScenario(scenario);
            });
        }
    }
}
