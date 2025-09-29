using System;
using Unity.Netcode;
using UnityEngine;
using static PlayerStateAnimator;

public class PlayerMovement2D : NetworkBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Refrences")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform playerModelTransform;
    [SerializeField] private PlayerCollider playerCollider;
    [SerializeField] private PlayerStateAnimator playerState;

    private float moveInput;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isFaling = false;

    [Header("Element Switch Settings")]
    private NetworkVariable<ElementalType> currentElementalType = new NetworkVariable<ElementalType>(
    ElementalType.Fire,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
    public ElementalType CurrentElementalType => currentElementalType.Value;

    [SerializeField] private float elementSwitchCooldown = 0.5f;
    private float lastElementSwitchTime = 0f;

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;
        this.playerCollider.OnGroundCollision += SetGrounded;
        this.currentElementalType.OnValueChanged += OnElementalTypeChanged;
        this.playerCollider.OnPlayerCollision += OnPlayerCollision;
    }

    public override void OnNetworkDespawn() {
        if (!IsOwner) return;
        this.playerCollider.OnGroundCollision -= SetGrounded;
        this.currentElementalType.OnValueChanged -= OnElementalTypeChanged;
        this.playerCollider.OnPlayerCollision -= OnPlayerCollision;
    }

    private void OnElementalTypeChanged(ElementalType previousValue, ElementalType newValue) => this.playerState.SetElementalTypeRpc(newValue);

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
            this.playerState.SetState(PlayerStateAnimator.PlayerState.Jump);
            this.rb.linearVelocityY = this.jumpForce;
        }

        // Falling detection
        this.isFaling = !this.isGrounded && this.rb.linearVelocity.y < 0;

        // Detect of q is pressed to change elemental type
        if (Input.GetKeyDown(KeyCode.Q) && Time.time - lastElementSwitchTime >= elementSwitchCooldown) {
            // Left Cycle to the next elemental type
            switch (this.currentElementalType.Value) {
                case ElementalType.Fire:
                    this.currentElementalType.Value = ElementalType.Grass;
                    break;
                case ElementalType.Water:
                    this.currentElementalType.Value = ElementalType.Fire;
                    break;
                case ElementalType.Grass:
                    this.currentElementalType.Value = ElementalType.Water;
                    break;
            }
            this.lastElementSwitchTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.E) && Time.time - lastElementSwitchTime >= elementSwitchCooldown) {
            // Right Cycle to the next elemental type
            switch (this.currentElementalType.Value) {
                case ElementalType.Fire:
                    this.currentElementalType.Value = ElementalType.Water;
                    break;
                case ElementalType.Water:
                    this.currentElementalType.Value = ElementalType.Grass;
                    break;
                case ElementalType.Grass:
                    this.currentElementalType.Value = ElementalType.Fire;
                    break;
            }
            this.lastElementSwitchTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            // Create a Vecotr2 direction from the player and behind the player
            var direction = -this.playerModelTransform.right + this.playerModelTransform.up * 0.9f;
            ApplyKnockbackRpc(direction);
        }

    }

    private void FixedUpdate() {
        if (!IsOwner) return; // Only the owner calculate physics for this player

        if (this.isFaling) {
            this.rb.linearVelocity += new Vector2(this.moveInput * this.moveSpeed, -7f);
        } else if (!this.isFaling && !this.isGrounded) {
            this.rb.linearVelocity += new Vector2(this.moveInput * this.moveSpeed, -3.5f);
        } else {
            this.rb.linearVelocity += new Vector2(this.moveInput * this.moveSpeed, 0);
        }
    }

    private void SetGrounded(bool grounded) => this.isGrounded = grounded;

    private void OnPlayerCollision(ulong collisionWithClientId) {

        Debug.Log($"Player {OwnerClientId} collided with Player {collisionWithClientId}");
        ulong clientID = OwnerClientId;

        // get the elemental type of the other player
        ElementalType collidedElementalType = GameServerManager.Instance.GetElementalTypeByClientId(collisionWithClientId);

        if (GameManager.Instance.IsSuperEffectiveElement(attacker: collidedElementalType, defender: this.currentElementalType.Value)) {
            Debug.Log($"Player {clientID} ({this.currentElementalType.Value}) hit Player {collisionWithClientId} ({collidedElementalType}) with super effective attack!");

            bool isAttackerRight = NetworkManager.Singleton.ConnectedClients[collisionWithClientId].PlayerObject.transform.position.x > this.playerTransform.position.x;
            Vector2 direction = this.playerModelTransform.up * 0.9f;
            Debug.Log($"Is Attacker Right: {isAttackerRight}");

            if (isAttackerRight) {
                direction = -this.playerModelTransform.right + this.playerModelTransform.up * 0.9f;
            } else {
                direction = this.playerModelTransform.right + this.playerModelTransform.up * 0.9f;
            }
            ApplyKnockback(direction);

        } else if (GameManager.Instance.IsSuperEffectiveElement(attacker: this.currentElementalType.Value, defender: collidedElementalType)) {
            Debug.Log($"Player {clientID} ({this.currentElementalType.Value}) hit Player {collisionWithClientId} ({collidedElementalType}) with super effective attack!");

            bool isAttackerRight = NetworkManager.Singleton.ConnectedClients[collisionWithClientId].PlayerObject.transform.position.x < this.playerTransform.position.x;
            Vector2 direction = this.playerModelTransform.up * 0.9f;

            Debug.Log($"Is Attacker Right: {isAttackerRight}");

            if (isAttackerRight) {
                direction = -this.playerModelTransform.right + this.playerModelTransform.up * 0.9f;
            } else {
                direction = this.playerModelTransform.right + this.playerModelTransform.up * 0.9f;
            }

            NetworkManager.Singleton.ConnectedClients[collisionWithClientId].PlayerObject.GetComponentInChildren<PlayerMovement2D>().ApplyKnockbackRpc(direction);
        }
    }

    [Rpc(SendTo.Owner)]
    public void ApplyKnockbackRpc(Vector2 direction) {
        Debug.Log($"Apply Knockback Rpc to player {OwnerClientId} in direction {direction}, {direction * 20f}");
        this.rb.linearVelocity += direction * 100f;
    }
    public void ApplyKnockback(Vector2 direction) {
        Debug.Log($"Applying knockback to player {OwnerClientId} in direction {direction}, {direction * 20f}");
        this.rb.linearVelocity += direction * 100f;
    }

    //[Rpc(SendTo.Owner)]
    public void EnableMovement() {
        this.rb.linearVelocity = Vector2.zero;
        this.rb.simulated = true;
    }

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
