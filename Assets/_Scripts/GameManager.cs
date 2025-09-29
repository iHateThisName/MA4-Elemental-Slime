using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour {
    public static GameManager Instance { get; private set; }
    public string lobbyCode;

    [SerializeField] private Vector3[] spawnPoints;

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
        if (sceneName != EnumScenes.SampleScene.ToString()) return;

        Debug.Log($"Client {clientId} finished loading {sceneName}");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)) {
            var playerObject = client.PlayerObject;
            if (playerObject != null) {
                Vector3 startPos = spawnPoints[clientId];
                playerObject.transform.position = startPos;
                Debug.Log($"Placed player {clientId} at {startPos}");
            }
        }
    }

    public void LoadStartGame() {
        if (!IsServer) return;
        Debug.Log("Loading Start Game Scene");
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadCompleteRpc;

        // Load fade in animation scene here.
        //await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("FadeInAnimationScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);

        //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(EnumScenes.SampleScene.ToString());

        NetworkManager.Singleton.SceneManager.LoadScene(EnumScenes.BasicArena1.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);

        // When fade in animation is done unloade the fade in animation scene here.
        //await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("FadeInAnimationScene");
    }

    public async Task LoadLobby() {
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(EnumScenes.Lobby.ToString());
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

    //private void HandlePlayerConnected(NetworkManager manager, ConnectionEventData data) {
    //    (string playerName, int index) playerInfo = (playerName: $"Player {data.ClientId}", index: (int)data.ClientId);
    //    this.OnPlayerConnected?.Invoke((playerInfo.playerName, playerInfo.index));
    //}
}
