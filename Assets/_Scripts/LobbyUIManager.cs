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
        }
    }
    private void Start() {
        this.lobbyCodeText.text = $"Lobby Code: {GameManager.Instance.lobbyCode}, Players {NetworkManager.Singleton.ConnectedClientsList.Count}/4";
        //NetworkManager.Singleton.OnConnectionEvent += HandleClientConnected;
        GameManager.Instance.OnPlayerConnected += HandleClientConnected;

        this.copyLobbyCodeButton.onClick.AddListener(() => {
            GUIUtility.systemCopyBuffer = GameManager.Instance.lobbyCode;
        });
    }

    private void HandleClientConnected(string name) {

        AddPlayerCardRpc(name);
    }

    public void AddPlayerCardRpc(string playerName) {
        Debug.Log($"Adding player card for {playerName}");

        //Transform playerCardTransform = Instantiate(this.playerLobbyCardUIPrefab, this.playerCardParentTransform);
        Transform playerCardTransform = Instantiate(this.playerLobbyCardUIPrefab, this.playerCardParentTransform, worldPositionStays:false);
        //playerCardTransform.SetParent(parent:this.playerCardParentTransform, worldPositionStays:false);

        // Set the player name on the card
        LobbyPlayerCard card = playerCardTransform.GetComponent<LobbyPlayerCard>();
        card.SetPlayerName(playerName);

        // Gets the network object and sets the owner to the player who instantiated it
        NetworkObject networkObject = playerCardTransform.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
    }
}
