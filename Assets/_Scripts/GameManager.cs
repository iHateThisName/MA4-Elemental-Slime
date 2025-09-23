using System;
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

    //private void HandlePlayerConnected(NetworkManager manager, ConnectionEventData data) {
    //    (string playerName, int index) playerInfo = (playerName: $"Player {data.ClientId}", index: (int)data.ClientId);
    //    this.OnPlayerConnected?.Invoke((playerInfo.playerName, playerInfo.index));
    //}
}
