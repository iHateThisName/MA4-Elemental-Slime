using System;
using Unity.Netcode;
using UnityEngine;
using static PlayerStateAnimator;

public class PlayerMovement2D : NetworkBehaviour {
    [Header("Movement Settings")]

    [Tooltip("The horizontal movement speed of the player. Default = 5")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("The upward force applied when the player jumps. Default = 100")]
    [SerializeField] private float jumpForce = 100f;

    [Tooltip("The force applied to the player when knocked back. Default = 100")]
    [SerializeField] private float knockbackForce = 100f;

    [Tooltip("The gravity strength applied when the player is falling. Default = 7")]
    [SerializeField] private float fallGravity = 7f;

    [Tooltip("The gravity strength applied when the player is rising in a jump. Default = 3.5")]
    [SerializeField] private float jumpGravity = 3.5f;

    [Header("Element Switch Settings")]

    [Tooltip("Cooldown time (in seconds) between switching elemental types. Default = 0.5")]
    [SerializeField] private float elementSwitchCooldown = 0.5f;
    public ElementalType CurrentElementalType => currentElementalType.Value;
    private float lastElementSwitchTime = 0f;
    private NetworkVariable<ElementalType> currentElementalType = new NetworkVariable<ElementalType>(value: ElementalType.Fire,
                                                                                                     readPerm: NetworkVariableReadPermission.Everyone,
                                                                                                     writePerm: NetworkVariableWritePermission.Owner);

    [Header("Refrences")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform playerModelTransform;
    [SerializeField] private PlayerCollider playerCollider;
    [SerializeField] private PlayerStateAnimator stateAnimator;
    [SerializeField] private PlayerCamerea playerCamera;

    private float moveInput;
    private bool isGrounded;
    private bool isFalling;
    private bool isKilled = false;
    private bool isDead = false;


    [Header("Debug")]
    [SerializeField] private bool enableLogging = false;

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;
        this.playerCollider.OnGroundCollision += SetGrounded;
        this.currentElementalType.OnValueChanged += OnElementalTypeChanged;
        this.playerCollider.OnPlayerCollision += OnPlayerCollision;
        GameManager.Instance.OnResetGameState += OnReset;
    }



    public override void OnNetworkDespawn() {
        if (!IsOwner) return;
        this.playerCollider.OnGroundCollision -= SetGrounded;
        this.currentElementalType.OnValueChanged -= OnElementalTypeChanged;
        this.playerCollider.OnPlayerCollision -= OnPlayerCollision;
        GameManager.Instance.OnResetGameState -= OnReset;
    }

    public async void OnReset() {
        if (!IsOwner) return;
        await System.Threading.Tasks.Task.Delay(1000);
        this.isDead = false;
        this.isKilled = false;
        this.rb.linearVelocity = Vector2.zero;
        this.rb.simulated = true;
        this.currentElementalType.Value = ElementalType.Fire;
        this.stateAnimator.SetState(PlayerState.Idle);
        this.playerCamera.EnableCamera();
        this.playerCollider.EnableCollidersRpc();
    }

    private void OnElementalTypeChanged(ElementalType previousValue, ElementalType newValue) => this.stateAnimator.SetElementalTypeRpc(newValue);

