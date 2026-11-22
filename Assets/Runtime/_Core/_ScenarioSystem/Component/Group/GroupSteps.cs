using System;
using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Step;
using _Core._ScenarioSystem.Module.Logger;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Core._ScenarioSystem.Component.Group
{
    public class GroupSteps : MonoBehaviour 
    {
        [BoxGroup("General"), PropertyOrder(0)]
        [Tooltip("Описание группы шагов. Если описания нет берётся название gameObject")]
        public string Description;
        [BoxGroup("General"), PropertyOrder(1), ReadOnly]
        [Tooltip("Список шагов используемых этой группой шагов")]
        public StepBase[] Steps;

        [BoxGroup("General"), PropertyOrder(2)]
        [Tooltip("Требуется ли проверка последовательности выполнения шагов?")]
        public bool IsStepsOrderRequired;

        [BoxGroup("End"), PropertyOrder(100)]
        [Tooltip("Условие. Если выбрано то любой закрытый завершающий кондишн завершит выполнение группы шагов.")]
        public bool IsAnyConditionFromEnd;
        [BoxGroup("End"), PropertyOrder(101)]
        [Tooltip("Условие. Список завершающих кондишнов." +
                 "При закрытии (всех/или одного из всех - зависит от параметра выше) завершит выполнение группы шагов.")]
        [ListDrawerSettings(ListElementLabelName = "ListLabel")]
        [LabelWidth(250)]
        [SerializeReference] public ConditionBase[] Conditions = Array.Empty<ConditionBase>();

        [BoxGroup("Debug"), PropertyOrder(200)]
        [Tooltip("Текущий статус выполнения группы шагов")]
        [SerializeField, ReadOnly] private State _state;

        private State _conditionState;
        
        public State State
        {
            get => _state;
            private set => _state = value;
        }

        public State ConditionState
        {
            get => _conditionState;
            set => _conditionState = value;
        }
        
        private void Start()
        {
            if (Description == "") Description = gameObject.name;
            
            // Собираем шаги в кучу
            List<StepBase> temp = new List<StepBase>();
            
            for (int x = 0; x < transform.childCount; x++)
            {
                StepBase tempStep = transform.GetChild(x).GetComponent<StepBase>();
                if (tempStep != null && tempStep.gameObject.activeSelf)
                    temp.Add(tempStep);
            }
            
            Steps = temp.ToArray();
        }

        public void Init()
        {
            State = State.Init;
            ConditionState = State.None;
        }

        public void CompleteGroup()
        {
            // визуальные хинты и другое

            for (int x = 0; x < Steps.Length; x++)
            {
                if (Steps[x].isUsed)
                {
                    if (Steps[x].State == State.Failure)
                    {
                        State = State.Failure;
                        return;
                    }
                    else if (Steps[x].State == State.Skipped)
                    {
                        State = State.Skipped;
                        return;
                    }
                }
            }

            State = State.Success;
            ConditionState = State.None;
        }

        public void ResetGroup()
        {
            for (int x = 0; x < Steps.Length; x++)
            {
                if (Steps[x].isUsed)
                {
                    Steps[x].ResetStep();
                }
            }

            for (int x = 0; x < Conditions.Length; x++)
            {
                Conditions[x].ResetConditionToDefault();
            }
            
            State = State.None;
            ConditionState = State.None;
        }

        public void SkipGroup(LogSystem logSystem)
        {
            for (int x = 0; x < Steps.Length; x++)
            {
                if (Steps[x].isUsed && Steps[x].State is State.None or State.Init)
                {
                    Steps[x].SkipStep(logSystem);
                }
            }
            State = State.Skipped;
            ConditionState = State.None;
            logSystem.Log(this);
        }

        public bool CheckCompleteAllSteps()
        {
            for (int x = 0; x < Steps.Length; x++)
            {
                if (Steps[x].isUsed && (Steps[x].State is State.None or State.Init))
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckCompleteAllConditions()
        {
            for (int x = 0; x < Conditions.Length; x++)
            {
                if (Conditions[x].isUsed && (Conditions[x].State is State.None or State.Init))
                {
                    return false;
                }
            }
            return true;
        }

        public void StopAllEndConditions()
        {
            for (int x = 0; x < Conditions.Length; x++)
            {
                Conditions[x].State = State.None;
            }
        }

        /// <summary>
        /// Проверка выполнения последовательности шагов внутри группы шагов
        /// </summary>
        public bool CheckStepOrderRequired(StepBase step)
        {
            for (int x = 0; x < Steps.Length; x++)
            {
                if (Steps[x] == step)
                    break;

                if (Steps[x].IsStepOrderRequired && Steps[x].State is State.Init)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
