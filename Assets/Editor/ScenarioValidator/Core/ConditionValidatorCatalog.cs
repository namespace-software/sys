using System;
using System.Collections.Generic;
using _Core._ScenarioSystem.Component.Condition;

namespace _Core.Editor.ScenarioValidator
{
    /// <summary>Каталог специфичных валидаторов. Регистрация выполняется по точному runtime-типу условия.</summary>
    public static class ConditionValidatorCatalog
    {
        private static readonly Dictionary<Type, IConditionValidator> Validators =
            new Dictionary<Type, IConditionValidator>
            {
                { typeof(ButtonCondition), new ButtonConditionValidator() },
                { typeof(AwaitCondition), new AwaitConditionValidator() },
                { typeof(GrabCondition), new GrabConditionValidator() },
                { typeof(TriggerCondition), new TriggerConditionValidator() },
                { typeof(PositionCondition), new PositionConditionValidator() },
                { typeof(EventCondition), new EventConditionValidator() },
                { typeof(ButtonPressCondition), new ButtonPressConditionValidator() },
                { typeof(WaitCondition), new WaitConditionValidator() },
                { typeof(DistanceInspectCondition), new DistanceInspectConditionValidator() }
            };

        public static void Validate(ValidationContext context, ConditionEntry entry)
        {
            if (entry == null || entry.Condition == null) return;
            if (Validators.TryGetValue(entry.Condition.GetType(), out IConditionValidator validator))
                validator.Validate(context, entry);
        }

        /// <summary>Добавляет или заменяет валидатор для конкретного типа.</summary>
        public static void Register<TCondition>(IConditionValidator validator)
            where TCondition : ConditionBase
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            Validators[typeof(TCondition)] = validator;
        }

        public static bool Unregister<TCondition>() where TCondition : ConditionBase
        {
            return Validators.Remove(typeof(TCondition));
        }

        public static bool HasValidator<TCondition>() where TCondition : ConditionBase
        {
            return Validators.ContainsKey(typeof(TCondition));
        }
    }
}
