using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Components;

public class HVR_SimpleButtonHelper : MonoBehaviour
{
    private HVRPhysicsButton _hVRPhysicsButton;
    // private SignalsInterpreterBase _signalsInterpreter;

    // [SerializeField] private EnumControllerType _controllerType;

    private void Awake()
    {
        // _signalsInterpreter = FindFirstObjectByType<SignalsInterpreterBase>();
        _hVRPhysicsButton = GetComponentInChildren<HVRPhysicsButton>();
    }

    private void OnEnable()
    {
        _hVRPhysicsButton.ButtonDown.AddListener(PressDown);
        _hVRPhysicsButton.ButtonUp.AddListener(PressUp);
    }

    private void OnDisable()
    {
        _hVRPhysicsButton.ButtonDown.RemoveAllListeners();
        _hVRPhysicsButton.ButtonUp.RemoveAllListeners();
    }

    private void PressDown(HVRPhysicsButton hVRPhysicsButton)
    {
        // Debug.Log(_controllerType + " Is pressed", hVRPhysicsButton.transform);
        // _signalsInterpreter.OnSignalResieved?.Invoke(_controllerType, -1, null, false);
    }

    public void PressUp(HVRPhysicsButton hVRPhysicsButton)
    {
        //Debug.Log(_controllerType + " Is up", hVRPhysicsButton.transform);
        //Button up up handler
    }
}
