using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalMover : MonoBehaviour
{
    [SerializeField] private bool _immediately;
    [SerializeField] private Vector3 _localVector;
    [SerializeField] private Vector3 _defPosition;

    public void RotateTo()
    {
        if (_immediately) transform.localEulerAngles = _localVector;
        else
        {

        }
    }

    public void MoveTo()
    {
        if (_immediately) transform.localPosition = _localVector;
        else
        {

        }
    }

    public void MoveToDefault()
    {
        if (_immediately) transform.localPosition = _defPosition;
        else
        {

        }
    }

    public void RotateToDefault()
    {
        if (_immediately) transform.localEulerAngles = _defPosition;
        else
        {

        }
    }
}
