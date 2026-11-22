using System;
using System.Collections;
using _Core._ScenarioSystem.Component.Condition;
using UnityEngine;

namespace _Core.Editor.ScenarioValidator
{
    internal sealed class ButtonConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e) =>
            ConditionValidatorHelpers.RequireReference(c, e, "_button", "Button", ValidationSeverity.Error);
    }

    internal sealed class AwaitConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e) =>
            ConditionValidatorHelpers.RequireReference(c, e, "_step", "StepBase для ожидания", ValidationSeverity.Error);
    }

    internal sealed class GrabConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e) =>
            ConditionValidatorHelpers.RequireReference(c, e, "_hVR_InteractionHelper", "HVR_InteractionHelper", ValidationSeverity.Error);
    }

    internal sealed class TriggerConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e)
        {
            ConditionValidatorHelpers.RequireReference(c, e, "_collider", "Collider", ValidationSeverity.Error);
            IList tags = ConditionValidatorHelpers.GetFieldValue(e.Condition, "_tags") as IList;
            if (tags == null || tags.Count == 0)
                c.Add("COND030", e.Location + ": TriggerCondition не содержит ни одного тега; условие не сможет совпасть с объектом.", ValidationSeverity.Warning, e.Target);
        }
    }

    internal sealed class PositionConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e)
        {
            ConditionValidatorHelpers.RequireReference(c, e, "itemTransform", "Transform объекта", ValidationSeverity.Error);
            ConditionValidatorHelpers.RequireReference(c, e, "targetTransform", "Transform цели", ValidationSeverity.Error);
            object distance = ConditionValidatorHelpers.GetFieldValue(e.Condition, "_distance");
            if (distance is float value && value < 0f)
                c.Add("COND031", e.Location + ": дистанция PositionCondition отрицательная.", ValidationSeverity.Error, e.Target);
        }
    }

    internal sealed class EventConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e)
        {
            object provider = ConditionValidatorHelpers.GetFieldValue(e.Condition, "_stateProvider");
            if (!(provider is MonoBehaviour behaviour))
                c.Add("COND032", e.Location + ": не назначен MonoBehaviour-поставщик события.", ValidationSeverity.Error, e.Target);
            else
            {
                bool implementsProvider = false;
                foreach (Type implemented in behaviour.GetType().GetInterfaces())
                    if (implemented.FullName == "_Alex.Utils.IBooleanStateProvider") { implementsProvider = true; break; }
                if (!implementsProvider)
                    c.Add("COND033", e.Location + ": компонент _stateProvider не реализует IBooleanStateProvider.", ValidationSeverity.Error, e.Target);
            }
        }
    }
}
