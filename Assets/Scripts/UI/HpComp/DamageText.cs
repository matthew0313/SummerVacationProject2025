using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class DamageText : PooledPrefab<DamageText>
{
    [SerializeField] TMP_Text text;
    [SerializeField] float disappearTime = 1.0f;
    [SerializeField] float moveDistance = 3.0f;
    public void Set(float damage, Vector2 dir)
    {
        text.text = damage.ToString();
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1.0f);

        transform.DOMove((Vector2)transform.position + dir * moveDistance, disappearTime).SetEase(Ease.OutCirc).OnComplete(() => { Release(); });
        text.DOColor(new Color(text.color.r, text.color.g, text.color.b, 0.0f), disappearTime).SetEase(Ease.OutCirc);
    }
}