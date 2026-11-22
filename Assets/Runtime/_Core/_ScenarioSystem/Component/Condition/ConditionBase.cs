using System;
using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Step;
using HighlightPlus;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace _Core._ScenarioSystem.Component.Condition
{
    /// <summary>
    /// Базовый класс проверки состояний.
    /// Необходим для проверки выполнения условий поставленных СС.
    /// Может быть так же перманентной ошибкой, для выявления неправильного прохождения сценария.
    ///
    /// PropertyOrder регламентирует позицию в инспекторе:
    /// 0-99 - Базовые свойства
    /// 100-199 - Свойства дочернего класса реализации
    /// 200-299 - Свойства инициализации базового класса
    /// 300-399 - Свойства выполнения кондиции (положительного и отрицательного)
    /// </summary>
    [Serializable]
    public abstract class ConditionBase
    {
        #region Odin Label

        // Свойство только для чтения, которое формирует заголовок для Odin
        public string ListLabel
        {
            get
            {
                string typeName = GetType().Name; // Имя дочернего класса
            
                if (string.IsNullOrEmpty(startInstructions.instructionText))
                {
                    return typeName.Replace("Condition", "");
                }
            
                return $"{typeName.Replace("Condition", "")} | {startInstructions.instructionText}";
            }
        }

        #endregion

        #region General

        [Tooltip("Установить true, если данное действие не должно быть включено в отчет\n" +
                 "Нужен для фильтрации: ReportView будет игнорировать данный кондишн для выгрузки\n" +
                 "Временное решение\n")]
        public bool ExcludeFromReport;

        [BoxGroup("General"), PropertyOrder(0)]
        [Tooltip("Сервисный тип. Необходим для обозначения шагов/кондишинов для выполнения или пропуска.\n" +
                 "1. Classic - работает во всех случаях.\n" +
                 "2. Info - информационный, используется в сценарии обучения, в тренировке/экзамене игнорируется\n" +
                 "3. End - шаг выхода из блока в режимах тренировки/экзамена, в обучении игнорируется\n")]
        [SerializeField] private ServiceType _serviceType;

        
        [ValueDropdown(nameof(GetAllowedTypes))]
        [BoxGroup("General"), PropertyOrder(1), HideIf(nameof(isPermanent))] public EnumMistakeType mistakeType;
        public static ValueDropdownList<EnumMistakeType> GetAllowedTypes() { return new ValueDropdownList<EnumMistakeType> { { EnumMistakeType.Common }, { EnumMistakeType.Critical } }; }
        
        [Tooltip("Количество поинтов за совершение ошибки")]
        [BoxGroup("General"), PropertyOrder(2)] public int penaltyPoints;

        [Tooltip("Включение логики зависимости на другой кондишн")]
        [BoxGroup("General"), PropertyOrder(3)] public bool hasDependencies;
        
        
        [InfoBox(
            "ВАЖНО!!! Для того чтобы добавить связанное состояние необходимо правой кнопкой щелкнуть на нужном состоянии, от выполнения которого будет зависеть текущее. Выбрать Copy Special -> Copy Reference. Щелкнуть правой кнопокой мыши на Depending Conditions -> Paste Element.",
            nameof(hasDependencies))]
        [BoxGroup("General"), PropertyOrder(4), SerializeReference, ShowIf(nameof(hasDependencies))] protected List<ConditionBase> _dependingConditions;

        [FoldoutGroup("General/Start Instructions"), HideLabel, PropertyOrder(5)]
        [InfoBox("Инструктаж. Аудио и текст")]
        public InstructionDataCondition startInstructions;

        
        [FoldoutGroup("General/Visual Hints"), HideLabel, PropertyOrder(6)]
        [InfoBox("Визуальные подсказки. Автоматически активируются при старте и выключаются при выполнении")]
        [HideIf(nameof(isPermanent))] public VisualHints visualHints;
        
        [FoldoutGroup("General/Unity Events"), PropertyOrder(7)]
        [InfoBox("События при старте кондишина")]
        [SerializeField, HideIf(nameof(isPermanent))] public UnityEvent OnConditionInitialized;

        [FoldoutGroup("General/Unity Events"), PropertyOrder(8)]
        [InfoBox("События при завершении кондишина")]
        [HideIf(nameof(isPermanent))] public UnityEvent OnConditionResolved;
        [HideInInspector] public UnityEvent<ConditionBase, bool> OnResolved;

        #endregion

        #region End

        [BoxGroup("End")]
        [FoldoutGroup("End/Success Instructions"), HideLabel, PropertyOrder(300)]
        [InfoBox("Набор параметров при успешном завершении кондишина")]
        [HideIf(nameof(isPermanent))] public InstructionDataBase successInstructions;

        [FoldoutGroup("End/Mistake Instructions"), HideLabel, PropertyOrder(301)]
        [InfoBox("Набор параметров при ошибочном завершении кондишина")]
        public InstructionDataBase mistakeInstructions;

        #endregion

        #region Debug

        [FoldoutGroup("Debug")]
        [Tooltip("Параметр текущего состояния шага/кондишина")]
        [PropertyOrder(400), SerializeField, ReadOnly] private State _state;
        
        [FoldoutGroup("Debug")]
        [Tooltip("Шаг-родитель")]
        [PropertyOrder(400), SerializeField, ReadOnly] protected StepBase parentStep;

        [FoldoutGroup("Debug")]
        [Tooltip("Параметр текущего состояния инициализации кондишина")]
        [PropertyOrder(401), ReadOnly] public bool initiated;

        [FoldoutGroup("Debug")]
        [Tooltip("Параметр текущего состояния выполнения кондишина")]
        [PropertyOrder(402), ReadOnly] public bool isConditionReached;

        [FoldoutGroup("Debug")]
        [Tooltip("Параметр текущего состояния закрытия кондишина")]
        [PropertyOrder(403), ReadOnly] public bool closed;

        [FoldoutGroup("Debug")]
        [Tooltip("Параметр текущего состояния пропуска кондишина")]
        [PropertyOrder(404), ReadOnly] public bool skipped;

        #endregion

        private bool _withHints;
    
        /// <summary>
        /// Логическая переменная. Будет ли использоваться этот шаг, после инициализации и проверки сервисного типа
        /// </summary>
        [FoldoutGroup("Debug"), PropertyOrder(399), ReadOnly] public bool isUsed;
        
        /// <summary>
        /// Логическая переменная. Для отображения параметров в инспекторе, если это перманентный кондишн
        /// </summary>
        [HideInInspector] public bool isPermanent = false;
    
        public ServiceType ServiceType
        {
            get => _serviceType;
            set
            {
                _serviceType = value;
            }
        }
        public State State
        {
            get => _state;
            set
            {
                _state = value;

                initiated = State != State.None;
                isConditionReached = State is State.Success or State.Failure;
                closed = State is State.Success or State.Failure;
                skipped = State == State.Skipped;
            
                // Debug.LogError($"<color=cyan>{instructions.instructionText}/{State}</color>>");
            }
        }
        public StepBase ParentStep => parentStep;
    
        /// <summary>
        /// Сохраняем параметру инициализации. Инициализируем, подаём сигнал инициализации вовне.
        /// </summary>
        /// <returns></returns>
        public void InitCondition(StepBase step, bool withHints)
        {
            TryToFixEmptyInstructions();

            parentStep = step;
            _withHints = withHints;

            // Debug.LogError($"State {startInstructions.instructionText}/{State}");
            SetupHints(_withHints);
            SetupCondition();

            OnConditionInitialized?.Invoke();
            State = State.Init;
        }


        /// <summary>
        /// Правим текст пропущенных полей, выводим лог с информацией инита
        /// </summary>
        /// <returns></returns>
        private void TryToFixEmptyInstructions()
        {
            if (startInstructions.instructionText == "") startInstructions.instructionText = "EMPTY";
            Debug.Log("InitCondition. " + "Hint: [" + startInstructions.instructionText + "]");
        }


        /// <summary>
        /// Проверка может ли быть успешное завершение. Проверяется наличие связанных условий и их статус завершения
        /// </summary>
        /// <returns></returns>
        public bool CanResolveAsSuccess()
        {
            if (!hasDependencies) return true;                     // Тут возвращаем тру если кондиция позитивная и нет связанных кондиций

            foreach (var item in _dependingConditions)
            {
                if (!item.isConditionReached) return false;
            }
            return true;                                                    // Тут возвращаем тру если все связанные кондиции завершены
        }

        /// <summary>
        /// Активация подсказок в зависимости от режима и сетапа
        /// </summary>
        /// <returns></returns>

        protected void SetupHints(bool active)
        {
            if (visualHints.highlightEffects != null && visualHints.highlightEffects.Count > 0)
            {
                foreach (HighlightEffect effect in visualHints.highlightEffects)
                {
                    if (effect) 
                        effect.highlighted = visualHints.activateEffectsAnyway || active;  //TODO Ошибка если передаем деактивацию, но при этом Anyway - всегда активен даже при выключении.
                    else 
                        effect.enabled = visualHints.activateEffectsAnyway || active;
                }
            }

            if (visualHints.gameObjects != null && visualHints.gameObjects.Count > 0)
            {
                foreach (var item in visualHints.gameObjects)
                {
                    item.SetActive(visualHints.activateObjectsAnyway || active);  //TODO Ошибка если передаем деактивацию, но при этом Anyway - всегда активен даже при выключении.
                }
            }
        }


        /// <summary>
        /// Тут ресетим изменения в объектах и в самом кондишене, возвращая его в состояние начала шага
        /// </summary>
        /// <returns></returns>
        public virtual void ResetConditionToDefault()
        {
            // IsInited = false;
            // IsConditionReached = false;
            // Closed = false;

            DisableCondition();
            SetupHints(false);
        
            State = State.None;
        }

        /// <summary>
        /// Деактивируем и помечаем как пропущенный
        /// </summary>
        /// <returns></returns>
        public virtual void SkipCondition()
        {
            //IsInited = false;
            //IsConditionReached = false;
            //Closed = false;
            // Skiped = true;
        
            OnConditionInitialized?.Invoke();
            OnConditionResolved?.Invoke();

            DisableCondition();
            SetupHints(false);
        
            State = State.Skipped;
        }

        /// <summary>
        /// Активация внутренней логики
        /// </summary>
        /// <returns></returns>
        public abstract void SetupCondition();

        /// <summary>
        /// Проверка состояния
        /// ????????????????????????????????????????????????????????????
        /// Запрос выполнения:?
        /// </summary>
        /// <returns></returns>
        public virtual void CheckCondition()
        {
            
        }

        /// <summary>
        /// Деактивация внутренней логики
        /// </summary>
        /// <returns></returns>
        public virtual void DisableCondition()
        {
            SetupHints(false);
        }
    }
}
