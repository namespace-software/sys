using _Core._ScenarioSystem.Component.Condition;

namespace _Core.Editor.ScenarioValidator
{
    /// <summary>Проверяет специфичные настройки одного типа условия.</summary>
    public interface IConditionValidator
    {
        void Validate(ValidationContext context, ConditionEntry entry);
    }
}
