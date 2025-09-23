using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    public static GameManager Instance { get; private set; }
    public string lobbyCode;

    //public Action<(string name, int playerIndex)> OnPlayerConnected;
    private void Awake() {
        // Singleton pattern implementation
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start() {
        //NetworkManager.Singleton.OnConnectionEvent += HandlePlayerConnected;
    }

    //[Rpc(SendTo.ClientsAndHost)] ?????
    public void LoadStartGameRpc() {
        if (!IsServer) return;

        // Load fade in animation scene here.
        //await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("FadeInAnimationScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);

        //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(EnumScenes.SampleScene.ToString());

        NetworkManager.Singleton.SceneManager.LoadScene(EnumScenes.SampleScene.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);

        // When fade in animation is done unloade the fade in animation scene here.
        //await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("FadeInAnimationScene");
    }

    public async Task LoadLobby() {
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(EnumScenes.Lobby.ToString());
    }

    //private void HandlePlayerConnected(NetworkManager manager, ConnectionEventData data) {
    //    (string playerName, int index) playerInfo = (playerName: $"Player {data.ClientId}", index: (int)data.ClientId);
    //    this.OnPlayerConnected?.Invoke((playerInfo.playerName, playerInfo.index));
    //}
}
