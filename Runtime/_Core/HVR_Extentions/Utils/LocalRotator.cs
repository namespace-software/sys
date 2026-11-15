using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalRotator : MonoBehaviour
{
    public void GetLocalAnimation(float rotateAngle, int steps, Vector3 axis, Action animationEndAction, bool destroyAfterUsing = true)
    {
        Debug.Log("Start animation axis: " + axis);
        StartCoroutine(LocalAnimationCoroutine(rotateAngle, steps, axis, animationEndAction, destroyAfterUsing));
    }

    private IEnumerator LocalAnimationCoroutine(float rotateAngle, int steps, Vector3 axis, Action animationEndAction,bool destroyAfterUsing)
    {
        float step = rotateAngle / steps;

        while (rotateAngle >= 0.01f)
        {
            transform.localEulerAngles += new Vector3(axis.x * step, axis.y * step, axis.z * step);
            //Debug.Log(transform.localEulerAngles);
            rotateAngle -= step;
            yield return null;
        }

        animationEndAction?.Invoke();

        if (destroyAfterUsing) Destroy(this);
    }
}