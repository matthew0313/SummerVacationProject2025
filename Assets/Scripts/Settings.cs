using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public static class Settings
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        m_SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        m_BGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
    }
    static float m_SFXVolume, m_BGMVolume;
    public static float SFXVolume
    {
        get => m_SFXVolume;
        set
        {
            m_SFXVolume = value;
            onSFXVolumeChange?.Invoke();
            PlayerPrefs.SetFloat("SFXVolume", value);
        }
    }
    public static float BGMVolume
    {
        get => m_BGMVolume;
        set
        {
            m_BGMVolume = value;
            onBGMVolumeChange?.Invoke();
            PlayerPrefs.SetFloat("BGMVolume", value);
        }
    }
    public static event Action onSFXVolumeChange, onBGMVolumeChange;
}