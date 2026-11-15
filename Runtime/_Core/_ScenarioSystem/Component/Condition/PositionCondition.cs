using System.Collections;
using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using UnityEngine;
using Sirenix.OdinInspector;

public class PositionCondition : ConditionBase
{
	[Header("Ссылки на связанные объекты сцены")]
	[Tooltip("Трансформ объекта который нужно перемещать (например каска)")]
	[BoxGroup("Trigger"), SerializeField, PropertyOrder(100)] private Transform itemTransform;

	[Tooltip("Трансформ объекта к которому нужно перемещать (например голова)")]
	[BoxGroup("Trigger"), SerializeField, PropertyOrder(101)] private Transform targetTransform;

	[Tooltip("Дистанция на которой кондишн считается законченным")]
	[BoxGroup("Trigger"), SerializeField, PropertyOrder(102)] private float _distance;

	public Transform ItemTransform { get => itemTransform; }
	public Transform TargetTransform { get => targetTransform; }
	
	public override void SetupCondition()
	{
        if (State != State.None)
            return;
		
		CoroutineRunner.RunCoroutine(PositionCheckCoroutine(), parentStep.gameObject);
	}

	public override void ResetConditionToDefault()
	{
		if (State == State.None)
			return;
		
		base.ResetConditionToDefault();
	}

	private IEnumerator PositionCheckCoroutine()
	{
		yield return new WaitUntil(() => Vector3.Distance(itemTransform.position, targetTransform.position) <= _distance);
		
		if (State != State.Init)
			yield break;
		
		Debug.Log("PositionReached!");
		State = State.Success;
		
        DisableCondition();
		

		OnConditionResolved?.Invoke();
		
		// тут может быть перезапуск в системе сценариев поэтому запускаем последним
		OnResolved?.Invoke(this, CanResolveAsSuccess());
	}
}