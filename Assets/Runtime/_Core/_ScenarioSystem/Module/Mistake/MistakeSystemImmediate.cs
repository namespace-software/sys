using System;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Step;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Mistake
{
    public class MistakeSystemImmediate : MistakeSystemBase
    {
        public override void Mistake(ConditionBase condition, StepBase step, Action action)
        {
            ActionComplete = action;
            
            // Если обычная ошибка, запускаем обработку кондишина, после шага
            if (condition.mistakeType == EnumMistakeType.Common)
            {
                Debug.Log($"<color=red>SL Совершена ошибка в кондишине: {condition.startInstructions.instructionText}</color>");
                
                // Запуск ошибки/озвучки кондишина
                SoundMediator.Play(condition.mistakeInstructions.instructionClip, 0, () =>
                {
                    // Запуск ошибки/озвучки степа если он есть
                    if (step != null)
                    {
                        SoundMediator.Play(step.mistakeInstructions.instructionClip, 0, () => { ErrorComplete(); });
                        HintsSystem.FillInfoPanel(step.mistakeInstructions.instructionText);
                    }
                    // Если нет – то запускаем эвент от СС
                    else
                    {
                        ErrorComplete();
                    }
                });
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
            }
            // Если критическая ошибка, запускаем обработку кондишина
            else
            {
                // Критическая
                Debug.Log($"<color=red>SL Совершена критическая ошибка в кондишине: {condition.startInstructions.instructionText}</color>");
                
                SoundMediator.Play(condition.mistakeInstructions.instructionClip, 0, () => { ErrorComplete(); });
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                //Логика критической ошибки...
            }
        }
        public override void Mistake(StepBase step, Action action)
        {
            ActionComplete = action;

            // Обычная
            Debug.Log($"<color=red>SL Совершена ошибка в кондишине: {step.mistakeInstructions.instructionText}</color>");
            SoundMediator.Play(step.mistakeInstructions.instructionClip, 0, () => { ErrorComplete(); });
            HintsSystem.FillInfoPanel(step.mistakeInstructions.instructionText);
            
            //Логика ошибки...
        }
    }
}
