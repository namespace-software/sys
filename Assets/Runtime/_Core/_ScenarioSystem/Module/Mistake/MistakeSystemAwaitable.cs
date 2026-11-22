using System;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Step;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Core._ScenarioSystem.Module.Mistake
{
    /// <summary>
    /// Реализация модуля ошибок с отложенной активацией
    /// </summary>
    public class MistakeSystemAwaitable : MistakeSystemBase
    {
        [Tooltip("Ссылка на кнопку при нажатии на которую произойдёт продолжение выполнения сценария")]
        [SerializeField] private Button[] _button;
        [Tooltip("Ссылка на объекты которые активируются после озвучки ошибки и выключатся после нажатия на кнопку _button")]
        [SerializeField] private GameObject[] _objectsFromError;
        [Tooltip("Событие которое активируются после озвучки ошибки")]
        [SerializeField] private UnityEvent _eventFromError;

        [Header("Настройки ошибки последовательности")]
        [SerializeField] private AudioClip _audioClipErrorOrderRequired;
        [SerializeField] private string _textErrorOrderRequired;
        public override void Mistake(ConditionBase condition, StepBase step, Action action)
        {
            SoundMediator.ClearList();
            
            ActionComplete = action;
            // Если обычная ошибка, запускаем обработку кондишина, после шага
            if (condition.mistakeType == EnumMistakeType.Common)
            {
                Debug.Log($"<color=red>SL Совершена ошибка в кондишине: {condition.startInstructions.instructionText}</color>");
                
                // Запуск ошибки/озвучки кондишина
                SoundMediator.Play(condition.mistakeInstructions.instructionClip, 0, () =>
                {
                    // Запуск ошибки/озвучки степа если он есть
                    if (step != null && step.mistakeInstructions.instructionClip != null)
                    {
                        SoundMediator.Play(step.mistakeInstructions.instructionClip, 0, () => { AwaitError(); });
                        HintsSystem.FillInfoPanel(step.mistakeInstructions.instructionText);
                    }
                    // Если нет – то запускаем эвент от СС
                    else
                    {
                        AwaitError();
                    }
                });
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
            }
            else if (condition.mistakeType == EnumMistakeType.OrderRequired)
            {
                // Запуск ошибки/озвучки кондишина
                SoundMediator.Play(_audioClipErrorOrderRequired, 0, () =>
                {
                    AwaitError();
                });
                HintsSystem.FillInfoPanel(_textErrorOrderRequired);
            }
            // Если критическая ошибка, запускаем обработку кондишина
            else
            {
                // Критическая
                Debug.Log($"<color=red>SL Совершена критическая ошибка в кондишине: {condition.startInstructions.instructionText}</color>");
                
                SoundMediator.Play(condition.mistakeInstructions.instructionClip, 0, () => { AwaitError(); });
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                //Логика критической ошибки...
            }
        }
        public override void Mistake(StepBase step, Action action)
        {
            SoundMediator.ClearList();
            
            ActionComplete = action;
            
            // Обычная
            Debug.Log($"<color=red>SL Совершена ошибка в кондишине: {step.mistakeInstructions.instructionText}</color>");
            SoundMediator.Play(step.mistakeInstructions.instructionClip, 0, () => { AwaitError(); });
            HintsSystem.FillInfoPanel(step.mistakeInstructions.instructionText);
            
            //Логика ошибки...
        }

        /// <summary>
        /// Метод начала ожидания ответа от пользователя
        /// </summary>
        private void AwaitError()
        {
            for (int x = 0; x < _objectsFromError.Length; x++)
            {
                _objectsFromError[x].SetActive(true);
            }
            _eventFromError?.Invoke();
            for (int x = 0; x < _button.Length; x++)
            {
                _button[x].onClick.AddListener(ErrorComplete);
            }
        }

        protected override void ErrorComplete()
        {
            for (int x = 0; x < _objectsFromError.Length; x++)
            {
                _objectsFromError[x].SetActive(false);
            }
            for (int x = 0; x < _button.Length; x++)
            {
                _button[x].onClick.RemoveListener(ErrorComplete);
            }
            
            base.ErrorComplete();
        }
    }
}
