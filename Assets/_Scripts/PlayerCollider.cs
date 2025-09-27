using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerCollider : MonoBehaviour {
    public Action<bool> OnGroundCollision;
    public Action<ulong> OnPlayerCollision;
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            this.OnGroundCollision?.Invoke(true);
        } else if (collision.gameObject.CompareTag("Player")) {
            Debug.Log($"Collided with player {collision.gameObject.GetComponent<NetworkObject>().OwnerClientId}");
            this.OnPlayerCollision?.Invoke(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            this.OnGroundCollision?.Invoke(false);
        }
    }
}
