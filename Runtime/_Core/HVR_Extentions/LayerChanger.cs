using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerChanger : MonoBehaviour
{
    [SerializeField] private GameObject _layerSampleGO;
    [SerializeField] private float _delay;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_delay);
        gameObject.layer = _layerSampleGO.layer;
    }
}
