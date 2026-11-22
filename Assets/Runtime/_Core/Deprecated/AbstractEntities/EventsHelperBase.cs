using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Obsolete("Этот enum устарел. Не используется. Будет удалён в 1.1.0")]
public enum EventParameterType
{
    Boolean = 0,
    Integer = 1,
    Float = 2,
    OuterEvent = 3,
    AnimationComplete = 4
}

[Obsolete("Этот класс устарел. Не используется. Будет удалён в 1.1.0")]
public abstract class EventsHelperBase : MonoBehaviour
{
    public Action<bool> OnCompleteCondition;
    [SerializeField] protected EventParameterType parameterType;

    [ShowIf(nameof(IsBooleanParameterType))]
    [SerializeField] protected bool _correctValue;

    [ShowIf(nameof(IsFloatParameterType))]
    [SerializeField] protected float _correctFloatValue;

    [ShowIf(nameof(IsIntegerParameterType))]
    [SerializeField] protected int _correctIntValue;

    [ShowIf(nameof(IsAnimationCompleteParameterType))]
    [SerializeField] protected Animator _animator;

    private bool IsBooleanParameterType() => parameterType == EventParameterType.Boolean;
    private bool IsFloatParameterType() => parameterType == EventParameterType.Float;
    private bool IsIntegerParameterType() => parameterType == EventParameterType.Integer;
    private bool IsAnimationCompleteParameterType() => parameterType == EventParameterType.AnimationComplete;

    private void Awake()
    {
        LogicSwitcher();
    }

    private void LogicSwitcher()
    {
        switch (parameterType)
        {
            case EventParameterType.Boolean:
                break;
            case EventParameterType.Integer:
                break;
            case EventParameterType.Float:
                break;
            case EventParameterType.OuterEvent:
                break;
            default:
                break;
        }
    }

    public abstract void ConditionReached(bool isCorrect);
}
