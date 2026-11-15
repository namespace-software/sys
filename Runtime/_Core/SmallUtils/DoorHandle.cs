using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorHandle : MonoBehaviour
{
    [SerializeField] private bool _opened;
    [SerializeField] private bool _locked = true;
    [Space(10)]

    [SerializeField] private AudioClip _openClip;
    [SerializeField] private AudioClip _tryToOpenLockedClip;
    [SerializeField] private AudioClip _lock;
    [Space(10)]

    [SerializeField] private List<GameObject> _lockIndicators;
    [SerializeField] private List<GameObject> _unlockIndicators;

    public UnityEvent OnOpen;
    public UnityEvent OnClose;
    public UnityEvent OnLock;
    public UnityEvent OnUnlock;

    public bool Opened { get => _opened; }
    public bool Unlocked { get => _locked; }

    // private VehicleStateMachineBase _vehicleStateMachine;
    private SoundComponent _soundComponent;

    private void Awake()
    {
        // _vehicleStateMachine = FindFirstObjectByType<VehicleStateMachineBase>();
        _soundComponent = GetComponent<SoundComponent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Key") { /*Debug.LogError("Key");*/ TryToLockInteractive(); }
        if (other.tag == "Hand") { /*Debug.LogError("Hand");*/ ActivateOutlunes(true); }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Key" || other.tag == "Hand") ActivateOutlunes(false);
    }

    private void ActivateOutlunes(bool isActive)
    {
        if (!isActive)
        {
            foreach (var item in _unlockIndicators)
            {
                item.SetActive(false);
            }

            foreach (var item in _lockIndicators)
            {
                item.SetActive(false);
            }
        }
        else
        {
            foreach (var item in _unlockIndicators)
            {
                item.SetActive(!_locked);
            }

            foreach (var item in _lockIndicators)
            {
                item.SetActive(_locked);
            }
        }
    }

    public void TryToOpen()
    {
        // if (_vehicleStateMachine.IsPlayerSit) return;

        if (!_locked)
        {
            _soundComponent.PlayShortWithClip(_openClip);
            OnOpen?.Invoke();
        }
        else _soundComponent.PlayShortWithClip(_tryToOpenLockedClip);
    }

    public void TryToLockInteractive()
    {
        if (_locked)
        {
            _locked = false;
            _soundComponent.PlayShortWithClip(_lock);
            OnUnlock?.Invoke();
            ActivateOutlunes(true);
        }
        else
        {
            if (!_locked)
            {
                _locked = true;
                _soundComponent.PlayShortWithClip(_lock);
                OnLock?.Invoke();
                ActivateOutlunes(true);
            }
        }

        // _vehicleStateMachine.OnDoorLock?.Invoke(_locked);
    }
}
