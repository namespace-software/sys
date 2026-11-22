using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.ControllerInput;

public class HVR_Char_ControllerDisabler : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Rigidbody[] _rigidbodies;
    private HVRPlayerInputs _playerInputs;
    private Vector3 _lastTransformPosition;

    private void Awake()
    {
        _characterController = GetComponentInChildren<CharacterController>();
        _lastTransformPosition = _characterController.transform.position;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        _rigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_playerInputs.LeftController.JoystickButtonState.Value > 0) _lastTransformPosition = _characterController.transform.position;
        else _characterController.transform.position = _lastTransformPosition;
    }
}
