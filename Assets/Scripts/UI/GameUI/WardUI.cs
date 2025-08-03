using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class WardUI : MonoBehaviour, ILocalPlayerSpawnCallback
{
    Player player;
    [SerializeField] Animator anim;
    [SerializeField] TMP_Text wardPlacedText, wardOwnedText;

    public void OnLocalPlayerSpawn(Player localPlayer)
    {
        player = localPlayer;
        player.wardComp.onWardsOwnedChange += OnWardsOwnedChange;
        player.wardComp.onWardsPlacedChange += OnWardsPlacedChange;
        OnWardsOwnedChange(); OnWardsPlacedChange();
    }
    readonly int canPlaceID = Animator.StringToHash("CanPlace");
    readonly int cooldownFillID = Animator.StringToHash("CooldownFill");
    private void Update()
    {
        if (player == null) return;
        anim.SetBool(canPlaceID, player.wardComp.canPlaceWard);
        anim.SetFloat(cooldownFillID, player.wardComp.wardPlaceCounter / player.wardComp.wardPlaceCooldown);
    }
    void OnWardsOwnedChange()
    {
        wardOwnedText.text = player.wardComp.wardsOwned.ToString();
    }
    void OnWardsPlacedChange()
    {
        wardPlacedText.text = $"{player.wardComp.wardsPlaced.Count}/{player.wardComp.maxWardsPlaced}";
    }
    #region old
    /*[Header("Ward Status")]
    [SerializeField] private GameObject wardStatusPanel;
    [SerializeField] private Image wardCooldownImage;
    [SerializeField] private TextMeshProUGUI wardCountText;
    [SerializeField] private TextMeshProUGUI wardTypeText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    
    [Header("Ward Type Icons")]
    [SerializeField] private Image wardTypeIcon;
    [SerializeField] private Sprite visionWardSprite;
    [SerializeField] private Sprite damageWardSprite;
    
    [Header("Ward Type Colors")]
    [SerializeField] private Color visionColor = Color.cyan;
    [SerializeField] private Color damageColor = new Color(1f, 0.5f, 0f);
    
    private Player localPlayer;
    private WardManager wardManager;
    
    private void Start()
    {
        wardManager = WardManager.Instance;
        
        if (wardManager != null)
        {
            wardManager.OnWardPlaced += OnWardPlaced;
            wardManager.OnWardDestroyed += OnWardDestroyed;
            wardManager.OnWardCooldownChanged += OnWardCooldownChanged;
        }
    }
    
    private void Update()
    {
        if (localPlayer == null)
        {
            localPlayer = Player.localPlayer;
            return;
        }
        
        UpdateWardUI();
    }
    
    private void UpdateWardUI()
    {
        if (localPlayer == null || wardManager == null) return;
        
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        
        int wardCount = wardManager.GetWardCount(clientId);
        int maxWards = 3;
        wardCountText.text = $"Wards: {wardCount}/{maxWards}";
        
        Ward.WardType currentType = localPlayer.GetCurrentWardType();
        wardTypeText.text = $"Type: {GetWardTypeName(currentType)}";
        
        UpdateWardTypeIcon(currentType);
        
        float cooldown = wardManager.GetWardCooldown(clientId);
        if (cooldown > 0)
        {
            cooldownText.text = $"Cooldown: {cooldown:F1}s";
            wardCooldownImage.fillAmount = cooldown / 30f;
            wardCooldownImage.gameObject.SetActive(true);
        }
        else
        {
            cooldownText.text = "Ready!";
            wardCooldownImage.gameObject.SetActive(false);
        }
        
        bool canPlaceWard = wardManager.CanPlaceWard(clientId);
        wardStatusPanel.SetActive(true);
        
        Color panelColor = canPlaceWard ? Color.white : Color.gray;
        wardStatusPanel.GetComponent<Image>().color = panelColor;
    }
    
    private void UpdateWardTypeIcon(Ward.WardType wardType)
    {
        Sprite iconSprite = null;
        Color iconColor = Color.white;
        
        switch (wardType)
        {
            case Ward.WardType.Vision:
                iconSprite = visionWardSprite != null ? visionWardSprite : CreateDefaultSprite();
                iconColor = visionColor;
                break;
            case Ward.WardType.Damage:
                iconSprite = damageWardSprite != null ? damageWardSprite : CreateDefaultSprite();
                iconColor = damageColor;
                break;
        }
        
        if (wardTypeIcon != null)
        {
            if (iconSprite != null)
            {
                wardTypeIcon.sprite = iconSprite;
            }
            wardTypeIcon.color = iconColor;
        }
    }
    
    private Sprite CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    private string GetWardTypeName(Ward.WardType wardType)
    {
        switch (wardType)
        {
            case Ward.WardType.Vision: return "Vision";
            case Ward.WardType.Damage: return "Damage";
            default: return "Unknown";
        }
    }
    
    private void OnWardPlaced(Ward ward)
    {
        if (wardTypeIcon != null)
        {
            StartCoroutine(FlashIcon(wardTypeIcon));
        }
    }
    
    private void OnWardDestroyed(Ward ward)
    {
        if (wardTypeIcon != null)
        {
            StartCoroutine(ShakeIcon(wardTypeIcon));
        }
    }
    
    private void OnWardCooldownChanged(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            UpdateWardUI();
        }
    }
    
    private System.Collections.IEnumerator FlashIcon(Image icon)
    {
        Color originalColor = icon.color;
        Color flashColor = Color.white;
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            icon.color = Color.Lerp(originalColor, flashColor, Mathf.Sin(progress * Mathf.PI));
            yield return null;
        }
        
        icon.color = originalColor;
    }
    
    private System.Collections.IEnumerator ShakeIcon(Image icon)
    {
        Vector3 originalPosition = icon.transform.localPosition;
        float shakeAmount = 5f;
        float shakeDuration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            float shake = shakeAmount * (1f - progress);
            
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shake, shake),
                Random.Range(-shake, shake),
                0f
            );
            
            icon.transform.localPosition = originalPosition + shakeOffset;
            yield return null;
        }
        
        icon.transform.localPosition = originalPosition;
    }
    
    public void SetWardType(int wardTypeIndex)
    {
        if (localPlayer != null)
        {
            Ward.WardType wardType = (Ward.WardType)wardTypeIndex;
            localPlayer.SetWardType(wardType);
        }
    }
    
    private void OnDestroy()
    {
        if (wardManager != null)
        {
            wardManager.OnWardPlaced -= OnWardPlaced;
            wardManager.OnWardDestroyed -= OnWardDestroyed;
            wardManager.OnWardCooldownChanged -= OnWardCooldownChanged;
        }
    }*/
    #endregion
}