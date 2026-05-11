using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    public static event Action<EnemyHealth, ulong> ServerDied;

    [SerializeField] private int maxHealth = 30;

    public int MaxHealth => maxHealth;

    public NetworkVariable<int> CurrentHealth { get; } = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsDead { get; } = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private ulong lastAttackerClientId = ulong.MaxValue;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        CurrentHealth.Value = maxHealth;
        IsDead.Value = false;
        lastAttackerClientId = ulong.MaxValue;
    }

    public void TakeDamageServer(int damage, ulong attackerClientId = ulong.MaxValue)
    {
        if (!IsServer)
        {
            return;
        }

        if (IsDead.Value)
        {
            return;
        }

        int finalDamage = Mathf.Max(damage, 0);

        if (finalDamage <= 0)
        {
            return;
        }

        if (attackerClientId != ulong.MaxValue)
        {
            lastAttackerClientId = attackerClientId;
        }

        CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - finalDamage, 0);

        if (CurrentHealth.Value <= 0)
        {
            DieServer();
        }
    }

    private void DieServer()
    {
        if (!IsServer)
        {
            return;
        }

        if (IsDead.Value)
        {
            return;
        }

        IsDead.Value = true;

        Debug.Log($"[EnemyHealth] Enemy died. Last attacker: {lastAttackerClientId}");

        ServerDied?.Invoke(this, lastAttackerClientId);

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}