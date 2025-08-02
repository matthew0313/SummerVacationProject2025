using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// 사운드 이름과 오디오 클립을 연결하기 위한 클래스
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Clips")]
    [SerializeField] private List<Sound> sounds; // 사운드 목록
    private Dictionary<string, AudioClip> soundDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 사운드 목록을 딕셔너리로 변환하여 빠르게 찾을 수 있도록 함
            soundDictionary = new Dictionary<string, AudioClip>();
            foreach (var sound in sounds)
            {
                soundDictionary[sound.name] = sound.clip;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- BGM & Local SFX ---
    public void PlayBGM(string bgmName)
    {
        if (soundDictionary.TryGetValue(bgmName, out AudioClip clip))
        {
            if (bgmSource.clip == clip) return;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayLocalSound(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- Networked Sounds ---
    public void PlayGlobalSound(string soundName, Vector3 position)
    {
        PlaySoundServerRpc(soundName, position, SoundAudience.Global, default);
    }

    public void PlayTeamSound(string soundName, Vector3 position, Team team)
    {
        PlaySoundServerRpc(soundName, position, SoundAudience.Team, team);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaySoundServerRpc(string soundName, Vector3 position, SoundAudience audience, Team targetTeam)
    {
        ClientRpcParams clientRpcParams = default;

        if (audience == SoundAudience.Team)
        {
            var teamPlayers = GameManager.Instance.GetTeamPlayers(targetTeam);
            var clientIds = teamPlayers.Select(p => p.OwnerClientId).ToArray();

            if (clientIds.Length == 0) return;

            clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = clientIds
                }
            };
        }

        PlaySoundClientRpc(soundName, position, clientRpcParams);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(string soundName, Vector3 position, ClientRpcParams clientRpcParams = default)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clipToPlay))
        {
            AudioSource.PlayClipAtPoint(clipToPlay, position);
        }
    }

    private enum SoundAudience
    {
        Global,
        Team
    }
}

