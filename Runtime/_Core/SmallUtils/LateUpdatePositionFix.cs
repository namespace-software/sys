using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateUpdatePositionFix : MonoBehaviour
{
    [SerializeField] private Transform anchor;

    private void LateUpdate()
    {
        transform.position = anchor.position;
    }
}
