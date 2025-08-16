using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using DG.Tweening;
using JetBrains.Annotations;

public class ShopDealUI_GenUpgrade : ShopDealUI
{
    [SerializeField] TMP_Text costText;
    int upgradeCost
    {
        get
        {
            var tmp = GameManager.Instance.GetBase(Player.localPlayer.team.Value);
            if (tmp != null) return tmp.generator.upgradeCost;
            else return 0;
        }
    }
    protected override void Purchase()
    {
        var teamBase = GameManager.Instance.GetBase(Player.localPlayer.team.Value);
        if(teamBase == null)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>해당 팀의 기지가 없습니다.</color>", Player.localPlayer);
            return;
        }
        if (Player.localPlayer.inventory.bolt.Value < upgradeCost)
        {
            MessageManager.Instance.SendMessageToPlayer("<color=#FF0000>스크랩이 부족합니다.</color>", Player.localPlayer);
            return;
        }
        var generator = GameManager.Instance.GetBase(Player.localPlayer.team.Value).generator;
        generator.UpgradeRpc(generator.upgradeTier.Value + 1);
    }
    private void Update()
    {
        costText.text = upgradeCost.ToString();
    }
}