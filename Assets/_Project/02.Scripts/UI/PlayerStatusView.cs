using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatusView : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;

    private PlayerStats localStats;

    private void Update()
    {
        if (localStats == null)
        {
            TryBindLocalPlayer();
        }

        if (hpText == null)
        {
            return;
        }

        if (localStats == null)
        {
            hpText.text = "HP: -";
            return;
        }

        hpText.text = localStats.IsDead.Value
            ? "HP: Dead"
            : $"HP: {localStats.CurrentHealth.Value} / {localStats.MaxHealth.Value}";
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

        localStats = playerObject.GetComponent<PlayerStats>();
    }
}