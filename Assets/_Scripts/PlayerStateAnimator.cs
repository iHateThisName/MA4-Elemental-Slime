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
    public static readonly int KnockBack = Animator.StringToHash("KnockBack");

    [Header("Sprite Rendere Refrences")]
    [SerializeField] private SpriteRenderer bodyRendere;
    [SerializeField] private SpriteRenderer eyesRendere;

    [Header("Sprites")]
    [Header("Fire")]
    [SerializeField] private Sprite fireBodySprite;
    [SerializeField] private Sprite fireEyesSprite;
    [Header("Water")]
    [SerializeField] private Sprite waterBodySprite;
    [SerializeField] private Sprite waterEyesSprite;
    [Header("Grass")]
    [SerializeField] private Sprite grassBodySprite;
    [SerializeField] private Sprite grassEyesSprite;


    public override void OnNetworkSpawn() {
        // Listen for state changes
        currentState.OnValueChanged += OnStateChanged;
    }

    public override void OnNetworkDespawn() {
        if (currentState != null)
            currentState.OnValueChanged -= OnStateChanged;
    }


    private void OnStateChanged(PlayerState previous, PlayerState next) {
        if (next == previous) return; // No change
        switch (next) {
            case PlayerState.Jump:
                this.animator.CrossFade(Jump, 0, 0);
                break;

            case PlayerState.KnockBack:
                this.animator.CrossFade(KnockBack, 0, 0);
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


    ///<summary>
    /// Sets the elemental type of the player and updates the body and eyes sprites accordingly.
    /// This method is called via RPC and will execute on all clients and the host.
    /// </summary>
    /// <param name="type">The elemental type to set (Fire, Water, Grass).</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void SetElementalTypeRpc(ElementalType type) {
        switch (type) {
            case ElementalType.Fire:
                bodyRendere.sprite = fireBodySprite;
                eyesRendere.sprite = fireEyesSprite;
                break;
            case ElementalType.Water:
                bodyRendere.sprite = waterBodySprite;
                eyesRendere.sprite = waterEyesSprite;
                break;
            case ElementalType.Grass:
                bodyRendere.sprite = grassBodySprite;
                eyesRendere.sprite = grassEyesSprite;
                break;
        }
    }



    public enum PlayerState : int {
        Idle, Jump, KnockBack
    }

    public enum ElementalType : int {
        None, Fire, Water, Grass
    }
}
