using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkLauncher : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "InGame";

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 1200, 860));

        if (NetworkManager.Singleton == null)
        {
            GUILayout.Label("NetworkManager not found.");
            GUILayout.EndArea();
            return;
        }

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host", GUILayout.Height(400)))
            {
                StartHost();
            }

            if (GUILayout.Button("Start Client", GUILayout.Height(400)))
            {
                StartClient();
            }
        }
        else
        {
            string mode = NetworkManager.Singleton.IsHost
                ? "Host"
                : NetworkManager.Singleton.IsServer
                    ? "Server"
                    : "Client";

            GUILayout.Label($"Connected Mode: {mode}");

            if (NetworkManager.Singleton.IsHost)
            {
                GUILayout.Label("Host controls scene loading.");
            }
        }

        GUILayout.EndArea();
    }

    private void StartHost()
    {
        bool success = NetworkManager.Singleton.StartHost();

        if (success)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                gameSceneName,
                LoadSceneMode.Single
            );
        }
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}