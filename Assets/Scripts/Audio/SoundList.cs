using System.Collections.Generic;
using UnityEngine;

namespace NetworkAudioManagement
{
    [CreateAssetMenu(fileName = "Sound List", menuName = "Scriptables/Sound List")]
    public class SoundList : ScriptableObject
    {
        [SerializeField] List<SoundNamePair> m_soundList;
        internal List<SoundNamePair> soundList => m_soundList;
    }

    [System.Serializable]
    internal struct SoundNamePair
    {
        public string name;
        public Sound sound;
    }
}