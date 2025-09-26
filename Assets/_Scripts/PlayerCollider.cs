using System;
using UnityEngine;

public class PlayerCollider : MonoBehaviour {
    public Action<bool> OnGroundCollision;
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            this.OnGroundCollision?.Invoke(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            this.OnGroundCollision?.Invoke(false);
        }
    }
}
