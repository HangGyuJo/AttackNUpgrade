using Unity.Netcode;
using UnityEngine;

public class CoreHealth : NetworkBehaviour
{
    public static CoreHealth Instance { get; private set; }

    [SerializeField] private int maxHealth = 100;

    public int MaxHealth => maxHealth;

    public NetworkVariable<int> CurrentHealth { get; } = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsDestroyed { get; } = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
            IsDestroyed.Value = false;
        }

        CurrentHealth.OnValueChanged += OnHealthChanged;
        IsDestroyed.OnValueChanged += OnDestroyedChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= OnHealthChanged;
        IsDestroyed.OnValueChanged -= OnDestroyedChanged;
    }

    public void TakeDamageServer(int damage)
    {
        if (!IsServer)
        {
            return;
        }

        if (IsDestroyed.Value)
        {
            return;
        }

        int finalDamage = Mathf.Max(damage, 0);
        CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - finalDamage, 0);

        if (CurrentHealth.Value <= 0)
        {
            DestroyCoreServer();
        }
    }

    private void DestroyCoreServer()
    {
        if (IsDestroyed.Value)
        {
            return;
        }

        IsDestroyed.Value = true;

        if (NetworkGamePhaseManager.Instance != null)
        {
            NetworkGamePhaseManager.Instance.EndRun(false);
        }
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        Debug.Log($"[Core] HP changed: {previousValue} ¡æ {newValue}");
    }

    private void OnDestroyedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log("[Core] Destroyed");
        }
    }
}