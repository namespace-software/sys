using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using _Alex.Utils;
using UnityEngine;

public class ActiveLever : ControllerMoveHandler
{
    [InfoBox("����� ����� ������ �� ������� ���������� ���� ������ ���� ������� ��������� ���� ������ Ve�tor3.Zero. ��� ������������� ������������ ������� ��� ����� ����� ������� �������� � ��������� ��� � ����������� ���������")]

    [Header("��������� ����� (��������� ��� ������)")]
    [Tooltip("����������� ����� ������. [0 - 0.5] - ������������� 0 - 50% �� ������ ��������� ����������� �� ����. ���������� ��� ��������� ����, ����� ������������ ������ ����� ��������� ����. ���� 0.2 ����� �������� ��� ��������" +
        " ����� ��� �� 20% ������ ��������� �������� �������� �����.")]
    [Range(0f, 0.5f)]
    [SerializeField, PropertyOrder(-1)] private float _inputBacklashFactor;

    [Header ("Debug info")]
    [SerializeField] private float _value;
    [SerializeField] private float _lowAngleLimit;
    [SerializeField] private float _highAngleLimit;

    [SerializeField] private float _lowAngleLimitBacklash;
    [SerializeField] private float _highAngleLimitBacklash;

    private ConfigurableJoint _configurableJoint;
    private float _normalizedValue;
    private bool _isActive;

    public float InputValue { get => _value; set => _value = value; }

    protected override void Awake()
    {
        Init();
    }

    private void Update()
    {
        if (!_isActive) return;

        CheckInputValue();
    }

    private void Init()
    {
        _availableDirection = EnumAvailableDirection.Both;
        // _signalsInterpreter = FindFirstObjectByType<SignalsInterpreterBase>();
        _configurableJoint = GetComponent<ConfigurableJoint>();
        _lowAngleLimit = Mathf.Abs(_configurableJoint.lowAngularXLimit.limit);
        _highAngleLimit = 360 - _configurableJoint.highAngularXLimit.limit;

        _lowAngleLimitBacklash = _lowAngleLimit * _inputBacklashFactor;
        _highAngleLimitBacklash = 360 - _configurableJoint.highAngularXLimit.limit * _inputBacklashFactor;
    }

    private void CheckInputValue()
    {
        if (transform.localEulerAngles.x <= _lowAngleLimitBacklash || transform.localEulerAngles.x >= _highAngleLimitBacklash) { _value = 0; return; }

        if (transform.localEulerAngles.x >= _highAngleLimit)
        {
            _normalizedValue = Mathf.InverseLerp(_highAngleLimit, _highAngleLimitBacklash, transform.localEulerAngles.x);
            _value = 1f - _normalizedValue;
        }
        else if (transform.localEulerAngles.x <= _lowAngleLimit)
        {
            _value = -(transform.localEulerAngles.x / _lowAngleLimit);
        }
    }

    public override void HandlingOn(Transform controllerTransform, Transform pivotTransform, Action onComplete = null)
    {
        _controllerTransform = controllerTransform;
        _pivotTransform = pivotTransform;
        // _signalsInterpreter.OnSignalResieved?.Invoke(controllerType, 1, this, false);
        _isActive = true;
    }

    public override void HandlingOff()
    {
        StopAllCoroutines();
        _controllerTransform = null;
        // _signalsInterpreter.OnSignalResieved?.Invoke(controllerType, 0, this, false);
        _value = 0;
        _isActive = false;
    }

    public override void ReturnBackWhen<T, U, W>(StateWrapper<T> stateWrapper, U awaitValue_1, W awaitValue_2)
    {
        throw new NotImplementedException();
    }
}
