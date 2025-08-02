using Unity.Netcode;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // 배경음악용
    [SerializeField] private AudioSource sfxSource; // UI 효과음용

    [Header("Audio Clips")]
    public AudioClip lobbyBGM;
    public AudioClip gameBGM;
    public AudioClip wardPlaceSound;
    public AudioClip wardDestroySound;
    public AudioClip uiClickSound;
    public AudioClip bulletFireSound; // 총알 발사음 추가

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

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayUISound(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // 모든 클라이언트에게 특정 위치에서 사운드 재생을 지시하는 RPC
    [ClientRpc]
    public void PlaySoundClientRpc(string clipName, Vector3 position)
    {
        AudioClip clipToPlay = null;
        switch (clipName)
        {
            case "wardPlace":
                clipToPlay = wardPlaceSound;
                break;
            case "wardDestroy":
                clipToPlay = wardDestroySound;
                break;
            case "bulletFire":
                clipToPlay = bulletFireSound;
                break;
        }

        if (clipToPlay != null)
        {
            // 3D 공간의 특정 위치에서 사운드를 재생합니다.
            AudioSource.PlayClipAtPoint(clipToPlay, position);
        }
    }
}