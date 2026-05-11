using Unity.Netcode;
using UnityEngine;

public class EnemyRewardSystem : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        Debug.Log("[EnemyRewardSystem] Spawned and subscribed.");

        EnemyHealth.ServerDied += OnEnemyDiedServer;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {
            return;
        }

        EnemyHealth.ServerDied -= OnEnemyDiedServer;
    }

    private void OnEnemyDiedServer(EnemyHealth enemy, ulong killerClientId)
    {
        if (enemy == null)
        {
            return;
        }

        if (killerClientId == ulong.MaxValue)
        {
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out NetworkClient client))
        {
            return;
        }

        if (client.PlayerObject == null)
        {
            return;
        }

        PlayerRunProgress progress = client.PlayerObject.GetComponent<PlayerRunProgress>();

        if (progress == null)
        {
            return;
        }

        EnemyReward reward = enemy.GetComponent<EnemyReward>();

        if (reward == null)
        {
            return;
        }

        progress.AddRewardServer(
            reward.CurrencyReward,
            reward.ExpReward
        );
    }
}