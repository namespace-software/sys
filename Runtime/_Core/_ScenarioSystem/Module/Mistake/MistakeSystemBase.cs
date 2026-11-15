using System;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Step;
using _Core.SoundSystem;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Mistake
{
    /// <summary>
    /// Базовый класс системы ошибок, для реализации используйте этот класс
    /// Этот класс прокидывается через всю систему сценариев и взаимодействует с ним
    /// </summary>
    public abstract class MistakeSystemBase : MonoBehaviour
    {
        /// <summary>
        /// Прослойка для взаимодействия со звуковой системой
        /// </summary>
        protected ScenarioAudioMediator SoundMediator;
        /// <summary>
        /// Хинт система для взаимодействия с текстом
        /// </summary>
        protected HintsSystem.HintsSystem HintsSystem;
        /// <summary>
        /// Экшен приходящий от СС, которым мы завершаем выполнение ошибки
        /// </summary>
        protected Action ActionComplete;

        private void Awake()
        {
            SoundMediator = FindFirstObjectByType<ScenarioAudioMediator>();
            HintsSystem = FindFirstObjectByType<HintsSystem.HintsSystem>();
        }
        
        /// <summary>
        /// Ошибка вызывается во время ошибки кондишина
        /// </summary>
        /// <param name="condition">Кондишн с ошибкой</param>
        /// <param name="step">Шаг с ошибкой.
        /// ВНИМАНИЕ!!! Может прийти null, если это кондишн из массива группы шагов на завершение</param>
        /// <param name="action">Экшен от СС для воспроизведения после обработки ошибки</param>
        public abstract void Mistake(ConditionBase condition, StepBase step, Action action);
        
        /// <summary>
        /// Ошибка вызывается во время ошибки шага, предварительно вызывается ошибка кондишина
        /// </summary>
        /// <param name="step">Шаг с ошибкой</param>
        /// <param name="action">Экшен от СС для воспроизведения после обработки ошибки</param>
        public abstract void Mistake(StepBase step, Action action);

        /// <summary>
        /// Метод завершения выполнения логики системы ошибок
        /// </summary>
        protected virtual void ErrorComplete()
        {
            ActionComplete?.Invoke();
        }
    }
}
