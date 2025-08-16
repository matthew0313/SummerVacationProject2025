using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace NetworkAudioManagement
{
    public class NetworkAudioManager : NetworkBehaviour
    {
        public static NetworkAudioManager Instance { get; private set; }
        public NetworkAudioManager() => Instance = this;

        [SerializeField] SoundList m_sounds, m_musics;
        [SerializeField] AudioSource musicSource;
        internal List<SoundNamePair> sounds => m_sounds.soundList;
        internal List<SoundNamePair> musics => m_musics.soundList;

        public readonly NetworkVariable<int> currentMusicIndex = new(0);
        Sound currentMusic = null;

        readonly Dictionary<ulong, PlayingSound> playingSounds = new();
        readonly List<ulong> removeQueue = new();
        Pooler<AudioSource> audioSourcePool;

        private void OnDrawGizmos()
        {
            foreach(var i in playingSounds.Values)
            {
                if (i.hasDistance)
                {
                    Vector3 center = i.locationSource != null ? i.locationSource.transform.position : i.location;
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(center, i.maxDist);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(center, i.minDist);
                }
            }
        }
        private void Awake()
        {
            audioSourcePool = new(() =>
            {
                var tmp = gameObject.AddComponent<AudioSource>();
                tmp.playOnAwake = false;
                tmp.loop = false;
                return tmp;
            })
            {
                onTakeout = source => source.enabled = true,
                onRelease = source => source.enabled = false
            };
        }
        private void Start()
        {
            currentMusicIndex.OnValueChanged += SetMusic;
            SetMusic(-1, currentMusicIndex.Value);
        }
        void SetMusic(int prev, int index)
        {
            if (index == -1)
            {
                currentMusic = null;
                musicSource.Stop();
                return;
            }
            currentMusic = musics[index].sound;
            musicSource.clip = currentMusic.clip;
            musicSource.volume = currentMusic.volume * Settings.BGMVolume;
            musicSource.pitch = currentMusic.pitch;
            musicSource.Play();
            musicSource.time = currentMusic.startTime;
        }
        public void ChangeMusic(string musicName)
        {
            if (!HasAuthority) return;
            for (int i = 0; i < sounds.Count; i++)
            {
                if (musics[i].name == musicName)
                {
                    currentMusicIndex.Value = i; return;
                }
            }
            throw new Exception($"Music {musicName} was not found.");
        }
        private void OnEnable()
        {
            NetworkManager.OnConnectionEvent += OnConnection;
            Settings.onBGMVolumeChange += OnBGMVolumeChange;
        }
        private void OnDisable()
        {
            NetworkManager.OnConnectionEvent -= OnConnection;
            Settings.onBGMVolumeChange -= OnBGMVolumeChange;
        }
        void OnBGMVolumeChange() => musicSource.volume = (currentMusic != null ? currentMusic.volume : 0) * Settings.BGMVolume;
        private void OnConnection(NetworkManager manager, ConnectionEventData data)
        {
            if (data.EventType == ConnectionEvent.PeerConnected)
            {
                foreach (var i in playingSounds.Values)
                {
                    if (i.isOwner)
                    {
                        AddExistingSoundRpc(i.id, i.soundIndex, i.source.time, RpcTarget.Single(data.ClientId, RpcTargetUse.Temp));
                    }
                }
            }
            if (data.EventType == ConnectionEvent.PeerDisconnected)
            {
                foreach (var i in playingSounds.Values)
                {
                    if (i.ownerID == data.ClientId)
                    {
                        i.source.Stop();
                        audioSourcePool.ReleaseObject(i.source);
                        removeQueue.Add(i.id);
                    }
                }
            }
        }
        private void Update()
        {
            foreach (var i in removeQueue) playingSounds.Remove(i); removeQueue.Clear();
            foreach (var i in playingSounds.Values) i.Update();
            if (currentMusic != null && !musicSource.isPlaying)
            {
                musicSource.Play();
                musicSource.time = currentMusic.startTime;
            }
        }
        public PlayingSound PlaySound(string soundName, RpcParams rpcParams = new())
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                if (sounds[i].name == soundName)
                {
                    ulong id = MathUtilities.GenerateRandomID();
                    PlayingSound tmp = new(
                        this,
                        NetworkManager.LocalClientId,
                        id,
                        audioSourcePool.GetObject(),
                        i);
                    tmp.source.Play();
                    tmp.source.time = tmp.sound.startTime;
                    playingSounds.Add(id, tmp);
                    PlaySoundRpc(id, i, rpcParams);
                    return tmp;
                }
            }
            throw new Exception($"Sound {soundName} was not found.");
        }
        public void StopSound(PlayingSound sound) => StopSoundRpc(sound.id);

        [Rpc(SendTo.NotMe, AllowTargetOverride = true)]
        void PlaySoundRpc(ulong soundID, int index, RpcParams rpcParams)
        {
            PlayingSound tmp = new(
                this,
                rpcParams.Receive.SenderClientId,
                soundID,
                audioSourcePool.GetObject(),
                index);
            tmp.source.Play();
            tmp.source.time = tmp.sound.startTime;
            playingSounds.Add(soundID, tmp);
        }
        [Rpc(SendTo.SpecifiedInParams)]
        void AddExistingSoundRpc(ulong soundID, int index, float time, RpcParams rpcParams)
        {
            PlayingSound tmp = new(
                this,
                rpcParams.Receive.SenderClientId,
                soundID,
                audioSourcePool.GetObject(),
                index);
            tmp.source.Play();
            tmp.source.time = time;
            playingSounds.Add(soundID, tmp);
        }
        [Rpc(SendTo.Everyone)]
        void StopSoundRpc(ulong soundID)
        {
            if (!playingSounds.TryGetValue(soundID, out PlayingSound sound) || removeQueue.Contains(soundID)) return;
            sound.source.Stop();
            audioSourcePool.ReleaseObject(sound.source);
            removeQueue.Add(soundID);
        }
        [Rpc(SendTo.NotMe)]
        internal void SetLoopRpc(ulong soundID, bool loop)
        {
            if (!playingSounds.TryGetValue(soundID, out PlayingSound sound)) return;
            sound.SetLoop(loop);
        }
        [Rpc(SendTo.NotMe)]
        internal void SetVolumeRpc(ulong soundID, float volume)
        {
            if (!playingSounds.TryGetValue(soundID, out PlayingSound sound)) return;
            sound.SetVolume(volume);
        }
        [Rpc(SendTo.NotMe)]
        internal void SetDistanceRpc(ulong soundID, Vector2 location, float minDist, float maxDist)
        {
            if (!playingSounds.TryGetValue(soundID, out PlayingSound sound)) return;
            sound.SetDistance(location, minDist, maxDist);
        }
        [Rpc(SendTo.NotMe)]
        internal void SetDistanceRpc(ulong soundID, NetworkBehaviourReference locationSourceRef, float minDist, float maxDist)
        {
            if (!playingSounds.TryGetValue(soundID, out PlayingSound sound) || !locationSourceRef.TryGet(out NetworkBehaviour locationSource)) return;
            sound.SetDistance(locationSource, minDist, maxDist);
        }
        [Rpc(SendTo.NotMe)]
        internal void RestartRpc(ulong soundID)
        {
            if (!playingSounds.TryGetValue(soundID, out PlayingSound sound)) return;
            sound.Restart();
        }
        [Rpc(SendTo.NotMe)]
        internal void SetVolumeGroupRpc(ulong soundID, VolumeGroup group)
        {
            if (!playingSounds.TryGetValue(soundID, out PlayingSound sound)) return;
            sound.SetVolumeGroup(group);
        }
        internal float GetVolume(VolumeGroup group)
        {
            float val = 1.0f;
            if ((group | VolumeGroup.SFX) != 0) val *= Settings.SFXVolume;
            if ((group | VolumeGroup.BGM) != 0) val *= Settings.BGMVolume;
            return val;
        }
    }
    public class PlayingSound
    {
        public readonly NetworkAudioManager manager;
        public readonly ulong ownerID;
        public readonly ulong id;
        public readonly AudioSource source;
        public readonly int soundIndex;
        public readonly Sound sound;

        public bool isOwner => manager.NetworkManager.LocalClientId == ownerID;
        public PlayingSound(NetworkAudioManager manager, ulong ownerID, ulong id, AudioSource source, int soundIndex)
        {
            this.manager = manager;
            this.ownerID = ownerID;
            this.id = id;
            this.source = source;
            this.soundIndex = soundIndex;
            sound = manager.sounds[soundIndex].sound;
            source.clip = sound.clip;
        }
        public float volume { get; private set; } = 1.0f;
        public PlayingSound SetVolume(float volume)
        {
            if (isOwner) manager.SetVolumeRpc(id, volume);
            this.volume = volume;
            return this;
        }
        public bool loop { get; private set; } = false;
        public PlayingSound SetLoop(bool loop)
        {
            if (isOwner) manager.SetLoopRpc(id, loop);
            this.loop = loop;
            return this;
        }

        public bool hasDistance { get; private set; } = false;
        internal NetworkBehaviour locationSource;
        internal Vector2 location;
        internal float minDist, maxDist;
        public PlayingSound SetDistance(Vector2 location, float minDist, float maxDist)
        {
            if (isOwner) manager.SetDistanceRpc(id, location, minDist, maxDist);
            hasDistance = true;
            this.location = location;
            this.minDist = minDist;
            this.maxDist = maxDist;
            return this;
        }
        public PlayingSound SetDistance(NetworkBehaviour locationSource, float minDist, float maxDist)
        {
            if (isOwner) manager.SetDistanceRpc(id, locationSource, minDist, maxDist);
            hasDistance = true;
            this.locationSource = locationSource;
            this.minDist = minDist;
            this.maxDist = maxDist;
            return this;
        }
        public PlayingSound Restart()
        {
            if (isOwner) manager.RestartRpc(id);
            source.Play();
            source.time = sound.startTime;
            return this;
        }
        public VolumeGroup group { get; private set; } = VolumeGroup.SFX;
        public PlayingSound SetVolumeGroup(VolumeGroup group)
        {
            if (isOwner) manager.SetVolumeGroupRpc(id, group);
            this.group = group;
            return this;
        }
        public void Update()
        {
            float volume = sound.volume * this.volume * manager.GetVolume(group);
            if (hasDistance && Player.localPlayer != null)
            {
                float dist = Vector2.Distance(Player.localPlayer.transform.position, locationSource != null ? locationSource.transform.position : location);
                if (dist < minDist) volume *= 1.0f;
                else if (dist < maxDist) volume *= Mathf.Clamp01(1.0f - (dist - minDist) / (maxDist - minDist));
                else volume *= 0.0f;
            }
            source.volume = volume;
            if (!source.isPlaying && isOwner)
            {
                if (!loop) Stop();
                else Restart();
            }
        }
        public void Stop()
        {
            manager.StopSound(this);
        }
    }
    [Flags]
    [System.Serializable]
    public enum VolumeGroup
    {
        SFX = 1 << 0,
        BGM = 1 << 1,
    }
}