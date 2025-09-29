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

    private void Start() {
        this.lobbyCodeText.text = $"Lobby Code: {GameManager.Instance.lobbyCode}, Players {NetworkManager.Singleton.ConnectedClientsList.Count}/4";
        GameServerManager.Instance.OnPlayerConnected += HandleClientConnected;

        this.copyLobbyCodeButton.onClick.AddListener(() => {
            GUIUtility.systemCopyBuffer = GameManager.Instance.lobbyCode;
        });

        if (IsServer) {
            AddPlayerCardRpc(name = string.Empty, playerIndex: 1);
            this.startGameButton.gameObject.SetActive(true);
            this.startGameButton.interactable = false;
            this.startGameButton.onClick.AddListener(OnStartGameButton);

        } else {
            this.startGameButton.gameObject.SetActive(false);
        }

        this.readyUpButton.onClick.AddListener(OnReadyButton);

    }

    public override void OnNetworkDespawn() {
        this.readyUpButton.onClick.RemoveListener(OnReadyButton);
        this.startGameButton.onClick.RemoveListener(OnStartGameButton);
    }
    private async void OnReadyButton() {
        // Disable the button to prevent multiple clicks
        this.readyUpButton.interactable = false;

        // Get the local player's card
        LobbyPlayerCard myPlayerCard = this.GetLobbyPlayerCard((int)NetworkManager.Singleton.LocalClientId);

        // Toggle the ready status
        bool newReadyStatus = !myPlayerCard.IsReady;
        myPlayerCard.SetPlayerReadyStatusRpc(newReadyStatus);

        // Update the button text based on the new status
        TMP_Text readyUpButtonText = this.readyUpButton.GetComponentInChildren<TMP_Text>();
        if (newReadyStatus) {
            readyUpButtonText.text = "Un Ready";
        } else {
            readyUpButtonText.text = "Ready Up";
        }
        Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} ready status: {myPlayerCard.IsReady}");

        await System.Threading.Tasks.Task.Delay(1000); // Small delay to ensure people not spaming the button.

        this.readyUpButton.interactable = true;

        CheckIfAlLPlayersAreReadyRpc();
    }

    private void OnStartGameButton() {
        if (IsServer) {
            GameManager.Instance.LoadStartGame();
        }
    }

    private void HandleClientConnected((string name, int playerIndex) playerInfo) {
        this.lobbyCodeText.text = $"Lobby Code: {GameManager.Instance.lobbyCode}, Players {NetworkManager.Singleton.ConnectedClientsList.Count}/4";
        AddPlayerCardRpc(name: playerInfo.name, playerIndex: playerInfo.playerIndex);

        // All players are not ready when a new player joins
        if (IsServer) {
            this.startGameButton.interactable = false;
        }
    }

    [Rpc(SendTo.Server)]
    public void AddPlayerCardRpc(string name, int playerIndex) {

        GameObject playerCardUI = Instantiate(this.playerLobbyCardUIPrefab.gameObject);

        // Set the player name on the card
        LobbyPlayerCard card = playerCardUI.GetComponent<LobbyPlayerCard>();

        // Gets the network object and sets the owner to the player who instantiated it
        playerCardUI.GetComponent<NetworkObject>().Spawn(true);
        playerCardUI.transform.SetParent(this.playerCardParentTransform);

        card.SetPlayerUsernameRpc(string.IsNullOrEmpty(name) ? $"Player {NetworkManager.Singleton.ConnectedClientsList.Count}" : name);

    }

    private LobbyPlayerCard GetLobbyPlayerCard(int clientId) {
        return this.playerCardParentTransform.GetChild(clientId).GetComponent<LobbyPlayerCard>();
    }

    [Rpc(SendTo.Server)]
    private void CheckIfAlLPlayersAreReadyRpc() {
        if (IsAllPlayersReady()) {
            this.startGameButton.interactable = true;
        } else {
            this.startGameButton.interactable = false;
        }
    }

    private bool IsAllPlayersReady() {
        foreach (Transform child in this.playerCardParentTransform) {
            LobbyPlayerCard card = child.GetComponent<LobbyPlayerCard>();
            if (!card.IsReady) {
                return false;
            }
        }
        return true;
    }
}
