using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Module.Logger;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace _Core._ScenarioSystem.Component.Step
{
    [System.Serializable]
    public abstract class StepBase : MonoBehaviour
    {
        #region General

        [BoxGroup("General")]
        // [Title("General", TitleAlignment = TitleAlignments.Centered, Bold = true)]
        [Tooltip("Сервисный тип. Необходим для обозначения шагов/кондишинов для выполнения или пропуска.\n" +
                 "1. Classic - работает во всех случаях.\n" +
                 "2. Info - информационный, используется в сценарии обучения, в тренировке/экзамене игнорируется\n")]
                 // +
                 // "3. End - шаг выхода из блока в режимах тренировки/экзамена, в обучении игнорируется\n")]
        [SerializeField] private ServiceType _serviceType;

        
        [BoxGroup("General")] [Tooltip("Индекс шага. Необходимо использовать индекс шага из ТЗ")]
        [SerializeField] private int _stepIndex;
        public int StepIndex => _stepIndex;

        
        [BoxGroup("General")] [Tooltip("Имя шага. Необходимо использовать имя шага из ТЗ")]
        [SerializeField] private string _stepName;
        public string StepName => _stepName;
        [BoxGroup("General")] [Tooltip("Переменная проверка последовательности выполнения шага в группе шагов, " +
                                       "шаг выполненный не последовательно завалится")]
        [SerializeField] private bool _isStepOrderRequired;
        public bool IsStepOrderRequired => _isStepOrderRequired;

        [FoldoutGroup("General/Start Instructions"), HideLabel]
        [InfoBox("Набор параметров при инициализации шага")]
        // [Tooltip("Набор параметров при инициализации шага")]
        public InstructionDataStep startInstructions;
        
        
        [FoldoutGroup("General/Unity Events")]
        [InfoBox("События при старте шага")]
        // [Tooltip("События при старте шага")]
        [SerializeField] public UnityEvent OnStepInit;
        [FoldoutGroup("General/Unity Events")]
        [InfoBox("События при рестарте шага/возвращении на шаг назад")]
        // [Tooltip("События при рестарте шага/возвращении на шаг назад")]
        [SerializeField] public UnityEvent OnStepReset;
        [FoldoutGroup("General/Unity Events")]
        [InfoBox("События при завершении шага")]
        // [Tooltip("События при завершении шага")]
        [SerializeField] public UnityEvent OnStepResolved;
        #endregion

        #region Conditions

        [BoxGroup("Conditions")]
        [Tooltip("Simultaneously - все состояния шага и связанные с ними объекты активируются одномоментно." +
                 "\nStepByStep - каждый следующий подшаг активируется и инициализируется после предыдущего в соответствии с порядком в листе." +
                 "\nManual - Каждый шаг активируется вручную, методом вызова")]
        public InitConditionType conditionsInitType = InitConditionType.StepByStep;

        private bool IsInitTypeOverrideSimultaneouslyVisible =>
            conditionsInitType == InitConditionType.Simultaneously;
        [BoxGroup("Conditions")]
        [Tooltip("Условный параметр переопределяющий тип инициализации кондишинов.\n" +
                 "Если включён: при запуске Тренировки/Экзамена тип инициализации будет переопределён на Simultaneously")]
        [HideIf(nameof(IsInitTypeOverrideSimultaneouslyVisible))]
        public bool IsInitTypeOverrideSimultaneously;

        private bool IsConditionOrderRequiredVisible =>
            !(conditionsInitType == InitConditionType.StepByStep && IsInitTypeOverrideSimultaneously || 
              conditionsInitType == InitConditionType.Simultaneously ||
              conditionsInitType == InitConditionType.Manual);
        [BoxGroup("Conditions")][Tooltip("Требуется проверка порядка выполнения condition")]
        [HideIf(nameof(IsConditionOrderRequiredVisible))]
        public bool IsConditionOrderRequired;

        [BoxGroup("Conditions")]
        [Tooltip("Объект состояния и действие по завершению. Шаг сценария будет закончен, когда каждый из элементов этого списка будет выполнен. " +
                 "Для правильного использования подсказок, порядок объектов списка должен соответствовать порядку действий игрока, если строгий порядок существует. /r Прим.(Подсказки будут показываться всегда в соответствии с порядком в этом листе)")]
        [SerializeReference]
        [ListDrawerSettings(ListElementLabelName = "ListLabel")]
        [LabelWidth(250)]
        public List<ConditionBase> conditions;

        #endregion

        #region End
        [BoxGroup("End")]
        [FoldoutGroup("End/Success Instructions"), HideLabel]
        // [Tooltip("Набор параметров при успешном завершении шага")]
        [InfoBox("Набор параметров при успешном завершении шага")]
        public InstructionDataStep successInstructions;
        [FoldoutGroup("End/Mistake Instructions"), HideLabel]
        // [Tooltip("Набор параметров при ошибке выполнения шага")]
        [InfoBox("Набор параметров при ошибке выполнения шага")]
        public InstructionDataStep mistakeInstructions;
        
        #endregion
        
        #region Debug

        // [Title("Debug", TitleAlignment = TitleAlignments.Centered, Bold = true)]
        
        [Tooltip("Параметр текущего состояния шага/кондишина")]
        [SerializeField, ReadOnly, FoldoutGroup("Debug")] private State _state;
        
        [Tooltip("Параметр текущего состояния инициализации шага")]
        [SerializeField, ReadOnly, FoldoutGroup("Debug")] protected bool _isStartEventsCompleted;
        [Tooltip("Параметр текущего состояния выполнения шага")]
        [SerializeField, ReadOnly, FoldoutGroup("Debug")] protected bool _isRelateConditionsReached;
        [Tooltip("Параметр текущего состояния завершения шага")]
        [SerializeField, ReadOnly, FoldoutGroup("Debug")] protected bool _isCompleted;
        [Tooltip("Параметр текущего состояния ошибки шага")]
        [SerializeField, ReadOnly, FoldoutGroup("Debug")] protected bool _isFailed;
        [Tooltip("Параметр текущего состояния пропуска шага")]
        [SerializeField, ReadOnly, FoldoutGroup("Debug")] protected bool isSkipped;

        private string _defaultStepName;
        
        /// <summary>
        /// Логическая переменная. Будет ли использоваться этот шаг, после инициализации и проверки сервисного типа
        /// </summary>
        [HideInInspector] public bool isUsed;
        
        public ServiceType ServiceType { get => _serviceType; set => _serviceType = value; }
        public State State
        {
            get => _state;
            set
            {
                _state = value;

                gameObject.name = $"{_defaultStepName}[{_state}]";

                _isStartEventsCompleted = State != State.None;
                
                _isCompleted = State == State.Success;
                _isFailed = State == State.Failure;
                isSkipped = State == State.Skipped;
                
                // Debug.LogError($"<color=cyan>{gameObject.name}/{State}</color>");
            }
        }
        
    
        public bool IsStartEventsCompleted { get => _isStartEventsCompleted; }
        public bool IsRelateConditionsReached { get => _isRelateConditionsReached; }
        public bool IsCompleted { get => _isCompleted; }
        public bool IsFailed { get => _isFailed; }
        public bool IsSkipped { get => isSkipped; }

        #endregion

        private void Awake()
        {
            _defaultStepName = gameObject.name;
        }

        public abstract void InitStep();
        public abstract void CompleteStep();
        public abstract bool CheckCompleteAllConditions();
        public abstract bool CheckConditionOrderRequired(ConditionBase condition);
        public abstract void ResetStep();
        public abstract void SkipStep(LogSystem logSystem);
        public abstract void FailOutside();
    }
}
