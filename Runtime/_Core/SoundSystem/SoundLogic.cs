using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using _Alex.Utils;

namespace _Alex.Sound
{
    public class SoundLogic : MonoBehaviour
    {
        private SoundSystem _soundSystem;

        private List<AudioSource> _audioSources;
        private List<AudioSource> _audioSources3D;

        private List<AudioSource> _audioSourcesInAction;

        private void Awake()
        {
            _audioSources = new List<AudioSource>();
            _audioSources3D = new List<AudioSource>();
            _audioSourcesInAction = new List<AudioSource>();

            _soundSystem = FindObjectOfType<SoundSystem>();
            CreateAudiosources(_soundSystem.preInstalledAudioSourcesCount);
        }

        private void OnEnable()
        {
            _soundSystem.OnSingleSoundPlay += SoundPlay;
            _soundSystem.OnSoundPlayWithSource += SoundPlay;
            _soundSystem.OnRepetitivePlayWithActionFloat += SoundPlayWhileFloat;
            _soundSystem.OnRepetitivePlayWithActionBool += SoundPlayWhileBool;
            _soundSystem.On_3D_SoundWithPositionAndMaxDistance += Play_3D_Sound;
            _soundSystem.OnSingleSoundPlayWithClip += PlayWithClip;
            _soundSystem.OnPlayInterrupt += InterruptPlaying;
            _soundSystem.OnClipInterrupt += InterruptAudioClip;
            _soundSystem.FullStop += StopAllPlaying;
            _soundSystem.OnSingleStop += SoundTypeStop;
        }

        private void OnDisable()
        {
            _soundSystem.OnSingleSoundPlay -= SoundPlay;
            _soundSystem.OnSoundPlayWithSource -= SoundPlay;
            _soundSystem.OnRepetitivePlayWithActionFloat -= SoundPlayWhileFloat;
            _soundSystem.OnRepetitivePlayWithActionBool -= SoundPlayWhileBool;
            _soundSystem.On_3D_SoundWithPositionAndMaxDistance -= Play_3D_Sound;
            _soundSystem.OnSingleSoundPlayWithClip -= PlayWithClip;
            _soundSystem.OnPlayInterrupt -= InterruptPlaying;
            _soundSystem.OnClipInterrupt -= InterruptAudioClip;
            _soundSystem.FullStop -= StopAllPlaying;
            _soundSystem.OnSingleStop -= SoundTypeStop;
        }

