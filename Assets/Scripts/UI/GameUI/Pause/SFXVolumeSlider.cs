using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using Unity.Services.Authentication;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SFXVolumeSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    private void OnEnable()
    {
        Settings.onSFXVolumeChange += OnSFXVolumeChange;
        slider.onValueChanged.AddListener(OnValueChanged);
        OnSFXVolumeChange();
    }

    private void OnDisable()
    {
        Settings.onSFXVolumeChange -= OnSFXVolumeChange;
        slider.onValueChanged.RemoveListener(OnValueChanged);
    }
    void OnSFXVolumeChange() => slider.value = Settings.SFXVolume;
    void OnValueChanged(float value) => Settings.SFXVolume = value;
}