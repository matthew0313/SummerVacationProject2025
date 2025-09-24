using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.InputSystem;

public class PlayerNameUI : NetworkBehaviour
{
    [SerializeField] Player targetPlayer;
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] float visibleDistance = 8.0f;
    [SerializeField] Color noTeam, redTeam, blueTeam, spectator;
    private void Start()
    {
        targetPlayer.playerName.OnValueChanged += (prev, val) =>
        {
            playerNameText.text = val.ToString();
        };
        playerNameText.text = targetPlayer.playerName.Value.ToString();
    }
    private void Update()
    {
        if (!IsOwner && Player.localPlayer != null)
        {
            Vector2 dir = Player.localPlayer.transform.position - targetPlayer.transform.position;
            playerNameText.gameObject.SetActive(dir.magnitude <= visibleDistance);
        }
        if (targetPlayer.spectator.Value)
        {
            playerNameText.color = spectator;
        }
        else
        {
            switch (targetPlayer.team.Value)
            {
                case Team.None: playerNameText.color = noTeam; break;
                case Team.Red: playerNameText.color = redTeam; break;
                case Team.Blue: playerNameText.color = blueTeam; break;
            }
        }
    }
}