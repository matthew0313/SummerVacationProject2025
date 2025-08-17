using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.InputSystem;

public class PlayerNameUI : NetworkBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] float visibleDistance = 8.0f;
    public readonly NetworkVariable<FixedString512Bytes> playerName = new();
    private void Start()
    {
        playerName.OnValueChanged += (prev, val) =>
        {
            playerNameText.text = val.ToString();
            Debug.Log(val.ToString());
        };
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
        SessionManager.Instance.onPlayerNameChange += OnPlayerNameChange;
    }
    private void OnDisable()
    {
        SessionManager.Instance.onPlayerNameChange -= OnPlayerNameChange;
    }
    void OnPlayerNameChange(string name)
    {
        if (!IsOwner) return;
        Debug.Log("RRRR");
        playerName.Value = name;
    }
}