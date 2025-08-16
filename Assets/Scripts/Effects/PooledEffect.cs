using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PooledEffect : PooledPrefab<PooledEffect>
{
    [SerializeField] float duration = 3.0f;
    [SerializeField] UnityEvent onGet;
    float counter = 0.0f;
    protected override void OnGet()
    {
        base.OnGet();
        counter = 0.0f;
        onGet?.Invoke();
    }
    private void Update()
    {
        counter += Time.deltaTime;
        if (counter > duration) Release();
    }
}