using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerRunProgressView : MonoBehaviour
{
    [SerializeField] private TMP_Text progressText;

    private PlayerRunProgress localProgress;

    private void Update()
    {
        if (localProgress == null)
        {
            TryBindLocalPlayer();
        }

        if (progressText == null)
        {
            return;
        }

        if (localProgress == null)
        {
            progressText.text = "Run: -";
            return;
        }

        progressText.text =
            $"Lv. {localProgress.RunLevel.Value} | " +
            $"EXP {localProgress.RunExp.Value} | " +
            $"Gold {localProgress.RunCurrency.Value}";
    }

    private void TryBindLocalPlayer()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            return;
        }

        NetworkObject playerObject =
            NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

        if (playerObject == null)
        {
            return;
        }

        localProgress = playerObject.GetComponent<PlayerRunProgress>();
    }
}