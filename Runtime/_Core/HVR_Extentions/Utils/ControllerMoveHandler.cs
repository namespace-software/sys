using System;
using UnityEngine;
using UnityEngine.Events;
using _Alex.Utils;

/// <summary>
/// Интерфейс возврата для обработки возврата значений и восстановления состояния
/// Позволяет вернуть значение контроллера в предыдущее состояние
/// value_1 и value_2 могут быть разными типами. stateWrapper - обертка для хранения типа данных, чтобы избежать конфликтов на этапе выполнения
/// value_1 и value_2 это проверочные значения для stateWrapper. Т.е. при возврате сначала проверяется значение value_1. Проверка этих значений происходит последовательно и при совпадении проверяется
/// также и value_2. После чего происходит возврат контроллера в предыдущее состояние
/// </summary>
public interface IReturnable
{
    void ReturnBackWhen<T, U, W>(StateWrapper<T> stateWrapper, U value_1, W value_2);
}

public abstract class ControllerMoveHandler : MonoBehaviour, IReturnable
{
    [Space(20)]
    [Header("Основные настройки")]
    // [Tooltip("Тип контроллера для определения его поведения")]
    [SerializeField] protected EnumControllerType controllerType; 
    [Range(2, 4)]
    [Tooltip("Количество шагов должно быть 2-4")]
    [SerializeField] protected int _stepsCount;
    [Tooltip("Индекс первого шага")]
    [SerializeField] protected int _firstStepIndex;
    [Space(10)]
    [Header("Состояние шага")]
    [SerializeField] protected int _stepIndex = 0;
    [Space(10)]

    [Tooltip("Доступное на данный момент направление. Both - оба. Forward и Backward только в одну сторону. None - не доступно.")]
    [SerializeField] protected EnumAvailableDirection _availableDirection;
    [Tooltip("Чувствительность для определения достижения порогового значения в градусах. Для поворота рекомендуется значение (примерно 30). Для скролла меньше (примерно 0.05-0.1)")]
    [SerializeField] protected float _deltaLimit;
    [Tooltip("Количество кадров анимации (рекомендуется брать 5 чтобы сохранить производительность)")]
    [SerializeField] protected int _animationStepsCount;

    [Tooltip("Ось вокруг которой будет происходить вращение")]
    [SerializeField] protected Vector3 _axis;
    [Tooltip("Для определения. Нужно чтобы при вращении направление вектора не менялось относительно оси по которой поворачивается объект.")]
    [SerializeField] protected Vector3 _axisDependedForward;

    [Tooltip("Автоматический возврат. Работает : после достижения лимита происходит возврат")]
    [SerializeField] protected bool _isAutomaticallyReturn;
    [Tooltip("Запрет на множественные шаги. Нужно чтобы избежать бесконечного цикла после смены")]
    [SerializeField] protected bool _isProhibitedMultipleSteps;

    [Tooltip("Ссылка на трансформ контроллера. Заполняется автоматически или вручную")]
    [SerializeField] protected Transform _controllerTransform;
    [Tooltip("Ссылка на точку опоры. Заполняется автоматически или вручную")]
    [SerializeField] protected Transform _pivotTransform;
    [Tooltip("Угол шага для одного поворота. От этого угла будет происходить расчет на каждый шаг.")]
    [SerializeField] protected float _stepAngle = 25;

    [Space(10)]
    [Header("Events")]
    [Space(5)]
    public UnityEvent<int> OnValueChanged;

    protected EnumAvailableDirection _prevDirection;
    [SerializeField] protected SignalsInterpreterBase _signalsInterpreter;
    protected Switcher _switcher;
    protected SoundComponent _soundComponent;

    protected bool _isKey;

    public EnumControllerType ControllerType { get => controllerType; }
    public int CurrentStepIndex { get => _stepIndex; }

    protected virtual void Awake()
    {
        if (_stepsCount == 2) _switcher = new TwoWaySwitcher(_firstStepIndex);
        else if (_stepsCount == 3) _switcher = new ThreeWaySwitcher(_firstStepIndex);
        else _switcher = new FourWaySwitcher(_firstStepIndex);

        _availableDirection = _switcher.GetCurrentAvailableDirection();
        _signalsInterpreter = FindFirstObjectByType<SignalsInterpreterBase>();
        _soundComponent = GetComponent<SoundComponent>();

        _prevDirection = EnumAvailableDirection.None;

        // _isKey = controllerType == EnumControllerType.KeyLock;
        SetDependingAxis();
    }

    private void SetDependingAxis()
    {
        if (!_isKey) _axisDependedForward = Vector3.forward; //TODO ����� ����������� � ����� ���� ����� ��������� ������ ��� Z. ���� ��� � �������� ����������� �������� � ������������ ���������� ��������
    }

    public virtual void HandlingOn(Transform controllerTransform, Transform pivot, Action onComplete = null)
    {

    }

    public virtual void HandlingOff()
    {

    }

    public virtual Vector2 GetAnalogValues()
    {
        return Vector2.zero;
    }

    public abstract void ReturnBackWhen<T, U, W>(StateWrapper<T> stateWrapper, U awaitValue_1, W awaitValue_2);
}
