using System.Collections;
using System.Collections.Generic;
using _Core._ScenarioSystem.Component;
using _Core._ScenarioSystem.Component.Condition;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Реализация кондишина триггера через коллайдер
/// </summary>
[System.Serializable]
public class TriggerCondition : ConditionBase
{
    [Tooltip("Ссылка на объект коллайдера в сцене (На нем так же, обязательно должен быть компонент ColliderEventHandler)")]
    [BoxGroup("Trigger"), PropertyOrder(100)]
    [SerializeField] private Collider _collider;

    [BoxGroup("Trigger"), PropertyOrder(101)]
    [SerializeField] protected ColliderEventType _eventType;

    [BoxGroup("Trigger"), PropertyOrder(102)]
    [ListDrawerSettings(Expanded = true)]
    [ValueDropdown(nameof(GetTags))]
    [SerializeField]
    protected List<string> _tags = new();

    protected TriggerRelay _triggerRelay;

    public override void DisableCondition()
    {
        base.DisableCondition();

        RemoveTriggerRelay();
    }

    // ValueDropdown/nameof must compile in player builds; UnityEditor APIs stay editor-only.
    private static IEnumerable<string> GetTags()
    {
#if UNITY_EDITOR
        return UnityEditorInternal.InternalEditorUtility.tags;
#else
        return System.Array.Empty<string>();
#endif
    }

    public override void SetupCondition()
    {
        if (_collider == null)
        {
            Debug.LogError("_collider field in empty in TriggerCondition", parentStep);
            return;
        }

        CreateTriggerRelay();

        // OnTriggerEnter не приходит, если объект уже внутри на старте.
        // Кадр нужен: State = Init выставляется после SetupCondition.
        if (_eventType == ColliderEventType.TriggerEnter)
            //CoroutineRunner.RunCoroutine(CheckInitialOverlap(), parentStep.gameObject);
            CoroutineRunner.RunCoroutine(CheckInitialOverlap(), _collider.gameObject);
    }

    public override void ResetConditionToDefault()
    {
        if (State == State.None)
            return;

        base.ResetConditionToDefault();
    }

    protected virtual void OnTriggerHandler(Collider collider, ColliderEventType eventType)
    {
        if (State != State.Init || _eventType != eventType || !_tags.Contains(collider.tag))
            return;

        Resolve(collider, eventType);
    }

    private IEnumerator CheckInitialOverlap()
    {
        yield return null;

        if (State != State.Init || _collider == null || _eventType != ColliderEventType.TriggerEnter)
            yield break;

        Collider[] overlaps = QueryZoneOverlaps();
        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider other = overlaps[i];
            if (other == null || other == _collider || other.gameObject == _collider.gameObject)
                continue;

            if (!_tags.Contains(other.tag))
                continue;

            Resolve(other, ColliderEventType.TriggerEnter);
            yield break;
        }
    }

    private Collider[] QueryZoneOverlaps()
    {
        switch (_collider)
        {
            case BoxCollider box:
                return Physics.OverlapBox(
                    box.transform.TransformPoint(box.center),
                    Abs(Vector3.Scale(box.size * 0.5f, box.transform.lossyScale)),
                    box.transform.rotation,
                    ~0,
                    QueryTriggerInteraction.Collide);

            case SphereCollider sphere:
                Vector3 sphereScale = sphere.transform.lossyScale;
                float maxScale = Mathf.Max(Mathf.Abs(sphereScale.x), Mathf.Abs(sphereScale.y), Mathf.Abs(sphereScale.z));
                return Physics.OverlapSphere(
                    sphere.transform.TransformPoint(sphere.center),
                    sphere.radius * maxScale,
                    ~0,
                    QueryTriggerInteraction.Collide);

            case CapsuleCollider capsule:
                GetCapsuleWorldPoints(capsule, out Vector3 point0, out Vector3 point1, out float radius);
                return Physics.OverlapCapsule(point0, point1, radius, ~0, QueryTriggerInteraction.Collide);

            default:
                Bounds bounds = _collider.bounds;
                return Physics.OverlapBox(
                    bounds.center,
                    bounds.extents,
                    Quaternion.identity,
                    ~0,
                    QueryTriggerInteraction.Collide);
        }
    }

    private static void GetCapsuleWorldPoints(CapsuleCollider capsule, out Vector3 point0, out Vector3 point1, out float radius)
    {
        Transform transform = capsule.transform;
        Vector3 center = transform.TransformPoint(capsule.center);
        Vector3 lossyScale = transform.lossyScale;

        Vector3 axis;
        float heightScale;
        float radiusScale;

        switch (capsule.direction)
        {
            case 0:
                axis = transform.right;
                heightScale = Mathf.Abs(lossyScale.x);
                radiusScale = Mathf.Max(Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
                break;
            case 1:
                axis = transform.up;
                heightScale = Mathf.Abs(lossyScale.y);
                radiusScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z));
                break;
            default:
                axis = transform.forward;
                heightScale = Mathf.Abs(lossyScale.z);
                radiusScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                break;
        }

        radius = capsule.radius * radiusScale;
        float height = Mathf.Max(capsule.height * heightScale, radius * 2f);
        float pointOffset = height * 0.5f - radius;
        point0 = center + axis * pointOffset;
        point1 = center - axis * pointOffset;
    }

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }

    protected virtual void Resolve(Collider collider, ColliderEventType eventType)
    {
        Debug.Log($"Trigger entry {collider.gameObject.name} with type {eventType}");
        State = State.Success;

        DisableCondition();

        OnConditionResolved?.Invoke();

        OnResolved?.Invoke(this, CanResolveAsSuccess());
    }

    private void CreateTriggerRelay()
    {
        _triggerRelay = _collider.gameObject.AddComponent<TriggerRelay>();

        _triggerRelay.OnTriggerEntered += OnTriggerHandler;
        _triggerRelay.OnTriggerExited += OnTriggerHandler;
        _triggerRelay.OnTriggerStayed += OnTriggerHandler;
        _triggerRelay.OnCollisionEntered += OnTriggerHandler;
    }

    private void RemoveTriggerRelay()
    {
        if (_triggerRelay)
        {
            _triggerRelay.OnTriggerEntered -= OnTriggerHandler;
            _triggerRelay.OnTriggerExited -= OnTriggerHandler;
            _triggerRelay.OnTriggerStayed -= OnTriggerHandler;
            _triggerRelay.OnCollisionEntered -= OnTriggerHandler;

            UnityEngine.Object.Destroy(_triggerRelay);
        }
    }
}