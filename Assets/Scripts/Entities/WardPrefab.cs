using UnityEngine;

/*[CreateAssetMenu(fileName = "WardPrefab", menuName = "Game/Ward Prefab")]
public class WardPrefab : ScriptableObject
{
    [Header("Ward Prefab Settings")]
    [SerializeField] private GameObject wardPrefab;
    [SerializeField] private Ward.WardType defaultWardType = Ward.WardType.Vision;
    [SerializeField] private float defaultRadius = 4f;
    [SerializeField] private float defaultDuration = 60f;
    [SerializeField] private Color defaultColor = Color.cyan;
    
    [Header("Visual Settings")]
    [SerializeField] private Sprite wardSprite;
    [SerializeField] private Material wardMaterial;
    [SerializeField] private ParticleSystem placementEffect;
    [SerializeField] private ParticleSystem destructionEffect;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip placementSound;
    [SerializeField] private AudioClip destructionSound;
    [SerializeField] private AudioClip ambientSound;
    
    public GameObject Prefab => wardPrefab;
    public Ward.WardType DefaultType => defaultWardType;
    public float DefaultRadius => defaultRadius;
    public float DefaultDuration => defaultDuration;
    public Color DefaultColor => defaultColor;
    
    public Sprite WardSprite => wardSprite;
    public Material WardMaterial => wardMaterial;
    public ParticleSystem PlacementEffect => placementEffect;
    public ParticleSystem DestructionEffect => destructionEffect;
    
    public AudioClip PlacementSound => placementSound;
    public AudioClip DestructionSound => destructionSound;
    public AudioClip AmbientSound => ambientSound;
    
    public GameObject CreateWardInstance(Vector3 position, Quaternion rotation)
    {
        if (wardPrefab == null)
        {
            Debug.LogError("Ward prefab is not assigned!");
            return null;
        }
        
        GameObject instance = Instantiate(wardPrefab, position, rotation);
        
        Ward ward = instance.GetComponent<Ward>();
        if (ward != null)
        {
            ward.SetWardType(defaultWardType);
        }
        
        WardComponent wardComponent = instance.GetComponent<WardComponent>();
        if (wardComponent != null)
        {
            wardComponent.SetWardRadius(defaultRadius);
            wardComponent.SetWardColor(defaultColor);
        }
        
        return instance;
    }
    
    public void ConfigureWardInstance(GameObject wardInstance)
    {
        if (wardInstance == null) return;
        
        SpriteRenderer spriteRenderer = wardInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && wardSprite != null)
        {
            spriteRenderer.sprite = wardSprite;
        }
        
        if (wardMaterial != null)
        {
            Renderer renderer = wardInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = wardMaterial;
            }
        }
        
        WardComponent wardComponent = wardInstance.GetComponent<WardComponent>();
        if (wardComponent != null)
        {
            var audioSource = wardInstance.GetComponent<AudioSource>();
            if (audioSource != null)
            {
            }
        }
    }
} */