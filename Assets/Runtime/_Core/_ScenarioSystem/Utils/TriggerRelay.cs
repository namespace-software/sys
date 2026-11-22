using System;
using _Core._ScenarioSystem.Component.Condition;
using UnityEngine;


public class TriggerRelay : MonoBehaviour
{
    public event Action<Collider, ColliderEventType> OnTriggerEntered;
    public event Action<Collider, ColliderEventType> OnTriggerExited;
    public event Action<Collider, ColliderEventType> OnTriggerStayed;
    public event Action<Collider, ColliderEventType> OnCollisionEntered;

    private int _usageCounter;

    public void Retain()
    {
        _usageCounter++;
    }

    public void Release()
    {
        _usageCounter--;

        if (_usageCounter <= 0)
        {
            Destroy(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEntered?.Invoke(other, ColliderEventType.TriggerEnter);
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerExited?.Invoke(other, ColliderEventType.TriggerExit);
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerStayed?.Invoke(other, ColliderEventType.TriggerStay);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEntered?.Invoke(collision.collider, ColliderEventType.Collision);
    }
}