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
