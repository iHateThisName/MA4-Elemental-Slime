using Unity.Netcode;
using UnityEngine;
using static PlayerStateAnimator;

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

    [Header("Element Switch Settings")]

    private NetworkVariable<ElementalType> currentElementalType = new NetworkVariable<ElementalType>(
    ElementalType.Fire,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
    public ElementalType CurrentElementalType => currentElementalType.Value;

    [SerializeField] private float elementSwitchCooldown = 0.5f;
    private float lastElementSwitchTime = 0f;


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
        }

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
    }

    private void FixedUpdate() {
        if (!IsOwner) return;
        this.rb.linearVelocity = new Vector2(this.moveInput * this.moveSpeed, rb.linearVelocity.y);
        //MoveServerRpc(this.moveInput);
    }

    private void SetGrounded(bool grounded) => this.isGrounded = grounded;

    private void OnPlayerCollision(ulong collisionWithClientId) {

        Debug.Log($"Player {OwnerClientId} collided with Player {collisionWithClientId}");
        ulong clientID = OwnerClientId;

        // get the elemental type of the other player
        ElementalType collidedElementalType = GameServerManager.Instance.GetElementalTypeByClientId(collisionWithClientId);

        if (GameManager.Instance.IsSuperEffectiveElement(attacker: this.currentElementalType.Value, defender: collidedElementalType)) {
            Debug.Log($"Player {clientID} ({this.currentElementalType.Value}) hit Player {collisionWithClientId} ({collidedElementalType}) with super effective attack!");

            var direction = (NetworkManager.Singleton.ConnectedClients[collisionWithClientId].PlayerObject.transform.position - this.playerTransform.position).normalized;
            // Apply knockback to the other player
            GameServerManager.Instance.ApplyKnockBackRpc(direction, collisionWithClientId);
        } else {

        }
    }

    [Rpc(SendTo.Owner)]
    public void ApplyKnockbackRpc(Vector2 direction) {
        this.playerState.SetState(PlayerState.KnockBack);
        //direction.y += 0.2f; // Add some vertical lift to the knockback
        //direction.x *= 1200f; // Scale the knockback force
        ////this.rb.linearVelocity += direction * moveSpeed * 10;

        //this.rb.AddForce(direction, ForceMode2D.Impulse);
        //Debug.Log($"Applying knockback to player {OwnerClientId} in direction {direction}");

        //// draw a debug line to show the direction of the knockback
        //Debug.DrawLine(this.playerTransform.position, this.playerTransform.position + (Vector3)direction * 5f, Color.red, 10f);
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
