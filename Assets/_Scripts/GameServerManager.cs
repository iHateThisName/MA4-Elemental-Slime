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

    private void Start() {
        NetworkManager.Singleton.OnConnectionEvent += HandlePlayerConnected;
    }

    // Add a handler method with any name (not a Unity message) to handle the event
    private void HandlePlayerConnected(NetworkManager manager, ConnectionEventData data) {
        if (IsServer) {
            Debug.Log($"Player connected: {data.ClientId}");
        }
    }

    //private void OnPlayerDisconnected(UnityEngine.NetworkPlayer player) {

    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //public void 
}
