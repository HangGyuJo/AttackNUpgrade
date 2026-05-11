using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private float baseMoveSpeed = 100f;
    [SerializeField] private int baseAttackDamage = 10;
    [SerializeField] private float baseAttackCooldown = 0.25f;
    [SerializeField] private float baseProjectileSpeed = 120f;

    public NetworkVariable<int> MaxHealth { get; } = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> CurrentHealth { get; } = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> MoveSpeed { get; } = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> AttackDamage { get; } = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> AttackCooldown { get; } = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> ProjectileSpeed { get; } = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsDead { get; } = new(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        ResetStatsServer();
    }

    public void ResetStatsServer()
    {
        if (!IsServer)
        {
            return;
        }

        MaxHealth.Value = baseMaxHealth;
        CurrentHealth.Value = baseMaxHealth;
        MoveSpeed.Value = baseMoveSpeed;
        AttackDamage.Value = baseAttackDamage;
        AttackCooldown.Value = baseAttackCooldown;
        ProjectileSpeed.Value = baseProjectileSpeed;
        IsDead.Value = false;
    }

    public void TakeDamageServer(int damage)
    {
        if (!IsServer)
        {
            return;
        }

        if (IsDead.Value)
        {
            return;
        }

        CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - Mathf.Max(damage, 0), 0);

        if (CurrentHealth.Value <= 0)
        {
            IsDead.Value = true;
        }
    }

    public void HealServer(int amount)
    {
        if (!IsServer)
        {
            return;
        }

        if (IsDead.Value)
        {
            return;
        }

        CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + Mathf.Max(amount, 0), MaxHealth.Value);
    }

    public void AddMaxHealthServer(int amount, bool healByAddedAmount = true)
    {
        if (!IsServer)
        {
            return;
        }

        int finalAmount = Mathf.Max(amount, 0);

        MaxHealth.Value += finalAmount;

        if (healByAddedAmount)
        {
            CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + finalAmount, MaxHealth.Value);
        }
    }

    public void AddAttackDamageServer(int amount)
    {
        if (!IsServer)
        {
            return;
        }

        AttackDamage.Value = Mathf.Max(0, AttackDamage.Value + amount);
    }

    public void AddMoveSpeedPercentServer(float percent)
    {
        if (!IsServer)
        {
            return;
        }

        float multiplier = 1f + percent;
        MoveSpeed.Value = Mathf.Max(0.1f, MoveSpeed.Value * multiplier);
    }

    public void AddAttackSpeedPercentServer(float percent)
    {
        if (!IsServer)
        {
            return;
        }

        float multiplier = 1f - percent;
        AttackCooldown.Value = Mathf.Max(0.05f, AttackCooldown.Value * multiplier);
    }

    public void AddProjectileSpeedPercentServer(float percent)
    {
        if (!IsServer)
        {
            return;
        }

        float multiplier = 1f + percent;
        ProjectileSpeed.Value = Mathf.Max(0.1f, ProjectileSpeed.Value * multiplier);
    }
}