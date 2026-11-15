using System;
using System.Collections;
using System.Collections.Generic;
using _Alex.Utils;
using UnityEngine;

public class HoldHandler : ControllerMoveHandler
{
    private Action _OnComplete;

    public override void HandlingOn(Transform controllerTransform, Transform pivotTransform, Action onComplete = null)
    {
        // _signalsInterpreter.OnSignalResieved?.Invoke(controllerType, 1, this, false);
        OnValueChanged?.Invoke(1);
        _OnComplete = onComplete;
    }

    public override void HandlingOff()
    {
        // _signalsInterpreter.OnSignalResieved?.Invoke(controllerType, 0, this, false);
        _OnComplete?.Invoke();
        OnValueChanged?.Invoke(0);
    }

    public override void ReturnBackWhen<T, U, W>(StateWrapper<T> stateWrapper, U awaitValue_1, W awaitValue_2)
    {
        throw new NotImplementedException();
    }
}
