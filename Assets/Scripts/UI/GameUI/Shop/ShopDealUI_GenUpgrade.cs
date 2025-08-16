using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;
using JetBrains.Annotations;

public class ShopDealUI_GenUpgrade : ShopDealUI
{
    [SerializeField] TMP_Text costText;

    TeamGenerator generator
    {
        get
        {
            var tmp = GameManager.Instance.GetBase(Player.localPlayer.team.Value);
            if (tmp != null) return tmp.generator;
            else return null;
        }
    }
    protected override void Purchase()
    {
        if(generator == null)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>해당 팀의 발전기가 없습니다.</color>", Player.localPlayer);
            return;
        }
        if (Player.localPlayer.inventory.bolt.Value < generator.upgradeCost)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>스크랩이 부족합니다.</color>", Player.localPlayer);
            return;
        }
        if(generator.upgradeTier.Value >= TeamGenerator.maxTier)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FFFF00>이미 최대 단계입니다.</color>", Player.localPlayer);
            return;
        }
        generator.UpgradeRpc(generator.upgradeTier.Value + 1);
    }
    private void Update()
    {
        costText.text = generator != null ? generator.upgradeCost.ToString() : "0";
        purchaseButton.interactable = generator != null && generator.upgradeTier.Value < TeamGenerator.maxTier;
    }
}