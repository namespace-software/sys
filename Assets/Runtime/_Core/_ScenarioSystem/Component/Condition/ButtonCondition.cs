using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// Реализация кондишина проверки нажатия на кнопку
/// </summary>
[System.Serializable]
public class ButtonCondition : ConditionBase
{
    [Tooltip("Ссылка на Button в сцене")]
    [BoxGroup("Trigger"), PropertyOrder(100)]
    [SerializeField] private Button _button;

    [BoxGroup("Trigger"), Tooltip("Включать кнопку при инициализации кондишина?")]
    [SerializeField, PropertyOrder(101)] private bool _isEnableOnStart = false;
    [BoxGroup("Trigger"), Tooltip("Выключать кнопку при завершении кондишина?")]
    [SerializeField, PropertyOrder(102)] private bool _isDisableOnEnd = false;

    [BoxGroup("Trigger"), Tooltip("Включать кнопку при инициализации кондишина?")]
    [SerializeField, PropertyOrder(103)] private bool _isEnableComponentOnStart = false;
    [BoxGroup("Trigger"), Tooltip("Выключать кнопку при завершении кондишина?")]
    [SerializeField, PropertyOrder(104)] private bool _isDisableComponentOnEnd = false;

    /// <summary>
    /// Сохранение значения видимости кнопки при инициализации, для ресета
    /// </summary>
    private bool _isStartVisibleButton;

    public override void SetupCondition()
    {
        if (State != State.None)
            return;

        _isStartVisibleButton = _button.gameObject.activeSelf;

        if (_isEnableOnStart)
            _button.gameObject.SetActive(true);

        if (_isEnableComponentOnStart)
            _button.enabled = true;

        _button.onClick.AddListener(OnClickHandler);
    }

    public override void DisableCondition()
    {
        base.DisableCondition();

        _button.onClick.RemoveListener(OnClickHandler);
    }

    public override void ResetConditionToDefault()
    {
        if (State == State.None)
            return;

        base.ResetConditionToDefault();

        _button.gameObject.SetActive(_isStartVisibleButton);
    }

    private void OnClickHandler()
    {
        if (State != State.Init)
            return;

        Debug.Log("ButtonCondition click here!", _button.transform);
        bool isSuccess = CanResolveAsSuccess();
        State = isSuccess ? State.Success : State.Failure;

        if (_isDisableOnEnd)
            _button.gameObject.SetActive(false);

        if (_isDisableComponentOnEnd)
            _button.enabled = false;

        DisableCondition();

        OnConditionResolved?.Invoke();

        // тут может быть перезапуск в системе сценариев поэтому запускаем последним
        OnResolved?.Invoke(this, CanResolveAsSuccess());
    }
}
