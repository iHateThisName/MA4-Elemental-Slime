using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class LobbyPlayerCard : NetworkBehaviour {

    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private UnityEngine.UI.Image playerStatusImage;
    [SerializeField] private GameObject cardRoot;

    //[SerializeField] private NetworkVariable<(string name, bool isReady)> playerInfo = new NetworkVariable<(string name, bool isReady)>();
    [SerializeField]
    private NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>(value: "You",
                                                                                                   readPerm: NetworkVariableReadPermission.Everyone,
                                                                                                   writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn() {
        this.username.OnValueChanged += OnPlayerUsernameChanged;
    }

    public override void OnNetworkDespawn() {
        this.username.OnValueChanged -= OnPlayerUsernameChanged;
    }
    private void OnPlayerUsernameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue) {
        playerNameText.text = newValue.ToString();
    }
    [Rpc(SendTo.Server)]
    public void SetPlayerUsernameRpc(string newUsername) {
        FixedString32Bytes fixedString = new FixedString32Bytes(newUsername);
        this.username.Value = fixedString;
    }

    //public override void OnNetworkSpawn() {
    //    this.playerInfo.OnValueChanged += OnPlayerInfoStateChanged;

    //    if (IsOwner) {
    //        SetPlayerInfo("You", false);
    //    }
    //}
    //public override void OnNetworkDespawn() {
    //    this.playerInfo.OnValueChanged -= OnPlayerInfoStateChanged;
    //}

    //private void OnPlayerInfoStateChanged((string name, bool isReady) oldValue, (string name, bool isReady) newValue) {
    //    playerNameText.text = newValue.name;
    //    playerStatusImage.color = newValue.isReady ? Color.green : Color.red;
    //}


    //public void SetPlayerInfo(string username, bool IsReady = false) {
    //    playerInfo.Value = (username, IsReady);
    //}

    //public void SetPlayerUsernameRpc(string newUsername) {
    //    playerInfo.Value = (newUsername, playerInfo.Value.isReady);
    //}
    //public void SetPlayerReadyStatus(bool isReady) {
    //    playerInfo.Value = (playerInfo.Value.name, isReady);
    //}
}
