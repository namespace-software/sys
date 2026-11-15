using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using _Alex.Utils;
using UnityEngine;
using UnityEngine.Events;

public enum Axis
{
    X,
    Y,
    Z
}

public class MoveHandler : ControllerMoveHandler, IResettable
{
    [SerializeField, PropertyOrder(-1)] private Axis _axisToCheck;
    [SerializeField] private MoveHandler _dependentHandler;
    [SerializeField] private bool _autoHandlingOff;

    private bool _isActive = true;
    private bool _inAction = false;
    private Vector3 _localForwardVector;

    //private MoveHandlerSynchronizator _synchronizator;
    //private MoveHandler _dependentHandler;

    private Coroutine _returnAwaitCoroutine;
    private Coroutine _limitExeedCoroutine;

    private Action _currentAction;

    private bool _directionCorrect;
    private bool _limitExeed;
    private bool _dependentFree;

    public Action<MoveHandler, int> OnIndexChanged;
    public bool IsActive { get => _isActive; }
    public bool InAction { get => _inAction; }

    public override void HandlingOn(Transform controllerTransform, Transform pivotTransform, Action onComplete = null)
    {
        // Debug.Log("HandlingOn [" + ControllerType + "]");
        _controllerTransform = controllerTransform;
        _pivotTransform = pivotTransform;
        _pivotTransform.position = controllerTransform.position;
        _currentAction = onComplete;
        _localForwardVector = GetLocalForwardVector();

        if (!_isActive) return;
        if (_returnAwaitCoroutine != null) { StopCoroutine(_returnAwaitCoroutine); }

        //if (_synchronizator == null) _synchronizator = GetComponent<MoveHandlerSynchronizator>();
        //if (_dependentHandler == null) _dependentHandler = GetComponent<MoveHandler>();

        //_controllerTransform = controllerTransform;
        //_pivotTransform = pivotTransform;
        //_currentAction = onComplete;
        //_localForwardVector = GetLocalForwardVector();

        //_inAction = false;
        _limitExeedCoroutine = StartCoroutine(LimitExceededAwaiter(/*onComplete*/));
    }


    public override void HandlingOff()
    {
        // Debug.Log("HandlingOff for" + controllerType);

        if (_limitExeedCoroutine != null) StopCoroutine(_limitExeedCoroutine);
        _controllerTransform = null;
    }


    private IEnumerator LimitExceededAwaiter(/*Action onComplete*/)
    {
        _inAction = false;

        yield return new WaitUntil(() =>  IsDirectionCorrect() && IsLimitExceeded() && IsDependentHandlerFree());

        _inAction = true;

        //if (!IsDependentHandlerFree()) { _inAction = false; /*yield break; */ yield return new WaitUntil(() => !_dependentHandler.InAction); }

        InitMovingToNextStepPosition();
        SwitchToNextAvailableDirection();
        _currentAction?.Invoke();
    }

    private bool IsDependentHandlerFree()
    {
        _dependentFree = false;
        if (_dependentHandler == null || !_dependentHandler.InAction) _dependentFree = true;
        return _dependentFree;
    }

    private bool IsLimitExceeded()
    {
        if (_pivotTransform == null || _controllerTransform == null) { _limitExeed = false; return false; }

        Vector3 localControllerPosition = _pivotTransform.InverseTransformPoint(_controllerTransform.position);
        Vector3 localPivotPosition = _pivotTransform.InverseTransformPoint(_pivotTransform.position);
        float distance = 0;

        switch (_axisToCheck)
        {
            case Axis.X:
                distance = Mathf.Abs(localControllerPosition.x - localPivotPosition.x);
                break;
            case Axis.Y:
                distance = Mathf.Abs(localControllerPosition.y - localPivotPosition.y);
                break;
            case Axis.Z:
                distance = Mathf.Abs(localControllerPosition.z - localPivotPosition.z);
                break;
        }

        _limitExeed = distance >= _deltaLimit;
        return distance >= _deltaLimit;
    }

    private bool IsDirectionCorrect()
    {
        _directionCorrect = false;
        if (_availableDirection == GetCurrentMoveDirection() || _availableDirection == EnumAvailableDirection.Both) _directionCorrect = true;
        return _directionCorrect;
    }

    private EnumAvailableDirection GetCurrentMoveDirection()
    {
        if (_controllerTransform == null) { /*StopAllCoroutines();*/ return EnumAvailableDirection.None; }

        Vector3 localHandPosition = _pivotTransform.InverseTransformPoint(_controllerTransform.position);
        //Vector3 localForwardVector = _pivotTransform.InverseTransformDirection(_axisToCheck == Axis.Z ? _pivotTransform.forward : -_pivotTransform.right);

        float dotProduct = Vector3.Dot(localHandPosition, _localForwardVector);

        if (dotProduct > 0)
        {
            //Debug.Log("Bias forward at: " + Vector3.Distance(_pivotTransform.position, _controllerTransform.position));
            _prevDirection = EnumAvailableDirection.Forward;
            return EnumAvailableDirection.Forward;
        }
        else if (dotProduct < 0)
        {
            //Debug.Log("Bias backward at: " + Vector3.Distance(_pivotTransform.position, _controllerTransform.position));
            _prevDirection = EnumAvailableDirection.Backward;
            return EnumAvailableDirection.Backward;
        }

        return EnumAvailableDirection.None;
    }

