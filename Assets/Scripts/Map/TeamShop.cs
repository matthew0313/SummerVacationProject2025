using Unity.Netcode;
using UnityEngine;

public class TeamShop : MonoBehaviour
{
    ShopTab shopTab => GameUIManager.Instance.shopTab;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Player.localPlayer == null) return;
        GameObject hit = collision.attachedRigidbody != null ? collision.attachedRigidbody.gameObject : collision.gameObject;
        if(hit == Player.localPlayer.gameObject && !GameUIManager.Instance.isTabOpen)
        {
            GameUIManager.Instance.OpenTab(shopTab);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Player.localPlayer == null) return;
        GameObject hit = collision.attachedRigidbody != null ? collision.attachedRigidbody.gameObject : collision.gameObject;
        if (hit == Player.localPlayer.gameObject && GameUIManager.Instance.currentTab == shopTab)
        {
            GameUIManager.Instance.CloseTab();
        }
    }
}