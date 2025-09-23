using NUnit.Framework;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : NetworkBehaviour {

    [SerializeField] private Button readyUpButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text lobbyCodeText;

    [SerializeField] private Transform playerLobbyCardUIPrefab;
    [SerializeField] private Transform playerCardParentTransform;

    [SerializeField] private Button copyLobbyCodeButton;

    private void Awake() {
        if (!IsServer) {
            startGameButton.gameObject.SetActive(false);
        } else {
            startGameButton.gameObject.SetActive(true);
        }
    }
    private void Start() {
        this.lobbyCodeText.text = $"Lobby Code: {GameManager.Instance.lobbyCode}, Players {NetworkManager.Singleton.ConnectedClientsList.Count}/4";
        GameServerManager.Instance.OnPlayerConnected += HandleClientConnected;

        this.copyLobbyCodeButton.onClick.AddListener(() => {
            GUIUtility.systemCopyBuffer = GameManager.Instance.lobbyCode;
        });

        if (IsServer) {
            AddPlayerCardRpc(name = string.Empty, playerIndex: 1);
        }
    }

    private void HandleClientConnected((string name, int playerIndex) playerInfo) {
        this.lobbyCodeText.text = $"Lobby Code: {GameManager.Instance.lobbyCode}, Players {NetworkManager.Singleton.ConnectedClientsList.Count}/4";
        AddPlayerCardRpc(name: playerInfo.name, playerIndex: playerInfo.playerIndex);
    }

    [Rpc(SendTo.Server)]
    public void AddPlayerCardRpc(string name, int playerIndex) {

        GameObject playerCardUI = Instantiate(this.playerLobbyCardUIPrefab.gameObject);

        // Set the player name on the card
        LobbyPlayerCard card = playerCardUI.GetComponent<LobbyPlayerCard>();
        //card.SetPlayerNameRpc(string.IsNullOrEmpty(name) ? $"Player {NetworkManager.Singleton.ConnectedClientsList.Count}" : name);

        // Gets the network object and sets the owner to the player who instantiated it
        playerCardUI.GetComponent<NetworkObject>().Spawn(true);
        playerCardUI.transform.SetParent(this.playerCardParentTransform);

        card.SetPlayerUsernameRpc($"Player {NetworkManager.Singleton.ConnectedClientsList.Count}");

    }
}
