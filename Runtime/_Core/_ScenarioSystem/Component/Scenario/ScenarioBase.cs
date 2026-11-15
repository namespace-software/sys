using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Scenario.Tactic;
using _Core._ScenarioSystem.Component.Step;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace _Core._ScenarioSystem.Component.Scenario
{
    [System.Serializable]
    public struct ConditionOrder
    {
        /// <summary>
        /// Сервисный тип для визуализации в инспекторе
        /// </summary>
        /// TODO багует при нулевой ссылке на перманентный кондишн
        // [HideInInspector]
        // public string Label => PermanentCondition != null ? $"{PermanentCondition.mistakeType}|" +
                               // $"{PermanentCondition.startInstructions.instructionText.Replace("\n", " ")}" : "null";
        
        [Tooltip("Перманентный кондишн")]
        [LabelText("$ConditionLabel")] // Указываем Odin брать текст из свойства ConditionLabel
        [LabelWidth(250)]
        [SerializeReference] public ConditionBase PermanentCondition;
        // Свойство только для отрисовки в инспекторе
        private string ConditionLabel
        {
            get
            {
                // Если объект не создан, возвращаем стандартное имя переменной
                if (PermanentCondition == null) 
                    return "Permanent Condition";

                // Если объект есть, используем тот самый ListLabel из базового класса
                return PermanentCondition.ListLabel;
            }
        }
        
        [Tooltip("Тип ошибки вызываемый при завершении кондишина")]
        public EnumMistakeType mistakeType;
        [Tooltip("Шаг при инициализации которого начинает действовать")]
        public StepBase StartStep;
        [Tooltip("Шаг при завершении которого прекращает действовать")]
        public StepBase EndStep;
    }

    /// <summary>
    /// Основной класс, управляющий прохождением сценария.
    /// Выполняет роль Workflow Orchestrator: запускает шаги, принимает их события,
    /// передаёт данные в блок логики ScenarioLogicBase и применяет полученное решение.
    /// Алгоритм обработки отделён и реализуется заменяемыми наследниками ScenarioLogicBase,
    /// а настройки реакции на результаты задаются через ScenarioModePolicy.
    /// </summary>
    public abstract class ScenarioBase : MonoBehaviour
    {
        // TODO для реализации политики
        // [SerializeField] private List<ScenarioModePolicy> modes;

        [FoldoutGroup("Events")]
        [Tooltip("Список эвентов запускаемых при инициализации сценария")]
        public UnityEvent OnScenarioInit;

        [BoxGroup("Permanent Conditions")]
        [Tooltip("Список перманентных состояний. Подходит для реализации логики ошибок которые могут отслеживаться без привязки к конкретным шагам и могут действовать на протяжении всего сценария." +
            " Например: нельзя снимать каску или нельзя превышать скоростной режим и т.п.")]
        // TODO баг нулевого перманент кондишина
        // [ListDrawerSettings(
        //     ListElementLabelName = "Label",
        //     DefaultExpandedState = false
        // )]
        [SerializeField] protected List<ConditionOrder> _permanentConditions;

        [BoxGroup("Debug")]
        [Tooltip("Тип сценария устанавливаемый системой сценариев")]
        [SerializeReference, ReadOnly] protected ScenarioLogicBase logic;
        //TODO переменная используется в неиспользуемом методе во внешне,
        //думаю стоит пересобрать под енум State и сделать приватной
        [BoxGroup("Debug")]
        [Tooltip("Сценарий инициализирован?")]
        [ReadOnly] public bool IsInited;

        protected bool _stopped;
        public bool Stopped { get => _stopped; }

        // START
        public abstract void StartScenario(ScenarioLogicBase logic);
        public abstract void StopScenario();
        
        // MOVE
        public abstract void RestartStage();
        public abstract void BackStage();
        public abstract void SkipStage();

        public abstract void Mistake(ConditionBase conditionBase, bool isEndScenario);
    }
}
