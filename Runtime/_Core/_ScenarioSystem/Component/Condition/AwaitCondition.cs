using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using _Core._ScenarioSystem.Component.Step;
using Sirenix.OdinInspector;
using UnityEngine;

public class AwaitCondition : ConditionBase
{
    [Tooltip("Ссылка на Button в сцене")]
    [BoxGroup("Trigger"), PropertyOrder(100)]
    [SerializeField] private StepBase _step;

    /// <summary>
    /// Сохранение значения видимости кнопки при инициализации, для ресета
    /// </summary>
    private bool _isStartVisibleButton;

    public override void SetupCondition()
    {
        if (State != State.None)
            return;
        
        _isStartVisibleButton = _step.gameObject.activeSelf;
        
        _step.OnStepResolved.AddListener(OnStepHandler);
    }

    public override void DisableCondition()
    {
        base.DisableCondition();
        
        _step.OnStepResolved.RemoveListener(OnStepHandler);
    }

    public override void ResetConditionToDefault()
    {
        if (State == State.None)
            return;
        
        base.ResetConditionToDefault();
        
        _step.gameObject.SetActive(_isStartVisibleButton);
    }

    private void OnStepHandler()
    {
        if (State != State.Init)
            return;

        bool isSuccess = CanResolveAsSuccess();
        State = isSuccess ? State.Success : State.Failure;
        
        DisableCondition();
      
        OnConditionResolved?.Invoke();
        
        // тут может быть перезапуск в системе сценариев поэтому запускаем последним
        OnResolved?.Invoke(this, CanResolveAsSuccess());
    }
}
