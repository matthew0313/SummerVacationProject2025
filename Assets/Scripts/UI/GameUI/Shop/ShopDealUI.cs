using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;

public abstract class ShopDealUI : MonoBehaviour
{
    [SerializeField] protected Button purchaseButton;
    [SerializeField] protected int gasCost, boltsCost;
    private void OnEnable()
    {
        purchaseButton.onClick.AddListener(OnPurchaseButtonClick);
    }
    private void OnDisable()
    {
        purchaseButton.onClick.RemoveListener(OnPurchaseButtonClick);
    }
    protected abstract void OnPurchaseButtonClick();
}