using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

public abstract class Tab : MonoBehaviour
{
    public bool isOpen { get; private set; } = false;
    public virtual void OpenTab() { isOpen = true; }
    public virtual void CloseTab() { isOpen = false; }
}