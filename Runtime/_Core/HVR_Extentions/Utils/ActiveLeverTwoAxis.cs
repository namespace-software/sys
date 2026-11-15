using _Alex.Utils;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Windows;

public enum AvailableAxis
{
    Both = 0,
    First = 1,
    Second = 2,
    Forbidden = 3
}

public enum LeverAxis
{
    X,
    Y,
    Z,
    negX,
    negY,
    negZ
}

/// <summary>
/// Важно чтобы объект на котором расположен этот скрипт имел нулевые локальные углы эйлера Veсtor3.Zero
/// При необходимости расположения объекта под углом нужно создать родителя и повернуть его в необходимое положение
/// </summary>

public class ActiveLeverTwoAxis : ControllerMoveHandler
{
    [InfoBox("Важно чтобы локальные углы объекта были равны Vector3.Zero. При необходимости использования объекта под углом нужно создать родителя и повернуть его в необходимое положение")]

    [Header("Настройка оси и люфта")]
    [Tooltip("Коэффициент люфта рычага. [0 - 0.5] - соответствует 0 - 50% от предела поворота ограничителя по оси. Используется для настройки зоны, где движение рычага не учитывается. Если 0.2 значит движение на 20% предела поворота не будет учитываться.")]
    [Range(0f, 0.5f)]
    [SerializeField, PropertyOrder(-1)] private float _inputBacklashFactor;
    [Space(10)]
    [Tooltip("Режим активации осей рычага. True - активны обе оси одновременно. " +
        "False - активна только одна ось в зависимости от направления движения. В стандартном режиме это используется для джойстиков с крестовиной. Но активна только одна ось " +
        "и на ней будет накапливаться значение от центра до упора. При смене оси значение на прошлой оси сбрасывается. В стандартном исполнении рекомендуется " +
        "использовать классический режим с переключением осей.")]
    [SerializeField, PropertyOrder(-1)] private bool _simultaneously_2_Axes;
    [Tooltip("Инвертировать направление по оси X")]
    [SerializeField, PropertyOrder(-1)] private bool _inverse_first_Axis;
    [Tooltip("Инвертировать направление по оси Z")]
    [SerializeField, PropertyOrder(-1)] private bool _inverse_second_Axis;

    [SerializeField, PropertyOrder(-2)]
    private LeverAxis _firstAxis = LeverAxis.X;

    [SerializeField, PropertyOrder(-2)]
    private LeverAxis _secondAxis = LeverAxis.Z;

    [Space(10)]
    [Header("Debug info")]
    [Tooltip("Текущая активная ось")]
    [SerializeField] private AvailableAxis _currentAvailableAxis = AvailableAxis.Both;

    [Tooltip("Значение поворота рычага по оси X [от -1 до 1]")]
    [SerializeField] private float _firstValue;

    [Tooltip("Значение поворота рычага по оси Z [от -1 до 1]")]
    [SerializeField] private float _secondValue;

    [SerializeField] private float _lowAngleLimitBacklash;

    [SerializeField, PropertyOrder(-1)]
    [Tooltip("После отпускания руки рычаг возвращается в центр, а по осям сразу отправляется 0.")]
    private bool _returnToCenterOnRelease = true;

    private ConfigurableJoint _configurableJoint;
    private Rigidbody _rigidbody;
    private bool _isActive;

    private Quaternion initialLocalRotation;
    private bool _firstAxisActive;
    private bool _secondAxisActive;
    private float _firstAxisDelta;
    private float _secondAxisDelta;

    public float InputValue_FirstAxis { get => _inverse_first_Axis ? -_firstValue : _firstValue; set => _firstValue = value; }
    public float InputValue_SecondAxis { get => _inverse_second_Axis ? -_secondValue : _secondValue; set => _secondValue = value; }


    protected override void Awake()
    {
        Init();
    }

    private void Update()
    {
        if (!_isActive) 
            return;

        CheckLocalRotation();
        CalculateFirstAxis();
        CalculateSecondAxis();

        SendInput();
    }

    private void SendInput()
    {
        _signalsInterpreter?.OnSignalResieved?.Invoke(controllerType, 0, Vector2.zero, false);
    }

    private void Init()
    {
        _availableDirection = EnumAvailableDirection.Both;
        // _signalsInterpreter = FindFirstObjectByType<SignalsInterpreterBase>();
        _configurableJoint = GetComponent<ConfigurableJoint>();
        _rigidbody = GetComponent<Rigidbody>();

        if (_configurableJoint == null)
        {
            enabled = false;
            return;
        }

        _lowAngleLimitBacklash = Mathf.Abs(_configurableJoint.lowAngularXLimit.limit) * _inputBacklashFactor;
        initialLocalRotation = transform.localRotation;
    }

    public override Vector2 GetAnalogValues()
    {
        return new Vector2(InputValue_FirstAxis, InputValue_SecondAxis);
    }


