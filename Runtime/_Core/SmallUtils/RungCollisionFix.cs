using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RungCollisionFix : MonoBehaviour
{
    [SerializeField] private bool _isCollisioner;
    [SerializeField] private RungCollisionFix _antiCollision;
    [SerializeField] private float _delay;
    public bool _isReady = true;
    private bool _isExit = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Rung") return;

        if (_isCollisioner) StartCoroutine(ReadyDelayAwaiterCoroutine(other));
        else StartCoroutine(ReadyDelayCoroutine(other));
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isCollisioner || other.tag != "Rung") return;
        _isExit = true;
    }

    private IEnumerator ReadyDelayCoroutine(Collider other)
    {
        other.isTrigger = true;
        _isExit = false;
        _isReady = false;
        yield return new WaitUntil(() => _isExit);
        yield return new WaitForSeconds(_delay);
        _isReady = true;
    }

    private IEnumerator ReadyDelayAwaiterCoroutine(Collider other)
    {
        yield return new WaitUntil(() => _antiCollision._isReady);
        other.isTrigger = false;
    }
}
