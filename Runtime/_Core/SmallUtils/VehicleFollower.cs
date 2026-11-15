using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleFollower : MonoBehaviour
{
    [SerializeField] private Transform parent;

    private void FixedUpdate()
    {
        transform.position = parent.transform.position;
        transform.forward = parent.forward;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
