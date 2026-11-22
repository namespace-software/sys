using _Core._ScenarioSystem.Component.Condition;
using UnityEngine;

namespace _Core.Editor.ScenarioValidator
{
    internal sealed class ButtonPressConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e)
        {
            ConditionValidatorHelpers.RequireReference(c, e, "_hVR_InteractionHelper", "HVR_InteractionHelper", ValidationSeverity.Error);
            bool right = ConditionValidatorHelpers.GetFieldValue(e.Condition, "_allowRightController") is bool r && r;
            bool left = ConditionValidatorHelpers.GetFieldValue(e.Condition, "_allowLeftController") is bool l && l;
            if (!right && !left)
                c.Add("COND050", e.Location + ": ButtonPressCondition запрещает оба контроллера — условие невозможно выполнить.", ValidationSeverity.Error, e.Target);
            ConditionValidatorHelpers.ValidateEnumField(c, e, "_buttonType", "тип кнопки контроллера",
                typeof(ButtonPressCondition.InteractionButtonType), "COND051");
            ConditionValidatorHelpers.ValidateNonNegativeNumber(c, e, "_holdDuration", "длительность удержания кнопки", "COND052");
        }
    }

    internal sealed class WaitConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e)
        {
            object modeValue = ConditionValidatorHelpers.GetFieldValue(e.Condition, "mode");
            int mode = modeValue != null ? System.Convert.ToInt32(modeValue) : 0;
            const int knownModes = 1 | 2 | 4;
            if (mode == 0 || (mode & ~knownModes) != 0)
                c.Add("COND053", e.Location + ": WaitCondition должен иметь хотя бы один корректный режим ожидания (Seconds, Audio, Animation).", ValidationSeverity.Error, e.Target);
            if ((mode & 1) != 0)
                ConditionValidatorHelpers.ValidateNonNegativeNumber(c, e, "waitSeconds", "время ожидания", "COND054");
            if ((mode & 2) != 0)
                ConditionValidatorHelpers.RequireReference(c, e, "waitAudioClip", "AudioClip для режима Audio", ValidationSeverity.Error);
            if ((mode & 4) != 0)
            {
                ConditionValidatorHelpers.RequireReference(c, e, "animator", "Animator для режима Animation", ValidationSeverity.Error);
                if (string.IsNullOrWhiteSpace(ConditionValidatorHelpers.GetFieldValue(e.Condition, "stateName") as string))
                    c.Add("COND055", e.Location + ": для режима Animation не задано имя состояния stateName.", ValidationSeverity.Error, e.Target);
                Animator animator = ConditionValidatorHelpers.GetFieldValue(e.Condition, "animator") as Animator;
                if (animator != null && animator.runtimeAnimatorController == null)
                    c.Add("COND056", e.Location + ": у Animator не назначен RuntimeAnimatorController.", ValidationSeverity.Error, e.Target);
            }
        }
    }

    internal sealed class DistanceInspectConditionValidator : IConditionValidator
    {
        public void Validate(ValidationContext c, ConditionEntry e)
        {
            ConditionValidatorHelpers.RequireReference(c, e, "_hVR_InteractionHelper", "HVR_InteractionHelper", ValidationSeverity.Error);
            ConditionValidatorHelpers.RequireReference(c, e, "_indicatorUI", "DistanceInspectionIndicator", ValidationSeverity.Error);
            object inspectTime = ConditionValidatorHelpers.GetFieldValue(e.Condition, "_inspectTime");
            if (inspectTime is int seconds && seconds <= 0)
                c.Add("COND057", e.Location + ": _inspectTime должен быть больше нуля, иначе расчёт прогресса осмотра некорректен.", ValidationSeverity.Error, e.Target);
        }
    }
}
