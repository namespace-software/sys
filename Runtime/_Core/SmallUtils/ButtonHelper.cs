using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHelper : MonoBehaviour
{
    [SerializeField] private List<GameObject> _objects;
    [SerializeField] private List<GameObject> _markObjects;
    private bool _activated;

    public bool Activated { get => _activated; }

    public void Init()
    {
        foreach (var item in _objects)
        {
            item.SetActive(true);
        }

        _activated = true;
    }

    public void FullReset()
    {
        foreach (var item in _objects)
        {
            item.SetActive(false);
        }

        _activated = false;
    }

    public void Click()
    {
        if (!_activated) { /*Debug.LogError("ButtonHelper component is not active");*/ return; }

        foreach (var item in _markObjects)
        {
            item.SetActive(true);
        }

    }
}
