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

    private int lastHeightPosition = 0;

    [Header("Element Switch Settings")]

    private NetworkVariable<ElementalType> currentElementalType = new NetworkVariable<ElementalType>(
    ElementalType.Fire,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
    public ElementalType CurrentElementalType => currentElementalType.Value;

    [SerializeField] private float elementSwitchCooldown = 0.5f;
    private float lastElementSwitchTime = 0f;
    private Vector2 knockback = new Vector2(0, 0);


    public override void OnNetworkSpawn() {
        // Disable physics on non-owners just follow the network interloop sync
        //if (!IsOwner) {
        //    this.rb.bodyType = RigidbodyType2D.Kinematic;
        //    return;
        //}

        if (!IsOwner) return;
        this.playerCollider.OnGroundCollision += SetGrounded;
        this.currentElementalType.OnValueChanged += OnElementalTypeChanged;
        this.playerCollider.OnPlayerCollision += OnPlayerCollision;
    }


    public override void OnNetworkDespawn() {
        if (!IsOwner) return;
        this.playerCollider.OnGroundCollision -= SetGrounded;
        this.currentElementalType.OnValueChanged -= OnElementalTypeChanged;
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
            //this.animator.SetTrigger("JumpTrigger");
            this.playerState.SetState(PlayerStateAnimator.PlayerState.Jump);
            this.rb.linearVelocityY = this.jumpForce;
        }

        // Falling detection
        this.isFaling = !this.isGrounded && this.rb.linearVelocity.y < 0;
        Debug.Log($"isFaling: {this.isFaling}, isGrounded: {this.isGrounded}, verticalVelocity: {this.rb.linearVelocity.y}");

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
            ApplyKnockback(direction);
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

            var direction = (NetworkManager.Singleton.ConnectedClients[collisionWithClientId].PlayerObject.transform.position - this.playerTransform.position).normalized;
            // Apply knockback to the other player
            //GameServerManager.Instance.ApplyKnockBackRpc(direction, collisionWithClientId);
            ApplyKnockback(direction);
        }
    }

    public void ApplyKnockback(Vector2 direction) {
        Debug.Log($"Applying knockback to player {OwnerClientId} in direction {direction}, {direction * 20f}");
        //this.playerState.SetState(PlayerState.KnockBack);
        //direction.y += 0.2f; // Add some vertical lift to the knockback
        //direction.x *= 1200f; // Scale the knockback force
        this.rb.linearVelocity += direction * 20f;


        //this.rb.AddForce(direction, ForceMode2D.Impulse);
        //Debug.Log($"Applying knockback to player {OwnerClientId} in direction {direction}");
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
