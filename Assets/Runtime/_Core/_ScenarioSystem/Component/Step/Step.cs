using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Module.Logger;

namespace _Core._ScenarioSystem.Component.Step
{
    [System.Serializable]
    public class Step : StepBase
    {
        public override void InitStep()
        {
            // _isStartEventsCompleted = true;
            State = State.Init;
        }

        public override void CompleteStep()
        {
            // визуальные хинты и другое

            for (int x = 0; x < conditions.Count; x++)
            {
                if (conditions[x].isUsed)
                {
                    if (conditions[x].State == State.Failure)
                    {
                        State = State.Failure;
                        return;
                    }
                    else if (conditions[x].State == State.Skipped)
                    {
                        State = State.Skipped;
                        return;
                    }
                }
            }

            State = State.Success;
        }

        public override void FailOutside()
        {
            // ???
        }

        public override bool CheckConditionOrderRequired(ConditionBase condition)
        {
            if (IsConditionOrderRequired)
            {
                for (int x = 0; x < conditions.Count; x++)
                {
                    if (condition.ParentStep.conditions[x] == condition)
                        return true;
                    
                    if (condition.ParentStep.conditions[x].State is State.Init)
                    {
                        condition.State = State.Failure;
                        return false;
                    }
                }
            }

            return true;
        }

        public override void ResetStep()
        {
            for (int x = 0; x < conditions.Count; x++)
            {
                if (conditions[x].isUsed)
                {
                    conditions[x].ResetConditionToDefault();
                }
            }
            OnStepReset?.Invoke();
            State = State.None;
        }

        public override void SkipStep(LogSystem logSystem)
        {
            OnStepInit?.Invoke();
            for (int x = 0; x < conditions.Count; x++)
            {
                if (conditions[x].isUsed && conditions[x].State is State.None or State.Init)
                {
                    conditions[x].SkipCondition();
                    logSystem.Log(conditions[x], false);
                }
            }
            State = State.Skipped;
            logSystem.Log(this);
            OnStepResolved?.Invoke();
        }

        public override bool CheckCompleteAllConditions()
        {
            for (int x = 0; x < conditions.Count; x++)
            {
                if (conditions[x].isUsed && (conditions[x].State is State.None or State.Init))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