    void Update() {
        if (!IsOwner || this.isDead) return; // Only the owner can control this player

        if (this.isKilled && this.isGrounded) {
            this.isDead = true;
            this.rb.simulated = false;
            this.stateAnimator.SetState(PlayerStateAnimator.PlayerState.Dead);
            this.playerCamera.DisableCamera();
            this.playerCollider.DisableCollidersRpc();

            GameManager.Instance.PlayerKilledRpc();
        }

        this.moveInput = Input.GetAxis("Horizontal");

        // Flip sprite based on direction
        if (this.moveInput != 0) {
            if (this.moveInput > 0 && this.playerModelTransform.localScale.x < 0) {
                this.playerModelTransform.localScale = new Vector3(1, 1, 1);
                if (this.enableLogging) Debug.Log("Flipping Right");
            } else if (this.moveInput < 0 && this.playerModelTransform.localScale.x > 0) {
                this.playerModelTransform.localScale = new Vector3(-1, 1, 1);
                if (this.enableLogging) Debug.Log("Flipping Left");
            }
        }

        // Jump
        if ((Input.GetButtonDown("Jump") || Input.GetButton("Jump")) && isGrounded) {
            this.stateAnimator.SetState(PlayerStateAnimator.PlayerState.Jump);
            this.rb.linearVelocityY = this.jumpForce;
        }

        // Falling detection
        this.isFalling = !this.isGrounded && this.rb.linearVelocity.y < 0;

        // Detect of q is pressed to change elemental type
        if (Input.GetKeyDown(KeyCode.Q) && Time.time - this.lastElementSwitchTime >= this.elementSwitchCooldown) {
            // Left Cycle to the next elemental type
            switch (this.CurrentElementalType) {
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
            switch (this.CurrentElementalType) {
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

        //if (Input.GetKeyDown(KeyCode.R)) {
        //    // Create a Vecotr2 direction from the player and behind the player
        //    var direction = -this.playerModelTransform.right + this.playerModelTransform.up * 0.9f;
        //    ApplyKnockbackRpc(direction);
        //}

    }

    private void FixedUpdate() {
        if (!IsOwner || this.isDead) return; // Only the owner calculate physics for this player

        if (this.isFalling) {
            //this.rb.linearVelocity += new Vector2(this.moveInput * this.moveSpeed, -7f);
            this.rb.linearVelocity += new Vector2(this.moveInput * this.moveSpeed * 1.5f, this.fallGravity * -1);
        } else if (!this.isFalling && !this.isGrounded) {
            //this.rb.linearVelocity += new Vector2(this.moveInput * this.moveSpeed, -3.5f);
            this.rb.linearVelocity += new Vector2(this.moveInput * this.moveSpeed * 1.5f, this.jumpGravity * -1);
        } else {
            this.rb.linearVelocity += new Vector2(this.moveInput * this.moveSpeed * 1.5f, 0);
        }
    }

    private void SetGrounded(bool grounded) => this.isGrounded = grounded;

    private void OnPlayerCollision(ulong collisionWithClientId) {

        if (this.enableLogging) {
            Debug.Log($"Player {OwnerClientId} collided with Player {collisionWithClientId}");
        }
        ulong clientID = OwnerClientId;

        // get the elemental type of the other player
        ElementalType collidedElementalType = GameServerManager.Instance.GetElementalTypeByClientId(collisionWithClientId);

        if (GameManager.Instance.IsSuperEffectiveElement(attacker: collidedElementalType, defender: this.currentElementalType.Value)) {
            if (this.enableLogging) {
                Debug.Log($"Player {clientID} ({this.currentElementalType.Value}) hit Player {collisionWithClientId} ({collidedElementalType}) with super effective attack!");
            }

            bool isAttackerRight = NetworkManager.Singleton.ConnectedClients[collisionWithClientId].PlayerObject.transform.position.x > this.playerTransform.position.x;
            Vector2 direction = this.playerModelTransform.up * 0.9f;
            if (this.enableLogging) {
                Debug.Log($"Is Attacker Right: {isAttackerRight}");
            }

            if (isAttackerRight) {
                direction = -this.playerModelTransform.right + this.playerModelTransform.up * 0.9f;
            } else {
                direction = this.playerModelTransform.right + this.playerModelTransform.up * 0.9f;
            }
            ApplyKnockback(direction);

        } else if (GameManager.Instance.IsSuperEffectiveElement(attacker: this.currentElementalType.Value, defender: collidedElementalType)) {
            if (this.enableLogging) {
                Debug.Log($"Player {clientID} ({this.currentElementalType.Value}) hit Player {collisionWithClientId} ({collidedElementalType}) with super effective attack!");
            }

            bool isAttackerRight = NetworkManager.Singleton.ConnectedClients[collisionWithClientId].PlayerObject.transform.position.x < this.playerTransform.position.x;
            Vector2 direction = this.playerModelTransform.up * 0.9f;

            if (this.enableLogging) {
                Debug.Log($"Is Attacker Right: {isAttackerRight}");
            }

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
        if (this.enableLogging) {
            Debug.Log($"Apply Knockback Rpc to player {OwnerClientId} in direction {direction}, {direction * 20f}");
        }
        this.rb.linearVelocity += direction * this.knockbackForce;
        this.isKilled = true;
        this.stateAnimator.SetState(PlayerStateAnimator.PlayerState.KnockBack);
    }
    public void ApplyKnockback(Vector2 direction) {
        if (this.enableLogging) {
            Debug.Log($"Applying knockback to player {OwnerClientId} in direction {direction}, {direction * 20f}");
        }
        this.rb.linearVelocity += direction * this.knockbackForce;
        this.isKilled = true;
        this.stateAnimator.SetState(PlayerStateAnimator.PlayerState.KnockBack);

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
