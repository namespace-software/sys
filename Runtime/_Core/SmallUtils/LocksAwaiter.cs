using System.Collections;
using System.Collections.Generic;
using _Alex.Utils;
using UnityEngine;

public class LocksAwaiter : MonoBehaviour, IBooleanStateProvider
{
    public StateWrapper<bool> _locksClosed;
    public bool State => _locksClosed.Value;

    private void Awake()
    {
        _locksClosed = new StateWrapper<bool>(true);
    }

    StateWrapper<bool> IBooleanStateProvider.GetStateWrapper()
    {
        return _locksClosed;
    }
}
