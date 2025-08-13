using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkAudioManager : NetworkBehaviour
{
    [SerializeField] List<SoundNamePair> sounds;
}
[System.Serializable]
public struct SoundNamePair
{
    public string name;
    public Sound sound;
}