using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {


    [SerializeField] private Button joinGameButton;
    [SerializeField] private TMP_InputField lobbyCodeInputField;

    [SerializeField] private RelayManager relayManager;
    private void Start() {
        this.joinGameButton.interactable = false;

        // Enable the join game button only if the lobby code input field has exactly 6 characters
        this.lobbyCodeInputField.onValueChanged.AddListener((value) => {
            this.joinGameButton.interactable = value.Length == 6;
        });

        // Add listener to the join game button
        this.joinGameButton.onClick.AddListener(() => {
            this.joinGameButton.interactable = false;
            this.PlayerJoinGame(this.lobbyCodeInputField.text);
        });
    }
    public async void OnHostGameButton(Button button) {
        button.interactable = false;
        await this.relayManager.CreateRelay();
        await GameManager.Instance.LoadLobby();
        if (!NetworkManager.Singleton.StartHost()) {
            this.joinGameButton.interactable = true;
        }
    }

    public async void PlayerJoinGame(string lobbyCode) {
        Debug.Log($"Player is trying to join game with lobby code: {lobbyCode}..");
        await this.relayManager.JoinRelay(lobbyCode);
        NetworkManager.Singleton.StartClient();
    }

    public void OnQuitButton() {
#if UNITY_EDITOR
        // Stop play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the application
        Application.Quit();
#endif
    }

}
