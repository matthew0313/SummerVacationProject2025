using Unity.Netcode;
using UnityEngine;

public class Ward : Entity
{
    [Header("Ward Configuration")]
    [SerializeField] private WardComponent wardComponent;
    [SerializeField] private SpriteRenderer wardSprite;
    [SerializeField] private Animator wardAnimator;
    
    [Header("Ward Properties")]
    [SerializeField] private WardType wardType = WardType.Vision;
    [SerializeField] private float placementRange = 3f;
    [SerializeField] private bool canBeDestroyed = true;
    [SerializeField] private float destroyTime = 2f;
    
    public enum WardType
    {
        Vision,
        Damage
    }
    
    public WardType Type => wardType;
    public float PlacementRange => placementRange;
    public bool CanBeDestroyed => canBeDestroyed;
    
    private bool isPlaced = false;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (wardComponent == null)
        {
            wardComponent = GetComponent<WardComponent>();
        }
        
        if (wardComponent != null)
        {
            wardComponent.onWardPlaced += OnWardPlaced;
            wardComponent.onWardDestroyed += OnWardDestroyed;
        }
        
        ConfigureWardByType();
    }
    
    private void ConfigureWardByType()
    {
        switch (wardType)
        {
            case WardType.Vision:
                if (wardComponent != null)
                {
                    wardComponent.SetWardColor(Color.cyan);
                    wardComponent.SetWardRadius(4f);
                }
                break;
                
            case WardType.Damage:
                if (wardComponent != null)
                {
                    wardComponent.SetWardColor(new Color(1f, 0.5f, 0f));
                    wardComponent.SetWardRadius(2f);
                }
                break;
        }
    }
    
    private void OnWardPlaced(WardComponent ward)
    {
        isPlaced = true;
        
        if (wardAnimator != null)
        {
            wardAnimator.SetTrigger("Place");
        }
        
        PlayPlacementEffects();
    }
    
    private void OnWardDestroyed(WardComponent ward)
    {
        isPlaced = false;
        
        if (wardAnimator != null)
        {
            wardAnimator.SetTrigger("Destroy");
        }
        
        PlayDestructionEffects();
    }
    
    private void PlayPlacementEffects()
    {
        if (wardSprite != null)
        {
            StartCoroutine(FadeInSprite());
        }
    }
    
    private void PlayDestructionEffects()
    {
        if (wardSprite != null)
        {
            StartCoroutine(FadeOutSprite());
        }
    }
    
    private System.Collections.IEnumerator FadeInSprite()
    {
        Color color = wardSprite.color;
        color.a = 0f;
        wardSprite.color = color;
        
        float elapsed = 0f;
        float duration = 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            wardSprite.color = color;
            yield return null;
        }
        
        color.a = 1f;
        wardSprite.color = color;
    }
    
    private System.Collections.IEnumerator FadeOutSprite()
    {
        Color color = wardSprite.color;
        float startAlpha = color.a;
        
        float elapsed = 0f;
        float duration = destroyTime;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            wardSprite.color = color;
            yield return null;
        }
        
        color.a = 0f;
        wardSprite.color = color;
    }
    
    public bool CanPlaceWard(Vector3 position)
    {
        return true;
    }
    
    public void SetWardType(WardType type)
    {
        wardType = type;
        ConfigureWardByType();
    }
    
    public void SetPlacementRange(float range)
    {
        placementRange = range;
    }
    
    public void SetCanBeDestroyed(bool destroyable)
    {
        canBeDestroyed = destroyable;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, placementRange);
        
        if (wardComponent != null && wardComponent.IsActive)
        {
            Gizmos.color = GetWardTypeColor();
            Gizmos.DrawWireSphere(transform.position, wardComponent.WardRadius);
        }
    }
    
    private Color GetWardTypeColor()
    {
        switch (wardType)
        {
            case WardType.Vision: return Color.cyan;
            case WardType.Damage: return new Color(1f, 0.5f, 0f);
            default: return Color.white;
        }
    }
} 