using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

public class TabOpener : MonoBehaviour
{
    Tab currentTab;
    public void OpenTab(Tab tab)
    {
        if (currentTab == tab)
        {
            CloseTab(); return;
        }

        if (currentTab != null) currentTab.CloseTab();
        currentTab = tab;
        if (currentTab != null) currentTab.OpenTab();
    }
    public void CloseTab()
    {
        if (currentTab != null) currentTab.CloseTab();
        currentTab = null;
    }
}