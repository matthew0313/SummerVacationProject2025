using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNameUI : NetworkBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    public readonly NetworkVariable<FixedString512Bytes> playerName = new();
    private void Start()
    {
        playerName.OnValueChanged += (prev, val) => playerNameText.text = val.ToString();
        playerNameText.text = playerName.Value.ToString();
    }
    private void OnEnable()
    {
        if (IsOwner)
        {
            SessionManager.Instance.onPlayerNameChange += OnPlayerNameChange;
            OnPlayerNameChange(SessionManager.Instance.playerName);
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