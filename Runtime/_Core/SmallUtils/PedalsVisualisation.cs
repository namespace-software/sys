using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Alex.Sound;
using HurricaneVR.Framework.ControllerInput;

public class PedalsVisualisation : MonoBehaviour
{
    private enum PedalType
    {
        Brake = 0,
        Acceleration = 1
    }

    private HVRPlayerInputs _playerInputs;
    // private VehicleStateMachineBase _stateMachine;
    [SerializeField] private float _movingAngleRange;
    [SerializeField] private PedalType pedalType;
    [SerializeField] private bool _deviceInput;

    // private SteeringWheelIntegration _wheelIntegration;
    private SoundSystem _soundSystem;
    private bool _isBrakeSound;
    private bool _isAccelerateSound;

    private void Awake()
    {
        _playerInputs = FindFirstObjectByType<HVRPlayerInputs>();
        _soundSystem = FindFirstObjectByType<SoundSystem>();
        // _stateMachine = FindFirstObjectByType<VehicleStateMachineBase>();
        // _wheelIntegration = FindFirstObjectByType<SteeringWheelIntegration>();

        // _deviceInput = _wheelIntegration != null;
    }

    //private void OnEnable()
    //{
    //    _stateMachine.OnPlayerSit += PlayerSitHandler;
    //}

    //private void OnDisable()
    //{
    //    _stateMachine.OnPlayerSit -= PlayerSitHandler;
    //}

    private void Update()
    {
        if (!_deviceInput) return;

        if(pedalType == PedalType.Brake)
        {
            // transform.localEulerAngles = new Vector3((_deviceInput ? _wheelIntegration.Pedal_1_Input : _playerInputs.LeftController.Trigger) * _movingAngleRange, 0, 0);
            if (!_isBrakeSound && _playerInputs.LeftController.Trigger > 0.5)
            {
                StartCoroutine(BrakePedalSoundCoroutine());
            }
        }
        else
        {
            // transform.localEulerAngles = new Vector3((_deviceInput ? _wheelIntegration.Pedal_2_Input : _playerInputs.RightController.Trigger) * _movingAngleRange, 0, 0);
            if (!_isAccelerateSound && _playerInputs.RightController.Trigger > 0.5)
            {
                StartCoroutine(AcceleratePedalSoundCoroutine());
            }
        }
    }

    private void PlayerSitHandler(bool value)
    {

    }

    private IEnumerator BrakePedalSoundCoroutine()
    {
        _isBrakeSound = true;
        // if (_stateMachine.IsPlayerSit) _soundSystem.OnSingleSoundPlay?.Invoke(EnumSoundType.PedalRelease);
        // _stateMachine.BrakePedal = true;
        yield return new WaitUntil(() => _playerInputs.LeftController.Trigger <= 0.1f);
        // _stateMachine.BrakePedal = false;
        _isBrakeSound = false;
    }

    private IEnumerator AcceleratePedalSoundCoroutine()
    {
        _isAccelerateSound = true;
        // if (_stateMachine.IsPlayerSit) _soundSystem.OnSingleSoundPlay?.Invoke(EnumSoundType.PedalPress);
        // _stateMachine.AccelerationPedal = true;
        yield return new WaitUntil(() => _playerInputs.RightController.Trigger <= 0.1f);
        // _stateMachine.AccelerationPedal = false;
        _isAccelerateSound = false;
    }
}
