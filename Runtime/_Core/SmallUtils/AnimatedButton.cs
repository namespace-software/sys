using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatedButton : MonoBehaviour
{
    // [SerializeField] private EnumControllerType _controllerType;
    [SerializeField] private int _blendShapeIndex;
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    public UnityEvent OnClick;
    public UnityEvent OnRelease;

    private SoundComponent _soundComponent;
    // private SignalsInterpreterBase _signalsInterpreterBase;
    private bool _activated;

    private void Awake()
    {
        _soundComponent = GetComponent<SoundComponent>();
        // _signalsInterpreterBase = FindFirstObjectByType<SignalsInterpreterBase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_activated || other.tag != "IndexFingerRight") return;
        ButtonClick();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_activated || other.tag != "IndexFingerRight") return;

        ButtonRelease();
    }

    public void ButtonClick()
    {
        _activated = true;

        _skinnedMeshRenderer.SetBlendShapeWeight(_blendShapeIndex, 100);
        if (_soundComponent != null) _soundComponent.PlayShort();
        OnClick?.Invoke();

        // if (_controllerType != EnumControllerType.Unknown && _signalsInterpreterBase != null) _signalsInterpreterBase.OnSignalResieved?.Invoke(_controllerType, 1, null, false);
    }

    public void ButtonRelease()
    {
        _activated = false;
        _skinnedMeshRenderer.SetBlendShapeWeight(_blendShapeIndex, 0);
        OnRelease?.Invoke();

        // if (_controllerType != EnumControllerType.Unknown && _signalsInterpreterBase != null) _signalsInterpreterBase.OnSignalResieved?.Invoke(_controllerType, 0, null, false);
    }
}
