using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

public class LobbyUI : TabOpener
{
    [SerializeField] Animator anim;

    static bool introPlayed = false;
    readonly int introID = Animator.StringToHash("Intro");
    private void Awake()
    {
        if (!introPlayed)
        {
            anim.SetTrigger(introID);
            introPlayed = true;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseTab();
    }
}
