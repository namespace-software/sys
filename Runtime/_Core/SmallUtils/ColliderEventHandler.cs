using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using _Alex.Sound;
using _Core._ScenarioSystem.Component.Condition;

//public enum ColliderEventType
//{
//    TriggerEnter = 0,
//    TriggerExit = 1,
//    Collision = 2,
//    TriggerStay = 3
//}

public class ColliderEventHandler : MonoBehaviour
{
    public event Action OnColliderEnter;
    public event Action OnColliderExit;
    public event Action<Collider> ColliderEventsCompleted;

    [Tooltip("Список тегов с которыми возможно взаимодействие")]
    [SerializeField] private List<string> _itemTags;

    [InfoBox("Здесь задаются условия при которых будет отправлена команда о завершении состояния. При необходимости просто проверить выход объекта из триггера или вход в него или коллизию" +
        " то достаточно в список ивентов добавить один тип (TriggerEnter или TriggerExit или Collision). Если же нужна последовательность, то необходимо в список добавить несколько типов ивентов. Пример: мы хотим чтобы сперва объект " +
        "вышел из триггера а затем вернулся обратно:" +
        " В этом случае мы добавляем первым тип события TriggerExit а вторым TriggerEnter. Событие завершения наступит после того как объект сначала выйдет из триггера а затем в него вернётся. Это может быть полезно при реализации" +
        " масляного щупа например")]
    [Tooltip("Cписок ивентов которые необходимы для отправки сигнала о наступлении события")]
    [SerializeField] private List<ColliderEventType> _eventsOrder;
    [Space(10)]

    [SerializeField] private AudioClip _colliderEventSound;

    private List<ColliderEventType> _orderInAction;
    [HideInInspector] public bool IsActive;
    [SerializeField] private bool _isSimpleTrigger;
    [Space(15)]

    public UnityEvent OnStart;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    private SoundSystem _soundSystem;

    private void Awake()
    {
        _orderInAction = new List<ColliderEventType>();
        _orderInAction.AddRange(_eventsOrder);
        _soundSystem = FindFirstObjectByType<SoundSystem>();
    }

    private void Start()
    {
        OnStart?.Invoke();
        if (_itemTags.Count < 1) Debug.LogError("Список тегов пуст. Необходимо добавить хотя бы одно имя тега с которым будет взаимодействовать компонент ColliderEventHandler", transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_itemTags.Contains(other.tag)) OnEnter?.Invoke();

        if (_isSimpleTrigger && _itemTags.Contains(other.tag) && IsActive)
        {
            if (!_eventsOrder.Contains(ColliderEventType.TriggerEnter)) return;
            //Debug.LogError(other.name);
            OnColliderEnter?.Invoke();
            if (_colliderEventSound != null) _soundSystem.OnSingleSoundPlayWithClip(_colliderEventSound, null);
            return;
        }

        if (!_itemTags.Contains(other.tag) || !IsActive) return;

        if (IsConsistInZeroIndex(ColliderEventType.TriggerEnter))
        {
            _orderInAction.RemoveAt(0);
        }

        CheckEndingState(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_itemTags.Contains(other.tag)) OnExit?.Invoke();

        if (_isSimpleTrigger && _itemTags.Contains(other.tag))
        {
            if (!_eventsOrder.Contains(ColliderEventType.TriggerExit)) return;

            ColliderEventsCompleted?.Invoke(other);
            OnColliderExit?.Invoke();
            return;
        }

        if (!_itemTags.Contains(other.tag) || !IsActive) return;

        if (IsConsistInZeroIndex(ColliderEventType.TriggerExit))
        {
            _orderInAction.RemoveAt(0);
        }

        CheckEndingState(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_itemTags.Contains(collision.transform.tag) || !IsActive) return;

        if (IsConsistInZeroIndex(ColliderEventType.Collision))
        {
            _orderInAction.RemoveAt(0);
        }

        CheckEndingState();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_itemTags.Contains(other.tag) || !IsActive) return;

        if (_isSimpleTrigger)
        {
            if (!_eventsOrder.Contains(ColliderEventType.TriggerStay)) return;

            ColliderEventsCompleted?.Invoke(other);
            return;
        }

        ColliderEventsCompleted?.Invoke(other);
    }

    private void CheckEndingState(Collider other = null)
    {
        if (_orderInAction.Count != 0) return;

        ColliderEventsCompleted?.Invoke(other);
        if (_colliderEventSound != null) _soundSystem.OnSingleSoundPlayWithClip(_colliderEventSound, null);
    }

    /// <summary>
    /// Определяем соответствует ли тип случившегося ивента и соответствует ли индекс, порядку в листе _eventsOrder[0].
    /// Событие выполнится только если текущий тип ивента является первым в списке и соответствует типу
    /// </summary>
    /// <returns></returns>
    private bool IsConsistInZeroIndex(ColliderEventType eventType)
    {
        if (!_orderInAction.Contains(eventType)) return false;
        if (_orderInAction.IndexOf(eventType) != 0) return false;
        return true;
    }
}
