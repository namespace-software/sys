using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using ScenarioSystem.Additional;
using System.Collections;
using UnityEngine;

public class DistanceInspectCondition : ConditionBase
{
    [Tooltip("Компонент HVR_InteractionHelper, который необходимо осмотреть")]
    [SerializeField] private HVR_InteractionHelper _hVR_InteractionHelper;

    [Tooltip("UI-контроллео индикатора осмотра")]
    [SerializeField] private DistanceInspectionIndicator _indicatorUI;

    [SerializeField] private HVR_ObjectsPointer _hvr_ObjectPointer;

    [Tooltip("Время осмотра")]
    [SerializeField] private int _inspectTime = 1;

    private Coroutine _inspectionCoroutine;
    private float _currentInspectTime;

    public override void SetupCondition()
    {
        _hVR_InteractionHelper.DeactivateOutlineManual = true;
        _hVR_InteractionHelper.OnPointerEnter.AddListener(OnRayEnter);
        _hVR_InteractionHelper.OnPointerExit.AddListener(OnRayExit);

        _currentInspectTime = _inspectTime;
        //_hvr_ObjectPointer.IsActive = true;
    }

    private void OnRayEnter()
    {
        if (_inspectionCoroutine != null)
        {
            _hVR_InteractionHelper.StopCoroutine(_inspectionCoroutine);
            _inspectionCoroutine = null;
        }

        _inspectionCoroutine = _hVR_InteractionHelper.StartCoroutine(InspectionRoutine());
    }

    private IEnumerator InspectionRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _inspectTime)
        {
            elapsedTime += Time.deltaTime;
            _currentInspectTime = _inspectTime - elapsedTime;

            float progress = elapsedTime / _inspectTime;
            float fillValue = Mathf.Clamp01(progress);

            _indicatorUI.ActiveIndicator(fillValue);

            yield return null;
        }
         
        OnInspectionComplete();
    }

    private void OnInspectionComplete()
    {
        _indicatorUI.ResetIndicator();

        State = State.Success;

        OnConditionResolved?.Invoke();
        OnResolved?.Invoke(this, CanResolveAsSuccess());

        //_hvr_ObjectPointer.IsActive = false;

        DisableCondition();
    }

    private void OnRayExit()
    {
        if (_inspectionCoroutine != null)
        {
            _hVR_InteractionHelper.StopCoroutine(_inspectionCoroutine);
            _inspectionCoroutine = null;
        }

        _indicatorUI.ResetIndicator();

        _currentInspectTime = _inspectTime;
    }

    public override void DisableCondition()
    {
        if (_inspectionCoroutine != null)
        {
            _hVR_InteractionHelper.StopCoroutine(_inspectionCoroutine);
            _inspectionCoroutine = null;
        }

        _hVR_InteractionHelper.OnPointerEnter.RemoveListener(OnRayEnter);
        _hVR_InteractionHelper.OnPointerExit.RemoveListener(OnRayExit);
        _hVR_InteractionHelper.DeactivateOutlineManual = false;

        base.DisableCondition();
    }
}
