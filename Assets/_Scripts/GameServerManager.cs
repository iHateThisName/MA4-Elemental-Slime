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

    public PlayerStateAnimator.ElementalType GetElementalTypeByClientId(ulong collisionWithClientId) {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(collisionWithClientId, out var client)) {
            var playerObject = client.PlayerObject;
            if (playerObject != null) {
                var playerElementalType = playerObject.GetComponentInChildren<PlayerMovement2D>();
                if (playerElementalType != null) {
                    return playerElementalType.CurrentElementalType;
                }
            }
        }
        return PlayerStateAnimator.ElementalType.None;
    }

    //[Rpc(SendTo.Server)]
    //public void ApplyKnockBackRpc(Vector3 direction, ulong collisionWithClientId) {
    //    Debug.Log($"Applying knockback to player {collisionWithClientId} in direction {direction}");
    //    // Get the PlayerMovement2D component of the collided player
    //    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(collisionWithClientId, out var client)) {
    //        var playerObject = client.PlayerObject;
    //        var playerMovement = playerObject.GetComponentInChildren<PlayerMovement2D>();
    //        playerMovement.ApplyKnockbackRpc(direction);
    //    }
    //}
}


