using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Alex.Sound
{
    [System.Serializable]
    public struct AudioVolumePair
    {
        public AudioClip clip;
        public float volume;
    }

    [CreateAssetMenu(menuName = "Settings/SoundSettings")]
    public class SoundSettings : ScriptableObject
    {
        public string soundName;
        public EnumSoundType soundType;
        public List<AudioVolumePair> sounds;
    }
}
