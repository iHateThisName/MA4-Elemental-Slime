using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCamerea : NetworkBehaviour {

    [SerializeField] GameObject BackgroundSpritePrefab;
    [SerializeField] Camera playerCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        if (!IsOwner) {
            this.enabled = false;
            return;
        }
        NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;

    }

    public override void OnNetworkDespawn() {
        if (!IsOwner) return;
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= SceneManager_OnLoadComplete;
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode) {

        this.BackgroundSpritePrefab = GameObject.Find("BackgroundSpritePrefab");
        if (this.BackgroundSpritePrefab != null) {
            this.playerCamera.gameObject.SetActive(true);
            ParalaxEffect[] paralaxEffects = this.BackgroundSpritePrefab.transform.GetChild(0).GetComponentsInChildren<ParalaxEffect>();
            foreach (ParalaxEffect paralax in paralaxEffects) {
                paralax.cam = playerCamera.gameObject;
            }
        }
    }

    public void DisableCamera() {
        this.playerCamera.gameObject.SetActive(false);
    }
}
