using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Alex.Utils;

public class PlatformMoverFront : MonoBehaviour
{
    [SerializeField] private ActiveLever _activeLever;

    public Transform container_1;
    public Transform container_2;

    public Vector3 targetPosition;
    public float moveSpeed;

    private bool _isActive;
    private Vector3 _startPosition;

    private void Update()
    {
        if (!_isActive) return;
        MovePlatformVertical();
    }

    private void MovePlatformVertical()
    {
        float input = _activeLever.InputValue;
        Vector3 newPosition = transform.localPosition + Vector3.up * moveSpeed * input * Time.deltaTime;
        newPosition.y = Mathf.Clamp(newPosition.y, _startPosition.y, targetPosition.y);
        transform.localPosition = newPosition;
    }

    public void MovePlatform()
    {
        container_1.SetParent(transform);
        _startPosition = transform.localPosition;
        _isActive = true;
    }

    IEnumerator MoveToLocalPosition()
    {
        Vector3 startPosition = transform.localPosition;

        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            float distCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            yield return null;
        }

        transform.localPosition = targetPosition;
    }
}