    private void CheckLocalRotation()
    {
        // Вычисляем дельту поворота относительно начального положения
        Quaternion deltaLocalRotation = Quaternion.Inverse(initialLocalRotation) * transform.localRotation;

        // Преобразуем в углы Эйлера
        Vector3 deltaEulerAngles = deltaLocalRotation.eulerAngles;

        // Нормализуем углы от -180 до 180
        deltaEulerAngles.x = NormalizeAngle(deltaEulerAngles.x); //насколько рычаг отклонился по X
        deltaEulerAngles.y = NormalizeAngle(deltaEulerAngles.y);
        deltaEulerAngles.z = NormalizeAngle(deltaEulerAngles.z);

        _firstAxisDelta = GetAxisValue(deltaEulerAngles, _firstAxis);
        _secondAxisDelta = GetAxisValue(deltaEulerAngles, _secondAxis);


        if (Mathf.Abs(_firstAxisDelta) <= _lowAngleLimitBacklash && Mathf.Abs(_secondAxisDelta) <= _lowAngleLimitBacklash && _currentAvailableAxis != AvailableAxis.Both)
        {
            _currentAvailableAxis = AvailableAxis.Both;
            _firstAxisActive = false;
            _secondAxisActive = false;
            ChangeLimits();
        }
        else if (Mathf.Abs(_firstAxisDelta) > _lowAngleLimitBacklash && !_secondAxisActive && _currentAvailableAxis != AvailableAxis.First)
        {
            _currentAvailableAxis = AvailableAxis.First;
            _firstAxisActive = true;
            _secondAxisActive = false;
            ChangeLimits();
        }
        else if (Mathf.Abs(_secondAxisDelta) > _lowAngleLimitBacklash && !_firstAxisActive && _currentAvailableAxis != AvailableAxis.Second)
        {
            _currentAvailableAxis = AvailableAxis.Second;
            _firstAxisActive = false;
            _secondAxisActive = true;
            ChangeLimits();
        }
    }

    private float GetAxisValue(Vector3 euler, LeverAxis axis)
    {
        return axis switch
        {
            LeverAxis.X => euler.x,
            LeverAxis.Y => euler.y,
            LeverAxis.Z => euler.z,
            LeverAxis.negX => -euler.x,
            LeverAxis.negY => -euler.y,
            LeverAxis.negZ => -euler.z,
            _ => 0
        };
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180) 
            angle -= 360;
        while (angle < -180) 
            angle += 360;
        return 
            angle;
    }

    private void CalculateFirstAxis()
    {
        //Если активна 1 ось
        if (!_firstAxisActive || !IsAvailableAxis(AvailableAxis.First)) 
        { 
            _firstValue = 0; 
            return; 
        }

        _firstValue = _firstAxisDelta / 30;
    }

    private void CalculateSecondAxis()
    {
        // Если активна 2 ось
        if (!_secondAxisActive || !IsAvailableAxis(AvailableAxis.Second)) 
        { 
            _secondValue = 0; 
            return; 
        }

        _secondValue = _secondAxisDelta / 30;
    }

    private bool IsAvailableAxis(AvailableAxis availableAxis)
    {
        if (_simultaneously_2_Axes) 
            return true;
        if (_currentAvailableAxis == AvailableAxis.Both || _currentAvailableAxis == availableAxis) 
            return true;
        return false;
    }


    private void ChangeLimits()
    {
        if (_simultaneously_2_Axes) 
            return;

        bool firstActive = _currentAvailableAxis == AvailableAxis.Both || _currentAvailableAxis == AvailableAxis.First;
        bool secondActive = _currentAvailableAxis == AvailableAxis.Both || _currentAvailableAxis == AvailableAxis.Second;

        SetAngularMotion( _firstAxis, firstActive ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked);
        SetAngularMotion( _secondAxis, secondActive ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked);
    }

    private void SetAngularMotion( LeverAxis axis, ConfigurableJointMotion motion)
    {
        switch (axis)
        {
            case LeverAxis.X:
                _configurableJoint.angularXMotion = motion;
                break;

            case LeverAxis.Y:
                _configurableJoint.angularYMotion = motion;
                break;

            case LeverAxis.Z:
                _configurableJoint.angularZMotion = motion;
                break;
        }
    }

    public override void HandlingOn(Transform controllerTransform, Transform pivotTransform, Action onComplete = null)
    {
        _controllerTransform = controllerTransform;
        _pivotTransform = pivotTransform;
        //_signalsInterpreter.OnSignalResieved?.Invoke(controllerType, 1, this, false);
        _isActive = true;
    }

    public override void HandlingOff()
    {
        StopAllCoroutines();
        _controllerTransform = null;
        _firstValue = 0;
        _secondValue = 0;
        _firstAxisActive = false;
        _secondAxisActive = false;
        _currentAvailableAxis = AvailableAxis.Both;
        ChangeLimits();

        if (_returnToCenterOnRelease)
            ReturnToCenter();

        SendInput();
        _isActive = false;
    }

    private void ReturnToCenter()
    {
        if (_configurableJoint != null)
            _configurableJoint.targetRotation = Quaternion.identity;

        transform.localRotation = initialLocalRotation;

        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
            return;

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.MovePosition(transform.position);
        _rigidbody.MoveRotation(transform.rotation);
    }

    public override void ReturnBackWhen<T, U, W>(StateWrapper<T> stateWrapper, U awaitValue_1, W awaitValue_2)
    {
        throw new NotImplementedException();
    }
}