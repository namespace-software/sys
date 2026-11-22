using _Alex.Utils;
using _Alex.Sound;
using UnityEngine;

public class SoundComponent : MonoBehaviour
{
    [SerializeField] private EnumSoundType soundType;

    private SoundSystem _soundSystem;

    public StateWrapper<bool> _loopInAction;
    public bool State => _loopInAction.Value;


    private void Awake()
    {
        _soundSystem = FindFirstObjectByType<SoundSystem>();
        _loopInAction = new StateWrapper<bool>(false);
    }

    public void LoopStart()
    {
        _loopInAction.SetValue(true);
        if (_soundSystem == null) _soundSystem = FindFirstObjectByType<SoundSystem>();
        _soundSystem.OnRepetitivePlayWithActionBool?.Invoke(soundType, _loopInAction);
    }

    public void LoopStop()
    {
        _loopInAction.SetValue(false);
    }

    public void PlayShort()
    {
        if (soundType == EnumSoundType.None) { Debug.LogError("You forget to changing SoundType at SoundComponent", transform); return; }
        if (_soundSystem == null) _soundSystem = FindFirstObjectByType<SoundSystem>();
        _soundSystem.OnSingleSoundPlay?.Invoke(soundType);
    }

    public void PlayShortWithType(EnumSoundType type)
    {
        if (type == EnumSoundType.None) { Debug.LogError("You forget to changing SoundType at SoundComponent", transform); return; }
        if (_soundSystem == null) _soundSystem = FindFirstObjectByType<SoundSystem>();
        _soundSystem.OnSingleSoundPlay?.Invoke(type);
    }

    public void StopPlayingWithType(EnumSoundType type)
    {
        if (_soundSystem == null) _soundSystem = FindFirstObjectByType<SoundSystem>();
        _soundSystem.OnSingleStop?.Invoke(type);
    }

    public void LoopStartWhile(EnumSoundType type, StateWrapper<bool> loopCondition)
    {
        if (_soundSystem == null) _soundSystem = FindFirstObjectByType<SoundSystem>();

        _soundSystem.OnRepetitivePlayWithActionBool?.Invoke(type, loopCondition);
    }

    public void PlayShortWithClip(AudioClip clip)
    {
        if (clip == null) { Debug.LogError("Audioclip is null"); return; }
        
        _soundSystem.OnSingleSoundPlayWithClip?.Invoke(clip, null);
    }

    public void InterruptClip(AudioClip clip)
    {
        _soundSystem.OnClipInterrupt.Invoke(clip);
    }
}
