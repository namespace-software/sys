using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Group;
using _Core._ScenarioSystem.Component.Step;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Core._ScenarioSystem.Module.Logger
{
    /// <summary>
    /// Класс сбора статистики
    /// TODO нужен базовый класс для вариативности
    /// </summary>
    public class LogSystem : MonoBehaviour
    {
        /// <summary>
        /// Стартовое время начало выполнения сценария
        /// </summary>
        private DateTime _startTime;
        
        [Tooltip("Лог всех действий пользователя по системе сценариев")]
        [BoxGroup("Logs")]
        [ListDrawerSettings(
            ListElementLabelName = "Label",
            DefaultExpandedState = false
        )]
        [SerializeField] private List<LogComponent> _logComponents = new List<LogComponent>();

        [Tooltip("Для вывода статистики в режиме реального времени")]
        [BoxGroup("Debug")]
        [SerializeField] private bool _isPrintLogs;

        public DateTime StartTime => _startTime;
        public List<LogComponent> LogComponents => _logComponents;
        
        
        public void Init()
        {
            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Лог группы шагов
        /// </summary>
        /// <param name="group"></param>
        public void Log(GroupSteps group)
        {
            _logComponents.Add(new LogComponent(
                group.GetInstanceID(),
                ComponentType.Group,
                group.State,
                EnumMistakeType.Common,// лишнее TODO надо переделать
                0,
                group.Description,
                "",// лишнее TODO надо переделать
                "",// лишнее TODO надо переделать
                DateTime.Now,
                false));
            
            if (_isPrintLogs)
                PrintLast();
        }

        /// <summary>
        /// Лог шага
        /// </summary>
        /// <param name="step"></param>
        public void Log(StepBase step)
        {
            _logComponents.Add(new LogComponent(
                step.GetInstanceID(),
                ComponentType.Step,
                step.State,
                EnumMistakeType.Common,// лишнее TODO надо переделать
                0,
                step.startInstructions.instructionText,
                step.mistakeInstructions.instructionText,
                step.successInstructions.instructionText,
                DateTime.Now,
                false));
            
            if (_isPrintLogs)
                PrintLast();
        }

        /// <summary>
        /// Лог кондишина
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isPermanent">Перманентный кондишн?</param>
        public void Log(ConditionBase condition, bool isPermanent)
        {
            Debug.Log(
    $"LOG: {condition.startInstructions.instructionText} | " +
    $"Hash={condition.GetHashCode()} | " +
    $"State={condition.State} | " +
    $"Frame={Time.frameCount}");
            _logComponents.Add(new LogComponent(
                condition.GetHashCode(),
                isPermanent ? ComponentType.ConditionPermanent : ComponentType.Condition,
                condition.State,
                condition.mistakeType,
                condition.penaltyPoints,
                condition.startInstructions.instructionText,
                condition.mistakeInstructions.instructionText,
                condition.successInstructions.instructionText,
                DateTime.Now,
                condition.ExcludeFromReport));

            if (_isPrintLogs)
                PrintLast();
        }

        private void PrintLast()
        {
            LogComponent last = _logComponents[^1];
            Debug.LogError($"<color=cyan>{GetType(last.Type)}</color>" +
                           $"<color=yellow>[{GetState(last.State)}]</color>" +
                           $"<color=green>{last.GuidId}|{last.Info}</color>>");
        }

        private string GetType(ComponentType type)
        {
            switch (type)
            {
                case ComponentType.Group:       return "Group\t\t";
                case ComponentType.Step:        return "Step\t\t";
                case ComponentType.Condition:   return "Condition\t";
                default:                        return "Permanent\t";
            }
        }

        private string GetState(State state)
        {
            switch (state)
            {
                case State.Init:    return "Init\t\t";
                case State.Success: return "Success\t";
                case State.Failure: return "Failure\t";
                case State.Skipped: return "Skipped\t";
                default:            return "None\t";
            }
        }
    }
}
