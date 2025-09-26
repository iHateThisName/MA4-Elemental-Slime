using Unity.Netcode;
using UnityEngine;

public class PlayerMovement2D : NetworkBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    //[SerializeField] private float jumpForce = 12f;

    [Header("Refrences")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform playerModelTransform;
    [SerializeField] private PlayerCollider playerCollider;
    [SerializeField] private PlayerStateAnimator playerState;

    private float moveInput;
    private bool isGrounded;
    public override void OnNetworkSpawn() {
        // Disable physics on non-owners (they just follow the network interloop sync)
        //if (!IsOwner) {
        //    this.rb.bodyType = RigidbodyType2D.Kinematic;
        //    return;
        //}

        if (!IsOwner) return;
        // Subscribe to the PlayerCollider
        this.playerCollider.OnGroundCollision += SetGrounded;
    }

    void Update() {
        if (!IsOwner) return; // Only the owner can control this player

        this.moveInput = Input.GetAxis("Horizontal");

        // Flip sprite based on direction
        if (this.moveInput != 0) {
            if (this.moveInput > 0 && this.playerModelTransform.localScale.x < 0) {
                this.playerModelTransform.localScale = new Vector3(1, 1, 1);
                Debug.Log("Flipping Right");
            } else if (this.moveInput < 0 && this.playerModelTransform.localScale.x > 0) {
                this.playerModelTransform.localScale = new Vector3(-1, 1, 1);
                Debug.Log("Flipping Left");
            }
        }

        // Jump
        if ((Input.GetButtonDown("Jump") || Input.GetButton("Jump")) && isGrounded) {
            //this.animator.SetTrigger("JumpTrigger");
            this.playerState.SetState(PlayerStateAnimator.PlayerState.Jump);
        }

    }

    private void FixedUpdate() {
        if (!IsOwner) return;
        this.rb.linearVelocity = new Vector2(this.moveInput * this.moveSpeed, rb.linearVelocity.y);
        //MoveServerRpc(this.moveInput);
    }

    private void SetGrounded(bool grounded) => this.isGrounded = grounded;

    //[ServerRpc]
    //private void MoveServerRpc(float moveX, ServerRpcParams serverRpcParams = default) {
    //    ulong clientId = serverRpcParams.Receive.SenderClientId;

    //    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client)) {
    //        NetworkObject playerObject = client.PlayerObject;
    //        Rigidbody2D playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
    //        playerRigidbody.linearVelocity = new Vector2(moveX * this.moveSpeed, playerRigidbody.linearVelocity.y);
    //    } else {
    //        Debug.LogWarning($"Client with ID {clientId} not found.");
    //    }
    //}

}
