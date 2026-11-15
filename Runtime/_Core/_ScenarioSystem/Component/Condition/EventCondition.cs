using _Alex.Utils;
using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using Sirenix.OdinInspector;
using UnityEngine;

public class EventCondition : ConditionBase
{
	// [Header("Ссылки на связанные объекты сцены")]
	[Tooltip(
		"Ссылка на объект сцены реализующий интерфейс IBooleanStateProvider. При достижении _eventEnded.OnValueChanged будет вызвано окончание данного состояния")]
	[BoxGroup("Trigger"), PropertyOrder(100)]
	[SerializeField]
	public MonoBehaviour _stateProvider;

	private IBooleanStateProvider _booleanStateProvider;
	public StateWrapper<bool> _eventEnded;

	public override void DisableCondition()
	{
        Debug.Log($"DisableCondition called\n{System.Environment.StackTrace}");

        base.DisableCondition();
		 
		if (_eventEnded == null) return;
		_eventEnded.OnValueChanged -= OnEventHandler; 
        Debug.Log("UNSUB");
    }

	public override void SetupCondition()
	{
		if (State != State.None)
			return;

		_booleanStateProvider = _stateProvider as IBooleanStateProvider;
		if (_booleanStateProvider == null)
		{
			Debug.LogError("The provided MonoBehaviour does not implement IBooleanStateProvider.");
			return;
		}

		_eventEnded = _booleanStateProvider.GetStateWrapper();
		if (_eventEnded == null)
		{
			Debug.LogError("StateWrapper<bool> is not initialized in the provided IBooleanStateProvider.");
			return;
		}

		Debug.Log(_eventEnded);

		_eventEnded.OnValueChanged += OnEventHandler;

        Debug.Log("SUBS");
    }

	public override void ResetConditionToDefault()
	{
		if (State == State.None)
			return;

		base.ResetConditionToDefault();
	}

	private void OnEventHandler(bool isSuccess)
	{
        if (State != State.Init)
			return;

		State = isSuccess ? State.Success : State.Failure;

        DisableCondition();

        OnConditionResolved?.Invoke();
		
        // тут может быть перезапуск в системе сценариев поэтому запускаем последним
		OnResolved?.Invoke(this, isSuccess);
	}
}
