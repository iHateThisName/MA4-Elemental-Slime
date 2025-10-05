using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerCollider : NetworkBehaviour {
    public Action<bool> OnGroundCollision;
    public Action<ulong> OnPlayerCollision;

    private Collider2D[] colliders;
    private void Awake() {
        this.colliders = this.GetComponents<Collider2D>();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DisableCollidersRpc() {
        Debug.Log("Disabling player colliders");
        foreach (Collider2D collider in this.colliders) {
            collider.enabled = false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnableCollidersRpc() {
        Debug.Log("Enabling player colliders");
        foreach (Collider2D collider in this.colliders) {
            collider.enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            this.OnGroundCollision?.Invoke(true);
        } else if (collision.gameObject.CompareTag("Player")) {
            Debug.Log($"Collided with player {collision.gameObject.GetComponent<NetworkObject>().OwnerClientId}");
            this.OnGroundCollision?.Invoke(true);
            this.OnPlayerCollision?.Invoke(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            this.OnGroundCollision?.Invoke(false);
        }
    }
}
