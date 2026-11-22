using System;
using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Group;
using _Core._ScenarioSystem.Module.Logger;
using _Core._ScenarioSystem.Module.Mistake;
using _Core._ScenarioSystem.Module.Tracker.Core;
using _Core.SoundSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Core._ScenarioSystem.Component.Scenario.Tactic
{ 
    /// <summary>
    /// Базовый класс стратегии обработки событий сценария.
    /// Получает контекст событий от Scenario, анализирует их по заданным правилам
    /// и возвращает решение о дальнейшем ходе сценария.
    /// </summary>
    public abstract class ScenarioLogicBase
    {
        [Tooltip("Массив групп шагов. Генерируется при инициализации сценария")]
        [SerializeField, ReadOnly] protected GroupSteps[] GroupSteps;
        private bool _isInit;
        protected bool IsMistakeAwait;
        
        protected ScenarioAudioMediator SoundSystem;
        protected HintsSystem.HintsSystem HintsSystem;
        protected MistakeSystemBase MistakeSystem;
        protected LogSystem LogSystem;
        protected TrackedManager TrackedManager;
        protected List<ConditionOrder> PermanentConditions;
        
        protected Action<ConditionBase> OnConditionReached;
        public Action EndScenario;
        
        #region Base
        
        /// <summary>
        /// Инициализация линков, систем и запуск основного цикла
        /// </summary>
        /// <param name="parent">Родительский объект сценария, для сканирования групп шагов</param>
        /// <param name="soundSystem">Аудио система</param>
        /// <param name="hintsSystem">Система вывода текста</param>
        /// <param name="mistakeSystem">Система вывода информации по ошибкам</param>
        /// <param name="permanentConditions">Перманентные кондишины сценария</param>
        /// <param name="logSystem">Система логирования</param>
        /// <param name="trackedManager">Система отслеживания стейтов объектов мира</param>
        public void Init(Transform parent, ScenarioAudioMediator soundSystem, HintsSystem.HintsSystem hintsSystem, 
            MistakeSystemBase mistakeSystem, List<ConditionOrder> permanentConditions, LogSystem logSystem,
            TrackedManager trackedManager)
        {
            SoundSystem = soundSystem;
            HintsSystem = hintsSystem;
            MistakeSystem = mistakeSystem;
            LogSystem = logSystem;
            TrackedManager = trackedManager;
            
            PermanentConditions = permanentConditions;
            
            if (_isInit)
                return;

            _isInit = true;

            if (!InitGroups(parent))
            {
                Debug.LogError($"У сценария нет ни одного GroupSteps");
                return;
            }
            
            LogSystem.Init();

            Init();

            Loop();
        }

        /// <summary>
        /// Инициализация групп шагов для оснвоного цикла
        /// </summary>
        /// <param name="parent">Родительский объект сценария</param>
        /// <returns>Возвращает успешность сбора групп шагов</returns>
        protected virtual bool InitGroups(Transform parent)
        {
            // Собираем группу в кучу
            List<GroupSteps> temp = new List<GroupSteps>();
            for (int x = 0; x < parent.childCount; x++)
            {
                GroupSteps tempStep = parent.GetChild(x).GetComponent<GroupSteps>();
                if (tempStep != null && tempStep.gameObject.activeSelf)
                    temp.Add(tempStep);
            }

            if (temp.Count <= 0)
            {
                GroupSteps = Array.Empty<GroupSteps>();
                return false;
            }

            GroupSteps = temp.ToArray();
            return true;
        }

        /// <summary>
        /// Завершение работы тактики
        /// </summary>
        public void EndLogic()
        {
            Dispose();
        }

        protected abstract void Init();
        
        /// <summary>
        /// Проверяет, является ли тип используемым в текущей тактике сценария
        /// </summary>
        /// <param name="type">Тип используемый классом при проигрывании сценария</param>
        /// <returns>Возвращает переменную булевую (использовать этот класс в сценарии или нет)</returns>
        protected abstract bool SetUsedComponentFromServiceType(ServiceType type);
        
        /// <summary>
        /// Отписка всех сущностей от внутренних методов
        /// </summary>
        protected abstract void Dispose();
        
        /// <summary>
        /// Основной цикл в котором живёт система сценариев (последовательность запуска/проверки/остановки системы)
        /// </summary>
        protected abstract void Loop();

        #endregion
        
        #region Movement

        /// <summary>
        /// Рестарт текущего и возвращение на шаг/блок назад
        /// </summary>
        public abstract void Back();
        
        /// <summary>
        /// Пропуск текущего шага/блока
        /// </summary>
        public abstract void Skip();
        
        /// <summary>
        /// Рестарт текущего шага/блока
        /// </summary>
        public abstract void Restart();

        #endregion

        #region Mistake

        protected virtual void MistakeCallback(Action action)
        {
            IsMistakeAwait = false;
            action?.Invoke();
        }

        public virtual void Mistake(ConditionBase conditionBase, bool isEndScenario = false)
        {
            IsMistakeAwait = true;

            conditionBase.State = State.Failure;
            LogSystem.Log(conditionBase, false);

            // если экзамен
            if (this is ScenarioLogicExamination)
            {
                // ошибку запускаем только при критической ошибке
                if (isEndScenario ||
                    conditionBase.mistakeType == EnumMistakeType.Critical)
                {
                    HintsSystem.FillInfoPanel(conditionBase.mistakeInstructions.instructionText);
                    MistakeSystem.Mistake(conditionBase, conditionBase.ParentStep, () =>
                    {
                        conditionBase.ResetConditionToDefault();
                        conditionBase.OnConditionResolved?.Invoke();
                        EndScenario?.Invoke();
                    });
                }
            }
            // если обучение или тренировка
            else
            {
                HintsSystem.FillInfoPanel(conditionBase.mistakeInstructions.instructionText);
            
                MistakeSystem.Mistake(conditionBase, conditionBase.ParentStep, () =>
                {
                    if (isEndScenario)
                    {
                        conditionBase.ResetConditionToDefault();
                        conditionBase.OnConditionResolved?.Invoke();
                        EndScenario?.Invoke();
                    }
                    else
                    {
                        MistakeCallback(Restart);
                    }
                });
            }
        }

        #endregion

    }
}
