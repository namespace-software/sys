using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoveHandlerPair
{
    public MoveHandler Handler;
    public int UnblockingValue;

    public MoveHandlerPair(MoveHandler handler, int unblockingValue)
    {
        Handler = handler;
        UnblockingValue = unblockingValue;
    }
}

public class MoveHandlerSynchronizator : MonoBehaviour
{
    public bool _available = true;

    [SerializeField] private MoveHandler[] _handlers;
    [SerializeField] private List<MoveHandlerPair> _moveHandlerPairs;

    private bool _isActive;

    private void Awake()
    {
        _handlers = GetComponents<MoveHandler>();
        _moveHandlerPairs = new List<MoveHandlerPair>();

        foreach (var item in _handlers)
        {
            _moveHandlerPairs.Add(new MoveHandlerPair(item, item.CurrentStepIndex));
        }
    }

    private void OnEnable()
    {
        foreach (var item in _moveHandlerPairs)
        {
            item.Handler.OnIndexChanged += ValueHandler;
        }
    }

    private void OnDisable()
    {
        foreach (var item in _moveHandlerPairs)
        {
            item.Handler.OnIndexChanged -= ValueHandler;
        }
    }

    private void ValueHandler(MoveHandler moveHandler, int value)
    {
        //StartCoroutine(AvailabilityCoroutine());

        var handlerPair = _moveHandlerPairs.FirstOrDefault(pair => pair.Handler == moveHandler);

        if (handlerPair != null)
        {
            if (handlerPair.UnblockingValue == value)
            {
                // Если UnblockingValue равен value, возобновляем взаимодействие у всех остальных
                foreach (var pair in _moveHandlerPairs.Where(pair => pair.Handler != moveHandler))
                {
                    pair.Handler.ResumeInteraction();
                }
            }
            else
            {
                // Если UnblockingValue не равен value, останавливаем взаимодействие у всех остальных
                foreach (var pair in _moveHandlerPairs.Where(pair => pair.Handler != moveHandler))
                {
                    pair.Handler.StopInteraction();
                }
            }
        }
    }

    public void Activator(bool enable)
    {
        _isActive = enable;
    }

    //private IEnumerator AvailabilityCoroutine()
    //{
    //    _available = false;
    //    yield return new WaitForSeconds(0.03f);
    //    _available = true;
    //}
}
