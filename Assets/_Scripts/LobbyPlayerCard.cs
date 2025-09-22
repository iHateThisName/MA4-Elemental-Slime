using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCard : MonoBehaviour {

    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image playerStatusImage;
    [SerializeField] private GameObject cardRoot;

    private void Awake() {
        this.cardRoot.SetActive(false);
    }
    void Start() {
        // Player is not ready up
        playerStatusImage.color = Color.red;
    }

    public void SetPlayerName(string name) {
        Debug.Log($"Setting player name to {name}");
        this.cardRoot.SetActive(true);
        playerNameText.text = name;
    }
}
