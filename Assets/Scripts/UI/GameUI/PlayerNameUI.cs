using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNameUI : NetworkBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] float visibleDistance = 8.0f;
    public readonly NetworkVariable<FixedString512Bytes> playerName = new();
    private void Start()
    {
        playerName.OnValueChanged += (prev, val) => playerNameText.text = val.ToString();
        playerNameText.text = playerName.Value.ToString();
        if (IsOwner)
        {
            OnPlayerNameChange(SessionManager.Instance.playerName);
        }
    }
    private void Update()
    {
        if (!IsOwner && Player.localPlayer != null)
        {
            playerNameText.gameObject.SetActive(Vector2.Distance(playerNameText.transform.position, Player.localPlayer.transform.position) <= visibleDistance);
        }
    }
    private void OnEnable()
    {
        if (IsOwner)
        {
            SessionManager.Instance.onPlayerNameChange += OnPlayerNameChange;
        }
    }
    private void OnDisable()
    {
        if (IsOwner)
        {
            SessionManager.Instance.onPlayerNameChange -= OnPlayerNameChange;
        }
    }
    void OnPlayerNameChange(string name) => playerName.Value = name;
}