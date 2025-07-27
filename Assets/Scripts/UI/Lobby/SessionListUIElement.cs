using Unity.Netcode;
using UnityEngine;
using Unity.Multiplayer.Widgets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplayer;

public class SessionListUIElement : MonoBehaviour
{
    [SerializeField] Button sessionJoinButton;
    [SerializeField] TMP_Text sessionID, sessionInfo;
    private void OnEnable()
    {
        sessionJoinButton.onClick.AddListener(JoinSession);
    }
    private void OnDisable()
    {
        sessionJoinButton.onClick.AddListener(JoinSession);
    }
    ISessionInfo currentInfo;
    public void Set(ISessionInfo info)
    {
        currentInfo = info;
        sessionID.text = $"ID: {info.Id}";
        sessionInfo.text = $"플레이어 수: {info.MaxPlayers - info.AvailableSlots}/{info.MaxPlayers}";
    }
    void JoinSession()
    {
        SessionManager.Instance.JoinSessionById(currentInfo.Id);
    }
}