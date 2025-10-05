using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour {
    public static GameManager Instance { get; private set; }
    public string lobbyCode;

    [SerializeField] private Vector3[] spawnPoints;

    private int playersAlive, playerAmount;
    public Action OnResetGameState;

    private void Awake() {
        // Singleton pattern implementation
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnSceneLoadCompleteRpc(ulong clientId, string sceneName, LoadSceneMode mode) {
        if (sceneName != EnumScenes.Arena5TheBigOne.ToString()) return;

        Debug.Log($"Client {clientId} finished loading {sceneName}");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)) {
            var playerObject = client.PlayerObject;
            Vector3 startPos = spawnPoints[clientId];
            playerObject.transform.position = startPos;
            //playerObject.GetComponent<AnticipatedNetworkTransform>().AnticipateMove(startPos);
            //playerObject.GetComponent<NetworkTransform>().Teleport(startPos, Quaternion.identity, Vector3.one);
            //TeleportPlayerRpc(clientId, startPos);
            Debug.Log($"Placed player {clientId} at {startPos}");

            playerObject.GetComponentInChildren<PlayerMovement2D>().EnableMovement();
        }

        this.playerAmount++;
        this.playersAlive++;
    }
    [Rpc(SendTo.Authority)]
    public void TeleportPlayerRpc(ulong clientId, Vector3 newPosition) {
        NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client);
        NetworkObject playerObject = client.PlayerObject;
        NetworkTransform networkTransform = playerObject.GetComponent<NetworkTransform>();
        networkTransform.Teleport(newPosition, Quaternion.identity, Vector3.one);
    }

    public void LoadStartGame() {
        if (!IsServer) return;
        Debug.Log("Loading Start Game Scene");
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadCompleteRpc;

        // Load fade in animation scene here.
        //await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("FadeInAnimationScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);

        //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(EnumScenes.SampleScene.ToString());

        NetworkManager.Singleton.SceneManager.LoadScene(EnumScenes.Arena5TheBigOne.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);



        // When fade in animation is done unloade the fade in animation scene here.
        //await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("FadeInAnimationScene");
    }

    public async Task LoadLobby() {
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(EnumScenes.Lobby.ToString());
    }

    public void LoadScene(EnumScenes loadScene) {
        NetworkManager.Singleton.SceneManager.LoadScene(loadScene.ToString(), LoadSceneMode.Single);
    }



    public bool IsSuperEffectiveElement(PlayerStateAnimator.ElementalType attacker, PlayerStateAnimator.ElementalType defender) {
        if (attacker == PlayerStateAnimator.ElementalType.Fire && defender == PlayerStateAnimator.ElementalType.Grass) {
            return true;
        } else if (attacker == PlayerStateAnimator.ElementalType.Water && defender == PlayerStateAnimator.ElementalType.Fire) {
            return true;
        } else if (attacker == PlayerStateAnimator.ElementalType.Grass && defender == PlayerStateAnimator.ElementalType.Water) {
            return true;
        } else {
            return false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayerKilledRpc() {
        this.playersAlive--;
        if (this.playersAlive == 1 && IsServer) {
            ResetGameStateRpc();
            LoadScene(EnumScenes.Arena5TheBigOne);
        }
    }

    [Rpc(SendTo.Owner)]
    private void ResetGameStateRpc() {
        Debug.Log("Resetting game state");
        OnResetGameState?.Invoke();
    }

    //private void HandlePlayerConnected(NetworkManager manager, ConnectionEventData data) {
    //    (string playerName, int index) playerInfo = (playerName: $"Player {data.ClientId}", index: (int)data.ClientId);
    //    this.OnPlayerConnected?.Invoke((playerInfo.playerName, playerInfo.index));
    //}
}
