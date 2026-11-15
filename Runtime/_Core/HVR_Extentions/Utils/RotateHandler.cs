using System;
using System.Collections;
using _Alex.Utils;
using UnityEngine;

public class RotateHandler : ControllerMoveHandler, IResettable
{
    private Quaternion _initialControllerRotation;
    private Action _OnComplete;

    private Coroutine _limitExeedCoroutine;

    [SerializeField] private Transform _rightControllerJoint;

    public override void HandlingOn(Transform controllerTransform, Transform pivotTransform, Action onComplete = null)
    {
        _controllerTransform = _isKey ? _rightControllerJoint : controllerTransform;
        _pivotTransform = pivotTransform;

        // Сохраняем начальное вращение контроллера
        _initialControllerRotation = _controllerTransform.localRotation;

        if (_isKey)
        {
            // Определяем ось, вокруг которой происходит вращение
            Vector3 rotationAxis = _rightControllerJoint.forward;

            // Получаем направления для расчета угла
            Vector3 controllerForward = _rightControllerJoint.up;
            Vector3 handModelForward = _handModelTransform.up;

            // Вычисляем начальную разницу углов между руками
            _initialAngleDifference = Vector3.SignedAngle(handModelForward, controllerForward, rotationAxis);
        }

        _OnComplete = onComplete;
        _limitExeedCoroutine = StartCoroutine(LimitExceededAwaiter());
    }

    public override void HandlingOff()
    {
        if (_limitExeedCoroutine != null) StopCoroutine(_limitExeedCoroutine);
        _controllerTransform = null;
    }

    private IEnumerator LimitExceededAwaiter()
    {
        yield return new WaitUntil(() => (IsLimitExceeded() && IsDirectionCorrect()));
        InitMovingToNextStepPosition();
        SwitchToNextAvailableDirection();
        _OnComplete?.Invoke();
    }

    public void ManualSwitch()
    {
        InitMovingToNextStepPosition();
        SwitchToNextAvailableDirection();
        _OnComplete?.Invoke();
    }

    // Переменная для хранения начального угла поворота
    private float _initialAngleDifference = 0f;
    public Transform _handModelTransform;
    public int totalAngle;
    private bool IsLimitExceeded()
    {
        float angleDifference = 0;

        if (!_isKey)
        {
            // Вычисляем разницу между текущим и начальным вращением контроллера
            Quaternion deltaRotation = Quaternion.Inverse(_initialControllerRotation) * _controllerTransform.localRotation;
            Vector3 initialForward = _initialControllerRotation * _axisDependedForward;
            Vector3 currentForward = _controllerTransform.localRotation * _axisDependedForward;

            angleDifference = Vector3.SignedAngle(initialForward, currentForward, _isKey ? _axisDependedForward : _axis);
        }
        else
        {
            // Определяем ось, вокруг которой происходит вращение
            Vector3 rotationAxis = _rightControllerJoint.forward;

            // Получаем направления для расчета угла
            Vector3 controllerForward = _rightControllerJoint.up; // Используем up, так как forward указывает вверх контроллера
            Vector3 handModelForward = _handModelTransform.up; // То же самое

            // Вычисляем текущий угол между направлениями рук
            angleDifference = (Vector3.SignedAngle(handModelForward, controllerForward, rotationAxis) - _initialAngleDifference);
        }

        totalAngle = Mathf.Abs((int)angleDifference);

        if (angleDifference > 0)
        {
            _prevDirection = EnumAvailableDirection.Forward;
        }
        else if (angleDifference < 0)
        {
            _prevDirection = EnumAvailableDirection.Backward;
        }

        return totalAngle >= _deltaLimit;
    }

    private bool IsDirectionCorrect()
    {
        return _availableDirection == GetCurrentRotateDirection() || _availableDirection == EnumAvailableDirection.Both;
    }

    private EnumAvailableDirection GetCurrentRotateDirection()
    {
        return _prevDirection;
    }

    private void InitMovingToNextStepPosition()
    {
        float directionFactor = (_prevDirection == EnumAvailableDirection.Forward) ? 1 : -1;
        Vector3 axis = new Vector3(_axis.x * directionFactor, _axis.y * directionFactor, _axis.z * directionFactor);
        gameObject.AddComponent<LocalRotator>().GetLocalAnimation(_stepAngle, _animationStepsCount, axis, AnimationEndHandler);

        _stepIndex += (int)directionFactor;
        _signalsInterpreter.OnSignalResieved?.Invoke(controllerType, _stepIndex, Vector2.zero, true);

        if (_soundComponent != null) _soundComponent.PlayShort();

        OnValueChanged?.Invoke(_stepIndex);
    }

    private void SwitchToNextAvailableDirection()
    {
        _availableDirection = _switcher.GetNextDirection(_prevDirection);
    }

    public override void ReturnBackWhen<T, U, W>(StateWrapper<T> stateWrapper, U awaitValue_1, W awaitValue_2)
    {
        throw new NotImplementedException();
    }

    private void AnimationEndHandler()
    {
        if (_isProhibitedMultipleSteps || _controllerTransform == null) return;

        _initialControllerRotation = _controllerTransform.localRotation;
        _limitExeedCoroutine = StartCoroutine(LimitExceededAwaiter());
    }

    public void ResetToPreviousCondition(int savedIndex)
    {
        int directionFactor = GetDirectionFactor(savedIndex);
        if (directionFactor == 0) { Debug.LogError("The same value, reset prohibited!"); return; }

        int resetSteps = GetStepsResetCount(savedIndex);
        // Debug.Log("Reset from: " + _stepIndex + " To: " + savedIndex + " | With stepsCount: " + resetSteps + " | controllerType: " + controllerType);

        for (int i = 0; i < resetSteps; i++)
        {
            Vector3 axis = new Vector3(_axis.x * directionFactor, _axis.y * directionFactor, _axis.z * directionFactor);
            gameObject.AddComponent<LocalRotator>().GetLocalAnimation(_stepAngle, _animationStepsCount, axis, null);

            _stepIndex += (int)directionFactor;
            _signalsInterpreter.OnSignalResieved?.Invoke(controllerType, _stepIndex, Vector2.zero, false);
            OnValueChanged?.Invoke(_stepIndex);

            _prevDirection = directionFactor > 0 ? EnumAvailableDirection.Forward : EnumAvailableDirection.Backward;
            SwitchToNextAvailableDirection();
            _OnComplete?.Invoke();
        }
    }

    private int GetDirectionFactor(int previousIndex)
    {
        if (_stepIndex == previousIndex) return 0;
        else if (_stepIndex > previousIndex) return -1;
        else return 1;
    }

    private int GetStepsResetCount(int previousIndex)
    {
        return Mathf.Abs(Mathf.Abs(_stepIndex) - Mathf.Abs(previousIndex));
    }
}
