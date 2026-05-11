using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerCollisionController : NetworkBehaviour
{
    [SerializeField] private Collider2D[] collidersToDisableOnDeath;

    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (collidersToDisableOnDeath == null || collidersToDisableOnDeath.Length == 0)
        {
            collidersToDisableOnDeath = GetComponentsInChildren<Collider2D>();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (stats == null)
        {
            return;
        }

        stats.IsDead.OnValueChanged += OnDeadChanged;

        ApplyCollisionState(stats.IsDead.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (stats == null)
        {
            return;
        }

        stats.IsDead.OnValueChanged -= OnDeadChanged;
    }

    private void OnDeadChanged(bool previousValue, bool newValue)
    {
        ApplyCollisionState(newValue);
    }

    private void ApplyCollisionState(bool isDead)
    {
        foreach (Collider2D targetCollider in collidersToDisableOnDeath)
        {
            if (targetCollider == null)
            {
                continue;
            }

            if (targetCollider.isTrigger)
            {
                continue;
            }

            targetCollider.enabled = !isDead;
        }
    }
}