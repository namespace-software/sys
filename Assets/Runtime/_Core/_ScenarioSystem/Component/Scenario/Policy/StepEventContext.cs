using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Step;

namespace ScenarioSystems
{
    /// <summary>
    /// Тип события, передаваемого шагом во внешний обработчик.
    /// Используется для обозначения ключевых этапов жизненного цикла шага и condition.
    /// </summary>
    public enum StepEventType
    {
        StepInitialized = 0,
        ConditionInitialized = 1,
        ConditionResolved = 2,
        StepResolved = 3
    }

    /// <summary>
    /// Результат события шага или condition.
    /// Используется для передачи результата обработки во внешний контекст.
    /// </summary>
    public enum StepEventResult
    {
        None = 0,
        Success = 1,
        Fail = 2,
        OrderFail = 3
    }

    /// <summary>
    /// Контекст события шага.
    /// Содержит тип события, результат, а также ссылки на шаг и condition,
    /// связанные с данным этапом обработки.
    /// </summary>
    public readonly struct StepEventContext
    {
        public readonly StepEventType EventType;
        public readonly StepEventResult Result;
        public readonly StepBase Step;
        public readonly ConditionBase Condition;

        public StepEventContext(StepEventType stepEventType, StepEventResult result, StepBase step, ConditionBase condition)
        {
            EventType = stepEventType;
            Result = result;
            Step = step;
            Condition = condition;
        }
    }
}