using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : NetworkBehaviour {

    [SerializeField] private Button readyUpButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text lobbyCodeText;

    private void Awake() {
        if (!IsServer) {
            startGameButton.gameObject.SetActive(false);
        }
    }
    private void Start() {
        lobbyCodeText.text = $"Lobby Code: {GameManager.Instance.lobbyCode}, Players {NetworkManager.Singleton.ConnectedClientsList.Count}/4";
    }
}
