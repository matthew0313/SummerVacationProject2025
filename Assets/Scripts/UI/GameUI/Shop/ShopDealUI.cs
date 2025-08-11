using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;

public abstract class ShopDealUI : MonoBehaviour
{
    [SerializeField] protected Button purchaseButton;
    protected Player player => Player.localPlayer;
    private void OnEnable()
    {
        purchaseButton.onClick.AddListener(Purchase);
    }
    private void OnDisable()
    {
        purchaseButton.onClick.RemoveListener(Purchase);
    }
    protected abstract void Purchase();
}