using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using Unity.Services.Authentication;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BGMVolumeSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    private void OnEnable()
    {
        Settings.onBGMVolumeChange += OnBGMVolumeChange;
        slider.onValueChanged.AddListener(OnValueChanged);
        OnBGMVolumeChange();
    }

    private void OnDisable()
    {
        Settings.onBGMVolumeChange -= OnBGMVolumeChange;
        slider.onValueChanged.RemoveListener(OnValueChanged);
    }
    void OnBGMVolumeChange() => slider.value = Settings.BGMVolume;
    void OnValueChanged(float value) => Settings.BGMVolume = value;
}