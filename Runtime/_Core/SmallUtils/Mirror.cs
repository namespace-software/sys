using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    [SerializeField] private List<GameObject> _mirrorCanvasGOs;
    [SerializeField] private List<GameObject> _mirrorCameraGOs;

    [SerializeField] private bool _alwaysOn;

    // private VehicleStateMachineBase _stateMachine;

    private void Awake()
    {
        // _stateMachine = FindFirstObjectByType<VehicleStateMachineBase>();
    }

    private void OnEnable()
    {
        // _stateMachine.OnTransmissionStateChanged += MirrorActivator;
    }

    private void OnDisable()
    {
        // _stateMachine.OnTransmissionStateChanged -= MirrorActivator;
    }

    // private void MirrorActivator(EnumTransmission state)
    // {
    //     if (_stateMachine.EngineOn && state == EnumTransmission.Reverse)
    //     {
    //         foreach (var item in _mirrorCanvasGOs)
    //         {
    //             item.SetActive(true);
    //         }
    //
    //         foreach (var item in _mirrorCameraGOs)
    //         {
    //             item.SetActive(true);
    //         }
    //     }
    //     else
    //     {
    //         foreach (var item in _mirrorCanvasGOs)
    //         {
    //             item.SetActive(_alwaysOn);
    //         }
    //
    //         foreach (var item in _mirrorCameraGOs)
    //         {
    //             item.SetActive(_alwaysOn);
    //         }
    //     }
    // }
}
