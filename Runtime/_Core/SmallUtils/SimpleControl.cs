using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleControl : MonoBehaviour
{
    // [SerializeField] private EnumControllerType _controllerType;
    [SerializeField] private Vector3 _localEuler_0;
    [SerializeField] private Vector3 _localEuler_1;

    [SerializeField] private RotateHandler rotateHandler;

    // private SignalsInterpreterBase _interpreterBase;

    private int index;

    private void Awake()
    {
        // _interpreterBase = FindFirstObjectByType<SignalsInterpreterBase>();
    }

    public void NextPosition()
    {

    }

    public void PrevPosition()
    {

    }

    public void SwitchPosition()
    {
        if (index == 0) index = 1;
        else index = 0;

        //rotateHandler.ManualSwitch();
        // _interpreterBase.OnSignalResieved?.Invoke(_controllerType, index, GetComponent<ControllerMoveHandler>(), false);
        transform.localEulerAngles = (index == 0) ? _localEuler_0 : _localEuler_1;
    }
}
