using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WardComponent : NetworkBehaviour
{
    [Header("Ward Settings")]
    [SerializeField] private float wardRadius = 4f;
    [SerializeField] private float wardDuration = 60f;
    [SerializeField] private Color wardLightColor = Color.cyan;
    [SerializeField] private float lightIntensity = 1f;
    [SerializeField] private LayerMask visionLayerMask = -1;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject wardVisualPrefab;
    [SerializeField] private ParticleSystem wardParticles;
    [SerializeField] private AudioSource wardAudioSource;
    [SerializeField] private AudioClip wardPlaceSound;
    [SerializeField] private AudioClip wardDestroySound;
    
    private NetworkVariable<float> remainingDuration = new(60f);
    private NetworkVariable<bool> isActive = new(false);
    private Light2D wardLight;
    private GameObject wardVisual;
    private float fadeOutTime = 2f;
    
    public event Action<WardComponent> onWardDestroyed;
    public event Action<WardComponent> onWardPlaced;
    
    public float WardRadius => wardRadius;
    public float RemainingDuration => remainingDuration.Value;
    public bool IsActive => isActive.Value;
    
    private void Awake()
    {
        InitializeWard();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (HasAuthority)
        {
            PlaceWardRpc();
        }
        
        remainingDuration.OnValueChanged += OnDurationChanged;
        isActive.OnValueChanged += OnActiveStateChanged;
    }
    
    private void InitializeWard()
    {
        GameObject lightObj = new GameObject("WardLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        wardLight = lightObj.AddComponent<Light2D>();
        wardLight.lightType = Light2D.LightType.Point;
        wardLight.pointLightOuterRadius = wardRadius;
        wardLight.pointLightInnerRadius = wardRadius * 0.3f;
        wardLight.intensity = lightIntensity;
        wardLight.color = wardLightColor;
        
        if (wardVisualPrefab != null)
        {
            wardVisual = Instantiate(wardVisualPrefab, transform);
            wardVisual.transform.localPosition = Vector3.zero;
        }
        
        if (wardParticles != null)
        {
            wardParticles.transform.SetParent(transform);
            wardParticles.transform.localPosition = Vector3.zero;
        }
        
        if (wardAudioSource == null)
        {
            wardAudioSource = gameObject.AddComponent<AudioSource>();
            wardAudioSource.spatialBlend = 1f;
            wardAudioSource.volume = 0.7f;
        }
    }
    
    [Rpc(SendTo.Everyone)]
    private void PlaceWardRpc()
    {
        isActive.Value = true;
        remainingDuration.Value = wardDuration;
        
        if (wardAudioSource != null && wardPlaceSound != null)
        {
            wardAudioSource.PlayOneShot(wardPlaceSound);
        }
        
        if (wardParticles != null)
        {
            wardParticles.Play();
        }
        
        onWardPlaced?.Invoke(this);
    }
    
    private void Update()
    {
        if (!isActive.Value) return;
        
        if (HasAuthority)
        {
            UpdateWardDuration();
        }
        
        UpdateWardEffects();
    }
    
    private void UpdateWardDuration()
    {
        if (remainingDuration.Value > 0)
        {
            remainingDuration.Value -= Time.deltaTime;
            
            if (remainingDuration.Value <= 0)
            {
                DestroyWardRpc();
            }
        }
    }
    
    private void UpdateWardEffects()
    {
        if (remainingDuration.Value <= fadeOutTime && remainingDuration.Value > 0)
        {
            float fadeProgress = 1f - (remainingDuration.Value / fadeOutTime);
            if (wardLight != null)
            {
                wardLight.intensity = lightIntensity * (1f - fadeProgress * 0.5f);
            }
        }
    }
    
    [Rpc(SendTo.Everyone)]
    private void DestroyWardRpc()
    {
        isActive.Value = false;
        
        if (wardAudioSource != null && wardDestroySound != null)
        {
            wardAudioSource.PlayOneShot(wardDestroySound);
        }
        
        if (wardLight != null)
        {
            StartCoroutine(FadeOutLight());
        }
        
        if (wardParticles != null)
        {
            wardParticles.Stop();
        }
        
        onWardDestroyed?.Invoke(this);
        
        Invoke(nameof(DestroyWard), 2f);
    }
    
    private System.Collections.IEnumerator FadeOutLight()
    {
        float startIntensity = wardLight.intensity;
        float elapsed = 0f;
        
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutTime;
            wardLight.intensity = startIntensity * (1f - progress);
            yield return null;
        }
        
        wardLight.intensity = 0f;
    }
    
    private void DestroyWard()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
        }
    }
    
    private void OnDurationChanged(float previousValue, float newValue)
    {
    }
    
    private void OnActiveStateChanged(bool previousValue, bool newValue)
    {
        if (wardLight != null)
        {
            wardLight.enabled = newValue;
        }
        
        if (wardVisual != null)
        {
            wardVisual.SetActive(newValue);
        }
    }
    
    public bool IsInWardRange(Vector3 position)
    {
        if (!isActive.Value) return false;
        
        float distance = Vector3.Distance(transform.position, position);
        return distance <= wardRadius;
    }
    
    public void SetWardColor(Color color)
    {
        if (wardLight != null)
        {
            wardLight.color = color;
        }
    }
    
    public void SetWardRadius(float radius)
    {
        wardRadius = radius;
        if (wardLight != null)
        {
            wardLight.pointLightOuterRadius = radius;
            wardLight.pointLightInnerRadius = radius * 0.3f;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (isActive.Value)
        {
            Gizmos.color = wardLightColor;
            Gizmos.DrawWireSphere(transform.position, wardRadius);
        }
    }
} 