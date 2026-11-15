using System;
using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Group;
using _Core._ScenarioSystem.Component.Step;
using UnityEngine;

namespace _Core._ScenarioSystem.Component.Scenario.Tactic
{
    [Serializable]
    public class ScenarioLogicTraining : ScenarioLogicStandard
    {
        private bool _isLoopRunning;
        private bool _isLoopRequested;

        #region Base

        protected override bool InitGroups(Transform parent)
        {
            int childCount = parent.childCount;
            if (childCount == 0)
            {
                GroupSteps = Array.Empty<GroupSteps>();
                return false;
            }

            List<GroupSteps> sourceGroups = new List<GroupSteps>(childCount);

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (!child.gameObject.activeSelf) continue;

                GroupSteps group = child.GetComponent<GroupSteps>();
                if (group != null)
                    sourceGroups.Add(group);
            }

            if (sourceGroups.Count == 0)
            {
                GroupSteps = Array.Empty<GroupSteps>();
                return false;
            }

            List<GroupSteps> resultGroups = new List<GroupSteps>(sourceGroups.Count);
            int chainStartIndex = 0;

            while (chainStartIndex < sourceGroups.Count)
            {
                int conditionalIndex = -1;

                for (int i = chainStartIndex; i < sourceGroups.Count; i++)
                {
                    ConditionBase[] conditions = sourceGroups[i].Conditions;
                    if (conditions != null && conditions.Length > 0)
                    {
                        conditionalIndex = i;
                        break;
                    }
                }

                if (conditionalIndex < 0)
                {
                    // throw new InvalidOperationException(
                        // $"Не найден GroupSteps с кондишинами выхода, необходимо их добавить для сборки сценария, начиная с индекса {chainStartIndex}.");
                    
                    Debug.LogWarning($"ScenarioSystem Не найден GroupSteps с кондишинами выхода, необходимо их добавить для сборки сценария, начиная с индекса {chainStartIndex}.\n" +
                                   $"Будет сформирована группа без выхода из группы");
                    conditionalIndex = sourceGroups.Count - 1;
                }

                GroupSteps targetGroup = sourceGroups[chainStartIndex];
                GroupSteps conditionalGroup = sourceGroups[conditionalIndex];
                List<StepBase> mergedSteps = new List<StepBase>();

                for (int i = chainStartIndex; i <= conditionalIndex; i++)
                {
                    GroupSteps currentGroup = sourceGroups[i];
                    StepBase[] steps = currentGroup.Steps;
                    if (steps == null || steps.Length == 0) continue;

                    for (int j = 0; j < steps.Length; j++)
                    {
                        StepBase step = steps[j];
                        if (step == null) continue;

                        mergedSteps.Add(step);

                        if (step.transform.parent != targetGroup.transform)
                            step.transform.SetParent(targetGroup.transform, true);
                    }
                }

                targetGroup.Description = conditionalGroup.Description;
                targetGroup.Conditions = conditionalGroup.Conditions;
                targetGroup.Steps = mergedSteps.ToArray();

                for (int i = 0; i < targetGroup.Steps.Length; i++)
                {
                    targetGroup.Steps[i].transform.SetSiblingIndex(i);
                }

                for (int i = chainStartIndex + 1; i <= conditionalIndex; i++)
                {
                    GroupSteps groupToRemove = sourceGroups[i];
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(groupToRemove.gameObject);
                    else
                        UnityEngine.Object.DestroyImmediate(groupToRemove.gameObject);
                }

                resultGroups.Add(targetGroup);
                chainStartIndex = conditionalIndex + 1;
            }

            GroupSteps = resultGroups.ToArray();
            return true;
        }
        
