using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialTab : Tab
{
    [Header("Tween")]
    [SerializeField] RectTransform tweenTarget;
    [SerializeField] CanvasGroup tweenAlphaTarget;
    [SerializeField] float tweenTime = 0.5f;
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
                    tweenAlphaTarget.alpha = tweenCounter;
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
}
