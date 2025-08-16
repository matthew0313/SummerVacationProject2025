using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class ShopTab : Tab
{
    [Header("Tween")]
    [SerializeField] RectTransform tweenTarget;
    [SerializeField] float tweenTime = 0.5f;
    [SerializeField] TMP_Text gasText, boltText;
    Tween tween;
    float tweenCounter = 0.0f;
    public override void OpenTab()
    {
        base.OpenTab();
        if (tween == null) tween = DOTween.To(() => tweenCounter, value => tweenCounter = value, 1.0f, tweenTime)
                .SetAutoKill(false)
                .SetEase(Ease.OutCirc)
                .OnUpdate(() =>
                {
                    tweenTarget.anchorMin = new Vector2(0, 1.0f - tweenCounter);
                    tweenTarget.anchorMax = new Vector2(1.0f, 2.0f - tweenCounter);
                });
        gameObject.SetActive(true);
        tween.OnStepComplete(null);
        tween.PlayForward();
    }
    public override void CloseTab()
    {
        base.CloseTab();
        tween.OnStepComplete(() => gameObject.SetActive(false));
        tween.PlayBackwards();
    }
    private void Update()
    {
        gasText.text = Player.localPlayer.inventory.gas.Value.ToString();
        boltText.text = Player.localPlayer.inventory.bolt.Value.ToString();
    }
}