using System;
using Unity.Netcode;
using UnityEngine;

public class GameServerManager : NetworkBehaviour {

    #region Singleton
    public static GameServerManager Instance { get; private set; }
    private void Awake() {
        // Singleton pattern implementation
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    #endregion

    public Action<(string name, int playerIndex)> OnPlayerConnected;
    private void Start() {
        //NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnectedRpc;
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnectedRpc;
        }
    }

    [Rpc(SendTo.Server)]
    private void HandleClientConnectedRpc(ulong obj) {

        (string playerName, int index) playerInfo = (playerName: String.Empty, index: NetworkManager.Singleton.ConnectedClientsList.Count);
        this.OnPlayerConnected?.Invoke((playerInfo.playerName, playerInfo.index));
    }
}
