using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostActivator : MonoBehaviour
{
    [SerializeField] private GameObject _ghostGO;
    [SerializeField] private Transform _ghostPointTransform;

    private void OnEnable()
    {
        _ghostGO.SetActive(true);
        _ghostGO.transform.position = _ghostPointTransform.position;
        _ghostGO.transform.rotation = _ghostPointTransform.rotation;
    }

    private void OnDisable()
    {
        _ghostGO.SetActive(false);
    }
}