        private void CreateAudiosources(int count)
        {
            _audioSources = new List<AudioSource>();

            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("AudioSource");
                _audioSources.Add(go.AddComponent<AudioSource>());
            }

            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("AudioSource");
                _audioSources3D.Add(go.AddComponent<AudioSource>());
            }
        }

        private AudioSource GetFreeAudiosource(AudioSource _audioSource = null)
        {
            AudioSource audioSource = null;

            foreach (var item in _audioSources)
            {
                if (!item.isPlaying && item != _audioSource) audioSource = item;
            }

            if (audioSource == null)
            {
                CreateAudiosources(1);
                audioSource = _audioSources[_audioSources.Count - 1];
            }

            return audioSource;
        }

        private AudioVolumePair GetRandom(List<AudioVolumePair> _audioClipsPairs)
        {
            int random = UnityEngine.Random.Range(0, _audioClipsPairs.Count);
            return _audioClipsPairs[random];
        }

        private void StopAllPlaying()
        {
            for (int i = _audioSources.Count - 1; i >= 0; i--)
            {
                AudioSource audioSource = _audioSources[i];
                if (audioSource == null || audioSource.loop)
                    continue;

                audioSource.Stop();
            }

            _audioSourcesInAction.RemoveAll(source => source == null || !source.isPlaying);
        }

        private void SoundPlayWhileFloat(EnumSoundType soundType, StateWrapper<float> state)
        {
            AudioSource audioSource = GetFreeAudiosource();
            audioSource.loop = true;
            SoundSettings settings = GetSoundSettings(soundType);
            if (settings == null) return;
            AudioVolumePair audioClipPair = GetRandom(settings.sounds);

            audioSource.volume = audioClipPair.volume;
            audioSource.clip = audioClipPair.clip;
            audioSource.Play();

            StartCoroutine(SoundPlayWhileFloatCoroutine(audioSource, state));
        }

        private IEnumerator SoundPlayWhileFloatCoroutine(AudioSource audioSource, StateWrapper<float> state)
        {
            float startValue = state.Value;
            yield return new WaitUntil(() => startValue != state.Value);
            ResetAudiosourceToDefault(audioSource);
        }


        private void SoundPlayWhileBool(EnumSoundType soundType, StateWrapper<bool> state)
        {
            AudioSource audioSource = GetFreeAudiosource();
            audioSource.loop = true;
            SoundSettings settings = GetSoundSettings(soundType);
            if (settings == null) return;
            AudioVolumePair audioClipPair = GetRandom(settings.sounds);

            audioSource.volume = audioClipPair.volume;
            audioSource.clip = audioClipPair.clip;
            audioSource.Play();

            StartCoroutine(SoundPlayWhileBoolCoroutine(audioSource, state));
        }

        private IEnumerator SoundPlayWhileBoolCoroutine(AudioSource audioSource, StateWrapper<bool> state)
        {
            bool startValue = state.Value;
            yield return new WaitUntil(() => startValue != state.Value);
            ResetAudiosourceToDefault(audioSource);
        }

        private void SoundPlay(EnumSoundType soundType)
        {
            AudioSource audioSource = GetFreeAudiosource();
            SoundSettings settings = GetSoundSettings(soundType);
            if (settings == null) return;
            AudioVolumePair audioClipPair = GetRandom(settings.sounds);
            audioSource.PlayOneShot(audioClipPair.clip, audioClipPair.volume);
            audioSource.loop = false;
        }

        private void SoundTypeStop(EnumSoundType soundType)
        {
            SoundSettings settings = GetSoundSettings(soundType);

            foreach (var item in _audioSources)
            {
                var matchingPair = settings.sounds.FirstOrDefault(s => s.clip == item.clip);
                if (item.isPlaying && matchingPair.clip != null)
                {
                    // ��������, ���� ������� ����������
                    item.Stop();
                }
            }
        }

        private void SoundPlay(EnumSoundType soundType, EnumPlayType playType, AudioSource audioSource)
        {
            SoundSettings settings = GetSoundSettings(soundType);
            AudioVolumePair audioClipPair = GetRandom(settings.sounds);

            audioSource.PlayOneShot(audioClipPair.clip, audioClipPair.volume);
        }

        private SoundSettings GetSoundSettings(EnumSoundType soundType)
        {
            SoundSettings settings = null;

            foreach (var item in _soundSystem.SoundsSettings)
            {
                if (item.soundType == soundType) return item;
            }

            if (settings == null) Debug.LogError("SoundSettings is null. When soundType= " + soundType);

            return settings;
        }

        private void Play_3D_Sound(EnumSoundType soundType, Vector3 position, float maxDistance)
        {
            AudioSource audioSource = null;

            for (int i = 0; i < _audioSources3D.Count; i++)
            {
                if (!_audioSources3D[i].isPlaying) { audioSource = _audioSources3D[i]; break; }
            }

            audioSource.maxDistance = maxDistance;
            audioSource.transform.position = position;

            SoundSettings settings = GetSoundSettings(soundType);
            AudioVolumePair audioClipPair = GetRandom(settings.sounds);

            audioSource.PlayOneShot(audioClipPair.clip, audioClipPair.volume);
        }

        private void ResetAudiosourceToDefault(AudioSource audioSource)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
            audioSource.volume = 1;
        }

        private void PlayWithClip(AudioClip clip, Action action)
        {
            AudioSource audioSource = GetFreeAudiosource();
            if (clip != null)
            {
                audioSource.PlayOneShot(clip, 1);
                audioSource.clip = clip;
            }
            audioSource.loop = false;

            _audioSourcesInAction.Add(audioSource);

            StartCoroutine(ActionAwaitCoroutine(audioSource, action));
        }

        private IEnumerator ActionAwaitCoroutine(AudioSource audioSource, Action action)
        {
            yield return new WaitUntil(() => !audioSource.isPlaying);
            _audioSourcesInAction.Remove(audioSource);
            action?.Invoke();
        }

        private void InterruptPlaying()
        {
            StopAllCoroutines();

            foreach (var item in _audioSourcesInAction)
            {
                item.Stop();
            }

            _audioSourcesInAction.Clear();
        }

        private void InterruptAudioClip(AudioClip clip)
        {
            foreach (var item in _audioSourcesInAction)
            {
                if (item.clip == clip)
                {
                    item.Stop();
                    if (clip) Debug.Log("������������ ���������� [" + clip.name + "] ����������� ��������.");
                }
            }
        }
    }
}
