using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Multiplayer;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

public class TabOpener : MonoBehaviour
{
    [SerializeField] Tab m_currentTab = null;
    public bool isTabOpen => currentTab != null;
    public Tab currentTab { get => m_currentTab; private set => m_currentTab = value; }
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