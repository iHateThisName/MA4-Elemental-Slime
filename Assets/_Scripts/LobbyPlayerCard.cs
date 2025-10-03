using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayerCard : NetworkBehaviour {

    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private UnityEngine.UI.Image playerStatusImage;
    [SerializeField] private GameObject cardRoot;

    //[SerializeField] private NetworkVariable<(string name, bool isReady)> playerInfo = new NetworkVariable<(string name, bool isReady)>();
    [SerializeField]
    private NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>(value: "You",
                                                                                                   readPerm: NetworkVariableReadPermission.Everyone,
                                                                                                   writePerm: NetworkVariableWritePermission.Server);
    [SerializeField]
    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(value: false,
                                                                      readPerm: NetworkVariableReadPermission.Everyone,
                                                                      writePerm: NetworkVariableWritePermission.Server);

    public bool IsReady => this.isReady.Value;
    private void Start() {
        this.playerNameText.text = username.Value.ToString(); // In case the value was set before OnNetworkSpawn
        this.playerStatusImage.color = this.isReady.Value ? Color.green : Color.red;
    }
    public override void OnNetworkSpawn() {
        this.username.OnValueChanged += OnPlayerUsernameChanged;
        this.isReady.OnValueChanged += OnPlayerReadyStatusChanged;
    }
    public override void OnNetworkDespawn() {
        this.username.OnValueChanged -= OnPlayerUsernameChanged;
        this.isReady.OnValueChanged -= OnPlayerReadyStatusChanged;
    }

    private void OnPlayerReadyStatusChanged(bool previousValue, bool newValue) {
        this.playerStatusImage.color = newValue ? Color.green : Color.red;
    }

    private void OnPlayerUsernameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue) {
        playerNameText.text = newValue.ToString();
    }

    [Rpc(SendTo.Server)]
    public void SetPlayerUsernameRpc(string newUsername) {
        FixedString32Bytes fixedString = new FixedString32Bytes(newUsername);
        this.username.Value = fixedString;
    }

    [Rpc(SendTo.Server)]
    public void SetPlayerReadyStatusRpc(bool readyStatus) {
        Debug.Log($"SetPlayerReadyStatusRpc: Setting player {this.username.Value} ready status to {readyStatus}");
        this.isReady.Value = readyStatus;
    }
}