    private void InitMovingToNextStepPosition()
    {
        float directionFactor = (_prevDirection == EnumAvailableDirection.Forward) ? 1 : -1;
        Vector3 axis = new Vector3(_axis.x * directionFactor, _axis.y * directionFactor, _axis.z * directionFactor);
        gameObject.AddComponent<LocalRotator>().GetLocalAnimation(_stepAngle, _animationStepsCount, axis, AnimationEndHandler);

        _stepIndex += (int)directionFactor;
        // _signalsInterpreter.OnSignalResieved?.Invoke(controllerType, _stepIndex, this, true);

        if (_soundComponent != null) _soundComponent.PlayShort();

        OnValueChanged?.Invoke(_stepIndex);
        OnIndexChanged?.Invoke(this, _stepIndex);
    }

    private void SwitchToNextAvailableDirection()
    {
        _availableDirection = _switcher.GetNextDirection(_prevDirection);
    }

    public override void ReturnBackWhen<T, U, W>(StateWrapper<T> stateWrapper, U awaitValue_1, W awaitValue_2)
    {
        //Base method
    }

    // public void ReturnBackWhen(StateWrapper<EnumSterringWeelSide> stateWrapper, EnumSterringWeelSide awaitValue_1, EnumSterringWeelSide awaitValue_2)
    // {
    //     Debug.Log("ReturnBackWhen");
    //     _returnAwaitCoroutine = StartCoroutine(CheckStateWrapper(stateWrapper, awaitValue_1, awaitValue_2));
    // }

    // public IEnumerator CheckStateWrapper(StateWrapper<EnumSterringWeelSide> stateWrapper, EnumSterringWeelSide awaitValue_1, EnumSterringWeelSide awaitValue_2)
    // {
    //     yield return new WaitForSeconds(2f);
    //     yield return new WaitUntil(() => stateWrapper.Value.Equals(awaitValue_1));
    //     yield return new WaitForSeconds(1f);
    //     yield return new WaitUntil(() => stateWrapper.Value.Equals(awaitValue_2));
    //
    //     _prevDirection = _availableDirection;
    //     InitMovingToNextStepPosition();
    //     SwitchToNextAvailableDirection();
    // }

    private void AnimationEndHandler()
    {
        _inAction = false;
        if (_isProhibitedMultipleSteps || _controllerTransform == null || _pivotTransform == null) return;

        _pivotTransform.position = _controllerTransform.position;
        if (!_autoHandlingOff) _limitExeedCoroutine = StartCoroutine(LimitExceededAwaiter());
        else HandlingOff();
    }

    private Vector3 GetLocalForwardVector()
    {
        switch (_axisToCheck)
        {
            case Axis.X: return new Vector3(-1, 0, 0);  // ������ InverseTransformDirection
            case Axis.Y: return new Vector3(0, -1, 0);
            case Axis.Z: return new Vector3(0, 0, 1);
            default: return Vector3.zero;
        }
    }

    public void StopInteraction()
    {
        // Debug.Log("StopInteraction for " + controllerType);
        _isActive = false;
        StopAllCoroutines();
    }

    public void ResumeInteraction()
    {
        //_inAction = false;
        // Debug.Log("ResumeInteraction for " + controllerType);
        _isActive = true;
        _limitExeedCoroutine = StartCoroutine(LimitExceededAwaiter(/*onComplete*/));
    }

    public void ResetToPreviousCondition(int savedIndex)
    {
        StopAllCoroutines();
        int directionFactor = GetDirectionFactor(savedIndex);
        if (directionFactor == 0) { Debug.LogWarning("The same value, reset prohibited!"); return; }

        int resetSteps = GetStepsResetCount(savedIndex);
        Debug.Log("Reset from: " + _stepIndex + " To: " + savedIndex + " | With stepsCount: " + resetSteps);

        for (int i = 0; i < resetSteps; i++)
        {
            Vector3 axis = new Vector3(_axis.x * directionFactor, _axis.y * directionFactor, _axis.z * directionFactor);
            gameObject.AddComponent<LocalRotator>().GetLocalAnimation(_stepAngle, _animationStepsCount, axis, null);

            _stepIndex += (int)directionFactor;
            // _signalsInterpreter.OnSignalResieved?.Invoke(controllerType, _stepIndex, this, false);
            OnValueChanged?.Invoke(_stepIndex);
            OnIndexChanged?.Invoke(this, _stepIndex);

            _prevDirection = directionFactor > 0 ? EnumAvailableDirection.Forward : EnumAvailableDirection.Backward;
            SwitchToNextAvailableDirection();
        }

        //_currentAction?.Invoke();
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
