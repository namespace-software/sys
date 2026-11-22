using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using _Core._ScenarioSystem.Component.Scenario;

[System.Serializable]
public class StepCondition
{
    public int StepIndex;
    public Transform StepStartPoint;
    public bool SetParent;
    public Transform Parent;
    public UnityEvent StartEvents;
}

public struct DefaultPosition
{
    public Vector3 _defPosition;
    public Quaternion _defRotation;

    public Vector3 _defPositionLocal;
    public Quaternion _defRotationLocal;

    public bool _isLocal;

    public DefaultPosition(Vector3 defPosition, Quaternion defRotation, Vector3 defPositionLocal, Quaternion defRotationLocal, bool isLocal)
    {
        _defPosition = defPosition;
        _defRotation = defRotation;
        _defPositionLocal = defPositionLocal;
        _defRotationLocal = defRotationLocal;
        _isLocal = isLocal;
    }
}

public class TrackedObject : MonoBehaviour
{
    [SerializeField] private bool _trackPosition = true;

    [ShowIf(nameof(_trackPosition))]
    [SerializeField] private bool _inLocalPosition;
    [Space(15)]

    [Header("Step Conditions")]
    [Tooltip("Используется если есть изменение состояния или позиции на конкретном шаге")]
    [SerializeField] private List<StepCondition> _stepConditions;
    [Space(15)]

    [Header("Extra features")]
    [SerializeField] private bool _autoAddToTrackedList;
    [SerializeField] private bool _debugMode;
    [SerializeField] private bool _skipApplyCondition;

    private DefaultPosition _defaultPosition;
    private Vector3 _prevPosition;
    private Vector3 _prevLocalPosition;
    private Quaternion _prevRotation;
    private Quaternion _prevLocalRotation;

    private IScenarioResettable _scenarioResettable;
    private Respawner _respawner;
    private _Core._ScenarioSystem.Component.Scenario.Scenario _scenario;

    private void Awake()
    {
        _defaultPosition = new DefaultPosition(transform.position, transform.rotation, transform.localPosition, transform.localRotation, _inLocalPosition);
        _scenarioResettable = GetComponent<IScenarioResettable>();
        _respawner = GetComponent<Respawner>();

        if (_autoAddToTrackedList)
        {
            _scenario = FindFirstObjectByType<_Core._ScenarioSystem.Component.Scenario.Scenario>();
            _scenario.AddTrackedObject(this);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!_debugMode) return;
        if (Input.GetKeyDown(KeyCode.Space)) ResetToPreviousState();
    }
#endif

    //Костыль, для того чтобы при посадке внутрь тягача бот перестал трекать позицию и перестал откатывать состояние, на время пока он внутри ТС
    public void TrackEnabler(bool enable)
    {
        _trackPosition = enable;
    }

    public void SaveCurrentState()
    {
        if (_trackPosition) SavePosition();
        _scenarioResettable?.SaveCurrentState();
        _respawner?.SaveCurrentPoint();
    }

    public void SaveToDefault()
    {
        _prevPosition = _defaultPosition._defPosition;
        _prevRotation = _defaultPosition._defRotation;

        _prevLocalPosition = _defaultPosition._defPositionLocal;
        _prevLocalRotation = _defaultPosition._defRotationLocal;

        _inLocalPosition = _defaultPosition._isLocal;
    }

    public void ResetToPreviousState()
    {
        if (_trackPosition && !_respawner) ResetToPreviousPosition();
        if (_respawner) _respawner.RespawnPrevious();

        _scenarioResettable?.ResetToPreviousState();
    }

    public bool TryToApplyStepCondition(int stepIndex)
    {
        if (_skipApplyCondition) return false;

        foreach (var item in _stepConditions)
        {
            if (item.StepIndex == stepIndex)
            {
                ApplyStepCondition(item);
                return true;
            }
        }

        return false;
    }

    private void ApplyStepCondition(StepCondition condition)
    {
        if (condition.SetParent) transform.SetParent(condition.Parent);
        if (condition.StepStartPoint)
        {
            transform.position = condition.StepStartPoint.position;
            transform.rotation = condition.StepStartPoint.rotation;
        }
        condition.StartEvents?.Invoke();
    }

    private void ResetToPreviousPosition()
    {
        if (_inLocalPosition)
        {
            transform.localPosition = _prevLocalPosition;
            transform.localRotation = _prevLocalRotation;
        }
        else
        {
            transform.position = _prevPosition;
            transform.rotation = _prevRotation;
        }
    }

    private void SavePosition()
    {
        _prevPosition = transform.position;
        _prevRotation = transform.rotation;

        _prevLocalPosition = transform.localPosition;
        _prevLocalRotation = transform.localRotation;
    }
}
