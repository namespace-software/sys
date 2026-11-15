using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Step;
using UnityEngine;

namespace _Core._ScenarioSystem.Component.Scenario.Tactic
{
    public class ScenarioLogicExamination : ScenarioLogicTraining
    {
        #region Step

        protected override void StepResolved(StepBase step)
        {
            if (IsMistakeAwait)
                return;
            
            // чек на ошибку происходит в комлит степ
            step.CompleteStep();
            LogSystem.Log(step);
            
            // тут нужно сделать все запросы какие необходимы
            // аудио, месседжи, ошибки и тд
            if (step.State == State.Success)
            {
                step.OnStepResolved?.Invoke();
            }
            // else
            // {
            //     HintsSystem.FillInfoPanel(step.MistakeInstructions.instructionText);
            //     MistakeSystem.Mistake(step, Restart);
            // }
            
        }

        #endregion
        
        #region Condition Classic
        
        protected override void ConditionResolved(ConditionBase condition, bool isSuccess)
        {
            if (IsMistakeAwait)
                return;
            
            bool isOrderRequiredMistake = false;
            if (isSuccess && condition.ParentStep != null)
            {
                if (GroupSteps[IndexGroup].IsStepsOrderRequired && condition.ParentStep.IsStepOrderRequired)
                    isSuccess = GroupSteps[IndexGroup].CheckStepOrderRequired(condition.ParentStep);
                if (isSuccess)
                    isSuccess = condition.ParentStep.CheckConditionOrderRequired(condition);
                isOrderRequiredMistake = !isSuccess;
            }
            
            EnumMistakeType temp = condition.mistakeType;
            if (isOrderRequiredMistake)
            {
                condition.mistakeType = EnumMistakeType.OrderRequired;
                condition.State = State.Failure;
            }
            
            LogSystem.Log(condition, false);
            
            if (isSuccess)
            {
                // Parent step нет у кондишинов проверки выхода из группы шагов
                if (condition.ParentStep != null)
                {
                    if (condition.ParentStep.conditionsInitType == InitConditionType.Simultaneously)
                    {
                        // тут происходит выполнение шага
                        if (condition.ParentStep.CheckCompleteAllConditions())
                        {
                            // Loop();
                            StepResolved(condition.ParentStep);
                        }
                    }
                    else
                    {
                        // тут происходит чек выполнения всех кондишинов шага
                        // TODO трабла появится скорее всего если последний кондишн будет выполнен, а первый нет
                        if (condition.ParentStep.conditions[^1] == condition)
                        {
                            StepResolved(condition.ParentStep);
                        }
                        else
                        {
                            for (int x = 0; x < condition.ParentStep.conditions.Count; x++)
                            {
                                if (condition.ParentStep.conditions[x].State == State.None)
                                {
                                    ConditionInit(condition.ParentStep.conditions[x], condition.ParentStep);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (condition.mistakeType == EnumMistakeType.Critical)
            {
                IsMistakeAwait = true;
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                MistakeSystem.Mistake(condition, condition.ParentStep, EndScenario);
                
                if (isOrderRequiredMistake)
                    condition.mistakeType = temp;
                return;
            }
            else
            {
                StepResolved(condition.ParentStep);
                
                if (isOrderRequiredMistake)
                    condition.mistakeType = temp;
            }
            
            //TODO Вылет за пределы группы в момент завершения сценария (трабла внутри последовательности инициализации).
            //Не записывается лог последнего кондишина в группе
            if (IndexGroup >= GroupSteps.Length)
            {
                // Debug.LogError($"TODO Вылет за пределы группы.");
                return;
            }
                
            // логика проверки выполнения всех шагов
            if (GroupSteps[IndexGroup].CheckCompleteAllSteps())
                Loop();
            else if (condition.ParentStep == null)
            {
                if (GroupSteps[IndexGroup].IsAnyConditionFromEnd)
                {
                    Skip();
                }
                else if (GroupSteps[IndexGroup].Conditions.Length > 0 && GroupSteps[IndexGroup].CheckCompleteAllConditions())
                {
                    Skip();
                }
            }
        }

        #endregion
        
        #region Condition Permanent

        protected override void ConditionResolvedPermanents(ConditionBase condition, bool isSuccess)
        {
            if (IsMistakeAwait)
                return;
            
            LogSystem.Log(condition, true);
       
            if (condition.mistakeType == EnumMistakeType.Common)
            {
                // HintsSystem.FillInfoPanel(condition.fail.instructionText);
                // SoundSystem.Play(condition.fail.instructionClip, 0, false);
                
                condition.ResetConditionToDefault();
                condition.OnConditionResolved?.Invoke();
                condition.InitCondition(condition.ParentStep, false);
            }
            else
            {
                IsMistakeAwait = true;
                
                // TODO Криты перманентов?
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                MistakeSystem.Mistake(condition, condition.ParentStep, () =>
                {
                    condition.ResetConditionToDefault();
                    condition.OnConditionResolved?.Invoke();
                    EndScenario?.Invoke();
                });
            }
        }

        #endregion
    }
}
