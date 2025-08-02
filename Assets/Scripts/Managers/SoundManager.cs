using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip lobbyBGM;
    public AudioClip gameBGM;
    public AudioClip wardPlaceSound;
    public AudioClip wardDestroySound;
    public AudioClip uiClickSound;
    public AudioClip bulletFireSound;
    public AudioClip teamAlertSound; // 팀 전용 사운드 클립 예시

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- BGM & Local SFX ---
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayLocalSound(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // --- Networked Sounds ---
    public void PlayGlobalSound(string clipName, Vector3 position)
    {
        PlaySoundServerRpc(clipName, position, SoundAudience.Global, default);
    }

    public void PlayTeamSound(string clipName, Vector3 position, Team team)
    {
        PlaySoundServerRpc(clipName, position, SoundAudience.Team, team);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaySoundServerRpc(string clipName, Vector3 position, SoundAudience audience, Team targetTeam)
    {
        ClientRpcParams clientRpcParams = default;

        if (audience == SoundAudience.Team)
        {
            var teamPlayers = GameManager.Instance.GetTeamPlayers(targetTeam);
            var clientIds = teamPlayers.Select(p => p.OwnerClientId).ToArray();

            if (clientIds.Length == 0) return; // 팀원이 없으면 실행하지 않음

            clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = clientIds
                }
            };
        }

        PlaySoundClientRpc(clipName, position, clientRpcParams);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(string clipName, Vector3 position, ClientRpcParams clientRpcParams = default)
    {
        AudioClip clipToPlay = GetClipFromName(clipName);
        if (clipToPlay != null)
        {
            AudioSource.PlayClipAtPoint(clipToPlay, position);
        }
    }

    private AudioClip GetClipFromName(string clipName)
    {
        switch (clipName)
        {
            case "wardPlace": return wardPlaceSound;
            case "wardDestroy": return wardDestroySound;
            case "bulletFire": return bulletFireSound;
            case "teamAlert": return teamAlertSound;
            default: return null;
        }
    }

    private enum SoundAudience
    {
        Global,
        Team
    }
}
