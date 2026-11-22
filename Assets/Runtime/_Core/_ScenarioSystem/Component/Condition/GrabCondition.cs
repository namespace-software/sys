using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Реализация кондишина проверки захвата, отпускания и задержки объекта
/// </summary>
public class GrabCondition : ConditionBase
{
    [Tooltip("Компонент HVR_InteractionHelper, который будет передавать сигнал о взаимодействии")]
    [BoxGroup("Trigger"), PropertyOrder(100)]
    [SerializeField] private HVR_InteractionHelper _hVR_InteractionHelper;
    [BoxGroup("Trigger"), PropertyOrder(101)]
    [SerializeField] protected GrabConditionEventType _conditionEventType;

    [ShowIf("@this._conditionEventType == GrabConditionEventType.HoldTime")]
    [BoxGroup("Trigger"), PropertyOrder(102)]
    [SerializeField] private float _holdedTime;

    public override void CheckCondition()
    {
        base.CheckCondition();
    }

    public override void SetupCondition()
    {
        _hVR_InteractionHelper.DeactivateOutlineManual = true;

        switch (_conditionEventType)
        {
            case GrabConditionEventType.Grab:
                _hVR_InteractionHelper.OnGrabbedEvent.AddListener(OnHandler);
                break;
            case GrabConditionEventType.Release:
                _hVR_InteractionHelper.OnReleasedEvent.AddListener(OnHandler);
                break;
            case GrabConditionEventType.HoldTime:
                //_hVR_InteractionHelper.OnAnyInteraction.AddListener(OnInteractionHandler);
                break;
            default:
                break;
        }
    }

    public override void DisableCondition()
    {
        base.DisableCondition();
        //if (_hVR_InteractionHelper.InactiveAfterUsing) _hVR_InteractionHelper.OutlineDisable(true);
        _hVR_InteractionHelper.DeactivateOutlineManual = false;

        switch (_conditionEventType)
        {
            case GrabConditionEventType.Grab:
                _hVR_InteractionHelper.OnGrabbedEvent.RemoveListener(OnHandler);
                break;
            case GrabConditionEventType.Release:
                _hVR_InteractionHelper.OnReleasedEvent.RemoveListener(OnHandler);
                break;
            case GrabConditionEventType.HoldTime:
                //_hVR_InteractionHelper.OnAnyInteraction.RemoveAllListeners();
                break;
            default:
                break;
        }
    }

    public override void ResetConditionToDefault()
    {
        if (State == State.None)
            return;
        
        base.ResetConditionToDefault();
    }

    protected virtual void OnHandler()
    {
        if (State != State.Init)
            return;
        
        if (_conditionEventType is GrabConditionEventType.Grab or GrabConditionEventType.Release)
        {
            State = State.Success;
            
            DisableCondition();
            
            OnConditionResolved?.Invoke();

            Debug.Log("Grabbed/Released");
            OnResolved?.Invoke(this, CanResolveAsSuccess());
        }
    }
}
