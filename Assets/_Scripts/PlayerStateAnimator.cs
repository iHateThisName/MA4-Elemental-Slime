using Unity.Netcode;
using UnityEngine;

public class PlayerStateAnimator : NetworkBehaviour {

    [SerializeField] private Animator animator;

    private NetworkVariable<PlayerState> currentState = new NetworkVariable<PlayerState>(
        PlayerState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public static readonly int Jump = Animator.StringToHash("Jump");
    public static readonly int Idle = Animator.StringToHash("Idle");




    public override void OnNetworkSpawn() {
        // Listen for state changes
        currentState.OnValueChanged += OnStateChanged;
    }

    public override void OnNetworkDespawn() {
        if (currentState != null)
            currentState.OnValueChanged -= OnStateChanged;
    }


    private void OnStateChanged(PlayerState previous, PlayerState next) {
        switch (next) {
            case PlayerState.Jump:
                animator.CrossFade(Jump, 0, 0);
                break;

            default:
                animator.CrossFade(Idle, 0, 0);
                break;
        }
    }

    public void SetState(PlayerState newState) {
        if (!IsOwner) return;
        if (currentState.Value == newState) return; // No change

        // Update network variable (will trigger OnValueChanged on all clients, including self)
        currentState.Value = newState;
    }

    public void OnJumpClipFinished() => SetState(PlayerState.Idle);



    public enum PlayerState : int {
        Idle, Jump
    }
}
