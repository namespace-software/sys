using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using _Alex.Utils;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Alex.Sound
{
    public class SoundSystem : MonoBehaviour
    {
        [Tooltip("Количество заранее отспавненных аудиосорсов")]
        public int preInstalledAudioSourcesCount = 5;
        //public float masterVolume = 1;

        public Action<EnumSoundType> OnSingleSoundPlay;
        public Action<EnumSoundType, StateWrapper<float>> OnRepetitivePlayWithActionFloat;
        public Action<EnumSoundType, StateWrapper<bool>> OnRepetitivePlayWithActionBool;
        public Action<EnumSoundType, EnumPlayType, AudioSource> OnSoundPlayWithSource;
        public Action<EnumSoundType, Vector3, float> On_3D_SoundWithPositionAndMaxDistance;

        public Action<EnumSoundType> OnSingleStop;
        public Action<AudioClip, Action> OnSingleSoundPlayWithClip;
        public Action<AudioClip> OnClipInterrupt;
        public Action OnPlayInterrupt;
        public Action FullStop;

        [SerializeField, ReadOnly] private List<SoundSettings> soundsSettings;
        [SerializeField, ReadOnly] private AudioSource[] audioSources;

        public List<SoundSettings> SoundsSettings { get => soundsSettings; }


#if UNITY_EDITOR
        [Button("Collect Sound Settings")]
        public void CollectSoundSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:SoundSettings");
            soundsSettings = new List<SoundSettings>();
            //soundsSettings.Clear();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SoundSettings soundSettings = AssetDatabase.LoadAssetAtPath<SoundSettings>(path);

                if (soundSettings != null)
                    soundsSettings.Add(soundSettings);
            }

            EditorUtility.SetDirty(this);

            if (soundsSettings.Count == 0)
            {
                Debug.LogError($"{nameof(SoundSystem)} на объекте '{name}': в проекте не найдено ни одного SoundSettings.", this);
                return;
            }

            Debug.Log($"{nameof(SoundSystem)} '{name}': найдено {soundsSettings.Count} SoundSettings.", this);
        }
#endif
    }
}