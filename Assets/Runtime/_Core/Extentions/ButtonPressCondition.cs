using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Shared;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class ButtonPressCondition : ConditionBase
{
    public enum InteractionButtonType
    {
        Trigger,
        Grip,
        Primary,
        Secondary
    }

    [BoxGroup("General"), PropertyOrder(100)]
    [Tooltip("Компонент HVR_InteractionHelper, по которому нужно кликнуть")]
    [SerializeField] private HVR_InteractionHelper _hVR_InteractionHelper;

    [BoxGroup("General"), PropertyOrder(101)]
    [Tooltip("Каким контроллером можно кликнуть")]
    [SerializeField] private bool _allowRightController = true;

    [BoxGroup("General"), PropertyOrder(102)]
    [SerializeField] private bool _allowLeftController = true;

    [BoxGroup("General"), PropertyOrder(103)]
    [Tooltip("Кнопка, которой нужно кликнуть")]
    [SerializeField] private InteractionButtonType _buttonType = InteractionButtonType.Trigger;

    [BoxGroup("General"), PropertyOrder(104), MinValue(0f)]
    [Tooltip("Сколько секунд нужно удерживать кнопку. 0 — обычный клик")]
    [SerializeField] private float _holdDuration = 0f;

    private Coroutine _clickRoutine;
    private bool _isPointered;

    public override void SetupCondition()
    {
        _isPointered = false;

        _hVR_InteractionHelper.DeactivateOutlineManual = true;
        _hVR_InteractionHelper.OnPointerEnter.AddListener(OnRayEnter);
        _hVR_InteractionHelper.OnPointerExit.AddListener(OnRayExit);

        _clickRoutine = _hVR_InteractionHelper.StartCoroutine(WaitClickRoutine());
    }

    private IEnumerator WaitClickRoutine()
    {
        while (true)
        {
            yield return null;

            if (!_isPointered || HVRInputManager.Instance == null)
                continue;

            if (_allowRightController && TryGetJustActivatedController(HVRInputManager.Instance.RightController, out HVRController rightController))
            {
                yield return HoldRoutine(rightController);
                if (State == State.Success)
                    yield break;
            }

            if (_allowLeftController && TryGetJustActivatedController(HVRInputManager.Instance.LeftController, out HVRController leftController))
            {
                yield return HoldRoutine(leftController);
                if (State == State.Success)
                    yield break;
            }
        }
    }

    private IEnumerator HoldRoutine(HVRController controller)
    {
        float elapsed = 0f;

        while (elapsed < _holdDuration)
        {
            yield return null;

            if (!_isPointered || !IsButtonActive(controller))
                yield break;

            elapsed += Time.deltaTime;
        }

        OnClicked();
    }

    private bool TryGetJustActivatedController(HVRController controller, out HVRController result)
    {
        result = controller;

        if (controller == null)
            return false;

        return GetButtonState(controller).JustActivated;
    }

    private bool IsButtonActive(HVRController controller)
    {
        if (controller == null)
            return false;

        return GetButtonState(controller).Active;
    }

    private HVRButtonState GetButtonState(HVRController controller)
    {
        return _buttonType switch
        {
            InteractionButtonType.Trigger => controller.TriggerButtonState,
            InteractionButtonType.Grip => controller.GripButtonState,
            InteractionButtonType.Primary => controller.PrimaryButtonState,
            InteractionButtonType.Secondary => controller.SecondaryButtonState,
            _ => default,
        };
    }

    private void OnRayEnter()
    {
        _isPointered = true;
    }

    private void OnRayExit()
    {
        _isPointered = false;
    }

    private void OnClicked()
    {
        _clickRoutine = null;

        State = State.Success;

        OnConditionResolved?.Invoke();
        OnResolved?.Invoke(this, CanResolveAsSuccess());

        DisableCondition();
    }

    public override void DisableCondition()
    {
        _isPointered = false;

        if (_clickRoutine != null)
        {
            _hVR_InteractionHelper.StopCoroutine(_clickRoutine);
            _clickRoutine = null;
        }

        _hVR_InteractionHelper.OnPointerEnter.RemoveListener(OnRayEnter);
        _hVR_InteractionHelper.OnPointerExit.RemoveListener(OnRayExit);
        _hVR_InteractionHelper.DeactivateOutlineManual = false;

        base.DisableCondition();
    }
}