using System;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Group;
using _Core._ScenarioSystem.Component.Step;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Core._ScenarioSystem.Component.Scenario.Tactic
{
    /// <summary>
    /// Стандартная реализация стратегии обработки событий сценария.
    /// Анализирует входящие данные по общим правилам режима и возвращает
    /// типовые решения для большинства сценариев.
    /// </summary>
    [Serializable]
    public class ScenarioLogicStandard : ScenarioLogicBase
    {
        #region Index

        [Tooltip("Индекс текущей группы шагов. \nМожет отличаться от выбранной тактики.")]
        [SerializeField, ReadOnly] protected int _indexGroup;
        [Tooltip("Индекс текущего шага. \nМожет отличаться от выбранной тактики.")]
        [SerializeField, ReadOnly] protected int _indexStep;
        [Tooltip("Индекс текущего кондишина. \nМожет отличаться от выбранной тактики.")]
        [SerializeField, ReadOnly] protected int _indexCondition;

        protected int IndexGroup
        {
            get => _indexGroup;
            set
            {
                _indexGroup = value;
                
                if (IndexGroup < 0)
                {
                    _indexGroup = 0;
                }
            }
        }
        protected int IndexStep
        {
            get => _indexStep;
            set
            {
                _indexStep = value;
                IndexCondition = 0;
            }
        }
        protected int IndexCondition
        {
            get => _indexCondition;
            set => _indexCondition = value;
        }

        #endregion

        #region Base

        protected override void Init()
        {
            // Прокидываем линки на эвенты для связи с кондишинами (почему здесь? чтобы не потерять линки при подписке)
            // TODO скорее всего стоит это дело поправить и сделать с нормальными подписками и отписками
            // но пока это фаст рабочее решение на данном этапе
            for (IndexGroup = 0; IndexGroup < GroupSteps.Length; IndexGroup++)
            {
                for (IndexStep = 0; IndexStep < GroupSteps[IndexGroup].Steps.Length; IndexStep++)
                {
                    GroupSteps[IndexGroup].Steps[IndexStep].isUsed = SetUsedComponentFromServiceType(GroupSteps[IndexGroup].Steps[IndexStep].ServiceType);
                    for (IndexCondition = 0; IndexCondition < GroupSteps[IndexGroup].Steps[IndexStep].conditions.Count; IndexCondition++)
                    {
                        GroupSteps[IndexGroup].Steps[IndexStep].conditions[IndexCondition].isUsed = 
                            SetUsedComponentFromServiceType(GroupSteps[IndexGroup].Steps[IndexStep].conditions[IndexCondition].ServiceType);
                        ConditionBase condition = GroupSteps[IndexGroup].Steps[IndexStep].conditions[IndexCondition];
                        condition.OnResolved.AddListener(ConditionResolved);
                    }
                }
            }

            IndexCondition = 0;
            IndexStep = 0;
            IndexGroup = 0;
            
            // Прокидываем линки тем же способом на перманентные кондишины
            for (int x = 0; x < PermanentConditions.Count; x++)
            {
                if (PermanentConditions[x].PermanentCondition != null)
                    PermanentConditions[x].PermanentCondition.OnResolved.AddListener(ConditionResolvedPermanents);
            }
        }

        /// <summary>
        /// В стандартной тактике раньше была реализована проверка на енд, позже эту механику выпилили из шагов и перенесли в группы
        /// </summary>
        /// <param name="type">Тип используемый классом при проигрывании сценария</param>
        /// <returns>Возвращает переменную булевую (использовать этот класс в сценарии или нет)</returns>
        protected override bool SetUsedComponentFromServiceType(ServiceType type)
        {
            return true;
        }

        protected override void Dispose()
        {
            for (IndexGroup = 0; IndexGroup < GroupSteps.Length; IndexGroup++)
            {
                for (IndexStep = 0; IndexStep < GroupSteps[IndexGroup].Steps.Length; IndexStep++)
                {
                    for (IndexCondition = 0; IndexCondition < GroupSteps[IndexGroup].Steps[IndexStep].conditions.Count; IndexCondition++)
                    {
                        ConditionBase condition = GroupSteps[IndexGroup].Steps[IndexStep].conditions[IndexCondition];
                        condition.OnResolved.RemoveListener(ConditionResolved);
                    }
                }
            }
            
            for (int x = 0; x < PermanentConditions.Count; x++)
            {
                if (PermanentConditions[x].PermanentCondition != null)
                    PermanentConditions[x].PermanentCondition.OnResolved.RemoveListener(ConditionResolvedPermanents);
            }
        }

        protected override void Loop()
        {
            if (IsMistakeAwait)
                return;
            
            for (; IndexGroup < GroupSteps.Length; IndexGroup++)
            {
                // инициализация группы
                if (GroupSteps[IndexGroup].State == State.None)
                {
                    GroupInit(GroupSteps[IndexGroup]);
                    return;
                }
                for (; IndexStep < GroupSteps[IndexGroup].Steps.Length; IndexStep++)
                {
                    // инициализация шага
                    StepBase step = GroupSteps[IndexGroup].Steps[IndexStep];
                    if (step.State == State.None)
                    {
                        if (!step.isUsed)
                            continue;
                        
                        StepInit(step);
                        return;
                    }
                    
                    for (; IndexCondition < GroupSteps[IndexGroup].Steps[IndexStep].conditions.Count; IndexCondition++)
                    {
                        // инициализация кондишина
                        ConditionBase condition = GroupSteps[IndexGroup].Steps[IndexStep].conditions[IndexCondition];
                        if (condition.State == State.None)
                        {
                            ConditionInit(condition, GroupSteps[IndexGroup].Steps[IndexStep]);

                            if (step.conditionsInitType == InitConditionType.StepByStep || 
                                IndexCondition + 1 >= step.conditions.Count)
                                return;
                        }
                    }
                    
                    // завершение шага
                    if (step.State is State.Init)
                    {
                        StepResolved(step);
                        return;
                    }

                    IndexCondition = 0;
                }
                
                // завершение группы
                if (GroupSteps[IndexGroup].State == State.Init)
                {
                    GroupResolve(GroupSteps[IndexGroup]);
                    return;
                }

                IndexStep = 0;
            }
            
            EndScenario?.Invoke();
        }

        #region Group

        protected virtual void GroupInit(GroupSteps group)
        {
            group.Init();
            LogSystem.Log(group);
            
            Loop();
        }

        protected virtual void GroupResolve(GroupSteps group)
        {
            group.CompleteGroup();
            LogSystem.Log(group);
            
            Loop();
        }

        #endregion
        
        #region Step

        protected virtual void StepInit(StepBase step)
        {
            step.InitStep();
            LogSystem.Log(step);
            
            ConditionPermanentSetup(step);
            HintsSystem.FillInfoPanel(step.startInstructions.instructionText);
            SoundSystem.Play(step.startInstructions.instructionClip, step.startInstructions.instructionPlayDelay, () =>
                {
                    step.OnStepInit?.Invoke();
                    Loop();
                });
            
            TrackedManager.OnUpdatedHistory?.Invoke(step);
        }

        protected virtual void StepResolved(StepBase step)
        {
            if (IsMistakeAwait)
                return;
            
            // чек на ошибку происходит в комлит степ
            step.CompleteStep();
            
            // тут нужно сделать все запросы какие необходимы
            // аудио, месседжи, ошибки и тд
            if (step.State == State.Success)
            {
                HintsSystem.FillInfoPanel(step.successInstructions.instructionText);
                SoundSystem.Play(step.successInstructions.instructionClip, step.successInstructions.instructionPlayDelay, () =>
                    {
                        step.OnStepResolved?.Invoke();
                        Loop();
                    });
            }
            else
            {
                IsMistakeAwait = true;
                
                HintsSystem.FillInfoPanel(step.mistakeInstructions.instructionText);
                MistakeSystem.Mistake(step, () => { MistakeCallback(Restart); });
            }

            LogSystem.Log(step);
            // Loop();
        }

        #endregion
        
        #region Condition Classic
        
        protected virtual void ConditionInit(ConditionBase condition, StepBase parent)
        {
            // Debug.Log($"<color=yellow>SL Condition Init \"{condition.instructions.instructionText}\"</color>");
            HintsSystem.FillInfoPanel(condition.startInstructions.instructionText);
            SoundSystem.Play(condition, true, condition.startInstructions.instructionClip, condition.startInstructions.instructionRepeatDelay);
            condition.InitCondition(GroupSteps[IndexGroup].Steps[IndexStep], true);
            
            LogSystem.Log(condition, false);
        }
        protected virtual void ConditionResolved(ConditionBase condition, bool isSuccess)
        {
            if (IsMistakeAwait)
                return;
            
            bool isOrderRequiredMistake = false;
            if (isSuccess)
            {
                isSuccess = condition.ParentStep.CheckConditionOrderRequired(condition);
                isOrderRequiredMistake = !isSuccess;
            }
            
            // Сохранение текущего значения ошибки для сохранения данных в логах о нарушении последовательности
            // TODO необходимо переработать
            EnumMistakeType temp = condition.mistakeType;
            if (isOrderRequiredMistake)
                condition.mistakeType = EnumMistakeType.OrderRequired;
            
            // Кондишен выполненен успешно
            if (isSuccess)
            {
                HintsSystem.FillInfoPanel(condition.successInstructions.instructionText);
                // Если шаг запущен с настройкой запуск всех кондишинов разом
                if (condition.ParentStep.conditionsInitType == InitConditionType.Simultaneously)
                {
                    SoundSystem.Play(condition, false, condition.successInstructions.instructionClip, 0,
                        condition.ParentStep.CheckCompleteAllConditions() ? Loop : null);
                }
                // Если шаг запущен с настройкой последовательного запуска
                else
                {
                    SoundSystem.Play(condition, false, condition.successInstructions.instructionClip, 0, Loop);
                }
            }
            // Кондишен провален
            else
            {
                IsMistakeAwait = true;
                
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                MistakeSystem.Mistake(condition, condition.ParentStep, () => { MistakeCallback(Restart); });
            }
            

            LogSystem.Log(condition, false);
            if (isOrderRequiredMistake)
                condition.mistakeType = temp;
        }

        #endregion
        
        #region Condition Permanent

        protected virtual void ConditionPermanentSetup(StepBase nowStartStep)
        {
            for (int x = 0; x < PermanentConditions.Count; x++)
            {
                if (PermanentConditions[x].PermanentCondition == null)
                    continue;
                
                if (PermanentConditions[x].PermanentCondition.State == State.None &&
                    (PermanentConditions[x].StartStep == nowStartStep ||
                    CheckRangePermanent(PermanentConditions[x], nowStartStep)))
                {
                    PermanentConditions[x].PermanentCondition.InitCondition(nowStartStep, false);
                    LogSystem.Log(PermanentConditions[x].PermanentCondition, true);
                }
                else if (PermanentConditions[x].PermanentCondition.State != State.None &&
                         (PermanentConditions[x].EndStep == nowStartStep ||
                         !CheckRangePermanent(PermanentConditions[x], nowStartStep)))
                {
                    PermanentConditions[x].PermanentCondition.ResetConditionToDefault();
                    PermanentConditions[x].PermanentCondition.OnConditionResolved?.Invoke();
                    LogSystem.Log(PermanentConditions[x].PermanentCondition, true);
                }
            }
        }

        private bool CheckRangePermanent(ConditionOrder order, StepBase step)
        {
            bool start = false;
            for (int indexGroup = 0; indexGroup < GroupSteps.Length; indexGroup++)
            {
                for (int indexStep = 0; indexStep < GroupSteps[indexGroup].Steps.Length; indexStep++)
                {
                    StepBase stepLoop = GroupSteps[indexGroup].Steps[indexStep];
                    if (!start && order.StartStep == stepLoop)
                    {
                        start = true;
                    }
                    
                    if (start)
                    {
                        if (stepLoop == step)
                        {
                            return true;
                        }
                        else if (order.EndStep == stepLoop)
                        {
                            return order.EndStep == step;
                        }
                    }
                }   
            }
            
            return false;
        }

        protected virtual void ConditionResolvedPermanents(ConditionBase condition, bool isSuccess)
        {
            if (IsMistakeAwait)
                return;
            // Debug.Log(
                // $"<color=green>SL Сondition Permanent[{isSuccess}/{condition.mistakeType}]: {condition.instructions.instructionText}</color>");
            
            LogSystem.Log(condition, true);
       
            if (condition.mistakeType == EnumMistakeType.Common)
            {
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                SoundSystem.Play(condition.mistakeInstructions.instructionClip, 0);
                
                condition.ResetConditionToDefault();
                condition.OnConditionResolved?.Invoke();
                condition.InitCondition(condition.ParentStep, false);
            }
            else
            {
                IsMistakeAwait = true;
                
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                MistakeSystem.Mistake(condition, condition.ParentStep, () =>
                {
                    condition.ResetConditionToDefault();
                    condition.OnConditionResolved?.Invoke();
                    MistakeCallback(Restart);
                });
            }
        }

        #endregion

        #endregion

        #region Movement

        public override void Back()
        {
            if (!HasValidScenario() || !HasValidPosition())
                return;
            
            GroupSteps[IndexGroup].Steps[IndexStep].ResetStep();
            LogSystem.Log(GroupSteps[IndexGroup].Steps[IndexStep]);
            
            SoundSystem.ClearList();
            
            IndexStep--;
            BackStepService();

            
            GroupSteps[IndexGroup].Steps[IndexStep].ResetStep();
            
            Loop();
        }

        private void BackStepService()
        {
            if (IndexStep < 0)
            {
                _indexStep = IndexGroup > 0 ? GroupSteps[IndexGroup - 1].Steps.Length - 1 : 0;
                IndexGroup -= IndexGroup > 0 ? 1 : 0;
            }

            if (!GroupSteps[IndexGroup].Steps[IndexStep].isUsed)
            {
                IndexStep--;
                BackStepService();
            }
        }

        public override void Skip()
        {
            if (!HasValidScenario() || !HasValidPosition())
                return;
            
            GroupSteps[IndexGroup].Steps[IndexStep].SkipStep(LogSystem);
            IndexStep++;
            
            SoundSystem.ClearList();
            
            if (IndexGroup < GroupSteps.Length &&
                IndexStep >= GroupSteps[IndexGroup].Steps.Length)
            {
                IndexStep = 0;
                IndexGroup++;
            }
            
            Loop();
        }

        public override void Restart()
        {
            if (!HasValidScenario() || !HasValidPosition())
                return;
            
            GroupSteps[IndexGroup].Steps[IndexStep].ResetStep();
            IndexCondition = 0;
            
            SoundSystem.ClearList();
            
            Loop();
        }
        
        protected bool HasValidScenario()
        {
            return GroupSteps != null && GroupSteps.Length > 0;
        }
        
        protected bool HasValidPosition()
        {
            return GroupSteps != null &&
                   IndexGroup >= 0 &&
                   IndexGroup < GroupSteps.Length &&
                   GroupSteps[IndexGroup] != null &&
                   GroupSteps[IndexGroup].Steps != null &&
                   IndexStep >= 0 &&
                   IndexStep < GroupSteps[IndexGroup].Steps.Length;
        }

        #endregion
    }
}