        protected override void Init()
        {
            // Прокидываем линки на эвенты для связи с кондишинами (почему здесь? чтобы не потерять линки при подписке)
            // TODO скорее всего стоит это дело поправить и сделать с нормальными подписками и отписками
            // но пока это фаст рабочее решение на данном этапе
            for (IndexGroup = 0; IndexGroup < GroupSteps.Length; IndexGroup++)
            {
                for (int x = 0; x < GroupSteps[IndexGroup].Conditions.Length; x++)
                {
                    GroupSteps[IndexGroup].Conditions[x].OnResolved.AddListener(ConditionResolved);
                    GroupSteps[IndexGroup].Conditions[x].isUsed = true;
                }
                for (IndexStep = 0; IndexStep < GroupSteps[IndexGroup].Steps.Length; IndexStep++)
                {
                    GroupSteps[IndexGroup].Steps[IndexStep].isUsed = SetUsedComponentFromServiceType(GroupSteps[IndexGroup].Steps[IndexStep].ServiceType);
                    if (GroupSteps[IndexGroup].Steps[IndexStep].IsInitTypeOverrideSimultaneously)
                    {
                        GroupSteps[IndexGroup].Steps[IndexStep].conditionsInitType = InitConditionType.Simultaneously;
                    }
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
        
        protected override void Dispose()
        {
            for (IndexGroup = 0; IndexGroup < GroupSteps.Length; IndexGroup++)
            {
                for (int x = 0; x < GroupSteps[IndexGroup].Conditions.Length; x++)
                {
                    GroupSteps[IndexGroup].Conditions[x].OnResolved.RemoveListener(ConditionResolved);
                }
                
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
        
        /// <summary>
        /// В тренировке/экзамене тактика проверяет на инфо и если это инфо выключаем проигрывание в сценарии
        /// </summary>
        /// <param name="type">Тип используемый классом при проигрывании сценария</param>
        /// <returns>Возвращает переменную булевую (использовать этот класс в сценарии или нет)</returns>
        protected override bool SetUsedComponentFromServiceType(ServiceType type)
        {
            return type != ServiceType.Info;
        }
        
        protected override void Loop()
        {
            if (_isLoopRunning)
            {
                _isLoopRequested = true;
                return;
            }

            _isLoopRunning = true;
            try
            {
                do
                {
                    _isLoopRequested = false;
                    RunLoop();
                }
                while (_isLoopRequested && !IsMistakeAwait);
            }
            finally
            {
                _isLoopRunning = false;
            }
        }

        private void RunLoop(){
            if (IsMistakeAwait)
                return;
            
            for (; IndexGroup < GroupSteps.Length; )
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
                    }
                    
                    for (; IndexCondition < GroupSteps[IndexGroup].Steps[IndexStep].conditions.Count; IndexCondition++)
                    {
                        // инициализация кондишина
                        ConditionBase condition = GroupSteps[IndexGroup].Steps[IndexStep].conditions[IndexCondition];
                        if (condition.State == State.None)
                        {
                            if (!condition.isUsed)
                                continue;
                            
                            ConditionInit(condition, GroupSteps[IndexGroup].Steps[IndexStep]);
                        }
                        
                        if (condition.ServiceType != ServiceType.Info && step.conditionsInitType == InitConditionType.StepByStep)
                            break;
                    }
                    
                    // завершение шага в тренировке/экзамене находится в резолве кондишинов
                    
                    // прерывание цикла при инициализации последнего шага
                    // логика проверки выполнения всех шагов
                    if (IndexStep + 1 >= GroupSteps[IndexGroup].Steps.Length &&
                        !GroupSteps[IndexGroup].CheckCompleteAllSteps())
                    {
                        // тут же происходит инициализация завершающих кондишинов,
                        // для отработки вначале внутренних кондишинов шагов (иногда кондишины могут дублировать друг друга)
                        if (GroupSteps[IndexGroup].ConditionState == State.None)
                        {
                            for (int x = 0; x < GroupSteps[IndexGroup].Conditions.Length; x++)
                            {
                                ConditionInit(GroupSteps[IndexGroup].Conditions[x], null);
                                // Debug.LogError($"===> Init End {GroupSteps[IndexGroup].Steps[IndexStep].conditions[IndexCondition].startInstructions.instructionText}");
                            }

                            GroupSteps[IndexGroup].ConditionState = State.Init;
                        }
                        return;
                    }
                    
                    IndexCondition = 0;
                }
                
                // завершение группы
                if (GroupSteps[IndexGroup].State == State.Init)
                {
                    if (GroupSteps[IndexGroup].CheckCompleteAllSteps())
                    {
                        GroupResolve(GroupSteps[IndexGroup]);
                    }
                    else
                    {
                        IndexStep = 0;
                    }
                    
                    return;
                }

                IndexStep = 0;
                IndexGroup++;
            }
            
            EndScenario?.Invoke();
        }
        
        #region Group

        protected override void GroupInit(GroupSteps group)
        {
            group.Init();
            LogSystem.Log(group);
            Loop();
        }

        protected override void GroupResolve(GroupSteps group)
        {
            group.CompleteGroup();
            LogSystem.Log(group);

            IndexGroup++;
            IndexStep = 0;
            IndexCondition = 0;
            
            Loop();
        }

        #endregion

        #region Step

        protected override void StepInit(StepBase step)
        {
            step.InitStep();
            LogSystem.Log(step);
            
            ConditionPermanentSetup(step);
            step.OnStepInit?.Invoke();
            
            // TODO на сколько корректно запускать трекер если мы запускаем сразу пачку шагов:?
            TrackedManager.OnUpdatedHistory?.Invoke(step);

            if (step.conditions.Count == 0)
            {
                StepResolved(step);
            }
        }

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
            else
            {
                IsMistakeAwait = true;
                
                HintsSystem.FillInfoPanel(step.mistakeInstructions.instructionText);
                MistakeSystem.Mistake(step, () => { MistakeCallback(Restart); });
            }
        }

        #endregion
        
        #region Condition Classic
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="parent">Родительский степ, при значении null шаг является завершающим</param>
        protected override void ConditionInit(ConditionBase condition, StepBase parent)
        {
            // HintsSystem.FillInfoPanel(condition.instructions.instructionText);
            // SoundSystem.Play(condition.instructions.instructionClip, condition.instructions.instructionRepeatDelay, true);
            condition.InitCondition(parent, false);
            
            LogSystem.Log(condition, false);
        }
        
        protected override void ConditionResolved(ConditionBase condition, bool isSuccess)
        {
            if (IsMistakeAwait)
                return;
            
            if (condition.State == State.None)
            {
                // TODO нужно проверить в каких случаях получается такой каламбур
                Debug.LogError($"<color=red>КОД красный сообщи об этой ошибке разработчику системы сценариев</color>");
                return;
            }
            
            
            bool isOrderRequiredMistake = false;

            // Parent step нет у кондишинов проверки выхода из группы шагов
            if (isSuccess && condition.ParentStep != null)
            {
                if (GroupSteps[IndexGroup].IsStepsOrderRequired && condition.ParentStep.IsStepOrderRequired)
                    isSuccess = GroupSteps[IndexGroup].CheckStepOrderRequired(condition.ParentStep);
                if (isSuccess)
                    isSuccess = condition.ParentStep.CheckConditionOrderRequired(condition);
                isOrderRequiredMistake = !isSuccess;
            }

            // Сохранение текущего значения ошибки для сохранения данных в логах о нарушении последовательности
            // TODO необходимо переработать
            EnumMistakeType temp = condition.mistakeType;
            if (isOrderRequiredMistake)
            {
                condition.mistakeType = EnumMistakeType.OrderRequired;
                condition.State = State.Failure;
            }
            LogSystem.Log(condition, false);

            
            if (isSuccess)
            {
                // HintsSystem.FillInfoPanel(condition.success.instructionText);
                // тут нужна проверка на количество кондишинов не завершённых в шаге
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
                        if (condition.ParentStep.conditions[^1] == condition)
                        {
                            StepResolved(condition.ParentStep);
                        }
                        else
                        {
                            for (int x = 0; x < condition.ParentStep.conditions.Count; x++)
                            {
                                // TODO возможен баг если последний кондишн Info
                                if (condition.ParentStep.conditions[x].isUsed &&
                                    condition.ParentStep.conditions[x].State == State.None)
                                {
                                    ConditionInit(condition.ParentStep.conditions[x], condition.ParentStep);
                                    return;
                                }
                            }
                            
                            // если вышли из цикла и не было кондишина на старт выполнения завершаем шаг текущий
                            StepResolved(condition.ParentStep);
                        }
                    }
                }

                //TODO Вылет за пределы группы в момент завершения сценария (трабла внутри последовательности инициализации).
                //Не записывается лог последнего кондишина в группе
                if (IndexGroup >= GroupSteps.Length)
                {
                    // Debug.LogError($"TODO Вылет за пределы группы.");
                    return;
                }
                // Debug.LogError($"Condition: {condition.startInstructions.instructionText}");
                // логика проверки выполнения всех шагов
                if (GroupSteps[IndexGroup].CheckCompleteAllSteps())
                {
                    GroupSteps[IndexGroup].StopAllEndConditions();
                    GroupResolve(GroupSteps[IndexGroup]);
                }
                else if (condition.ParentStep == null)
                {
                    if (GroupSteps[IndexGroup].IsAnyConditionFromEnd ||
                        GroupSteps[IndexGroup].CheckCompleteAllConditions())
                    {
                        IsMistakeAwait = true;
                        
                        HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                        MistakeSystem.Mistake(condition, condition.ParentStep, () => { MistakeCallback(Restart); });
                    }
                }
            }
            else
            {
                IsMistakeAwait = true;
                
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                MistakeSystem.Mistake(condition, condition.ParentStep, () => { MistakeCallback(Restart); });
            }
            
            if (isOrderRequiredMistake)
                condition.mistakeType = temp;
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
                HintsSystem.FillInfoPanel(condition.mistakeInstructions.instructionText);
                // SoundSystem.Play(condition.fail.instructionClip, 0, false);
                
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
            
            GroupSteps[IndexGroup].ResetGroup();
            LogSystem.Log(GroupSteps[IndexGroup]);
            SoundSystem.ClearList();
            
            IndexGroup--;
            IndexStep = 0;
            IndexCondition = 0;
            GroupSteps[IndexGroup].ResetGroup();
            
            Loop();
        }

        public override void Skip()
        {
            if (!HasValidScenario() || !HasValidPosition())
                return;
            
            GroupSteps[IndexGroup].SkipGroup(LogSystem);
            IndexGroup++;
            IndexStep = 0;
            IndexCondition = 0;
            SoundSystem.ClearList();
            
            Loop();
        }

        public override void Restart()
        {
            if (!HasValidScenario() || !HasValidPosition())
                return;
            
            GroupSteps[IndexGroup].ResetGroup();
            IndexStep = 0;
            IndexCondition = 0;
            SoundSystem.ClearList();
            
            Loop();
        }

        #endregion
    }
}