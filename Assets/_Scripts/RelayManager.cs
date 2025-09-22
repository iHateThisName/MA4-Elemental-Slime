using System.Threading.Tasks;
using UnityEngine;

public class RelayManager : MonoBehaviour {

    [SerializeField] private int maxPlayers = 4;
    private bool isSignedIn = false;
    
    public async Task CreateRelay() {
        if (!isSignedIn) {
            await SignInAnonymously();
        }

        Unity.Services.Multiplayer.SessionOptions sessionOptions = new Unity.Services.Multiplayer.SessionOptions {
            MaxPlayers = this.maxPlayers,
            IsPrivate = true, // Private sessions are not visible in queries and cannot be joined with quick-join. They can still be joined by ID or by Code.
        };

        var allocation = await Unity.Services.Multiplayer.MultiplayerService.Instance.CreateSessionAsync(sessionOptions);

        Debug.Log("Allocation ID: " + allocation.Id);
        Debug.Log("Join Code: " + allocation.Code);

        GameManager.Instance.lobbyCode = allocation.Code;
    }

    public async Task JoinRelay(string joinCode) {
        if (!isSignedIn) {
            await SignInAnonymously();
        }

        try {
            var joinAllocation = await Unity.Services.Multiplayer.MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);
            Debug.Log("Joined Relay with Allocation ID: " + joinAllocation.Id);
        } catch (Unity.Services.Multiplayer.SessionException e) {
            Debug.LogError("Failed to join relay: " + e.Message);
        }
    }

    private async Task SignInAnonymously() {
        if (isSignedIn) return;

        // Initialize Unity Services
        await Unity.Services.Core.UnityServices.InitializeAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.LogError("Failed to initialize Unity Services: " + task.Exception);
                return;
            }

        });

        // Sign in anonymously
        await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync().ContinueWith(signInTask => {
            if (signInTask.IsFaulted) {
                Debug.LogError("Failed to sign in anonymously: " + signInTask.Exception);
            } else {
                Debug.Log("Signed in anonymously with Player ID: " + Unity.Services.Authentication.AuthenticationService.Instance.PlayerId);
                this.isSignedIn = true;
            }
        });
    }
}
