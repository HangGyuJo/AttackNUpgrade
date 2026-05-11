using Unity.Netcode;
using UnityEngine;

public class PlayerRunProgress : NetworkBehaviour
{
    [SerializeField] private int expToLevelUp = 100;

    public NetworkVariable<int> RunCurrency { get; } = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> RunExp { get; } = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> RunLevel { get; } = new(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        ResetRunProgressServer();
    }

    public void ResetRunProgressServer()
    {
        if (!IsServer)
        {
            return;
        }

        RunCurrency.Value = 0;
        RunExp.Value = 0;
        RunLevel.Value = 1;
    }

    public void AddRewardServer(int currency, int exp)
    {
        if (!IsServer)
        {
            return;
        }

        RunCurrency.Value += Mathf.Max(currency, 0);
        AddExpServer(exp);
    }

    private void AddExpServer(int amount)
    {
        int finalAmount = Mathf.Max(amount, 0);

        if (finalAmount <= 0)
        {
            return;
        }

        RunExp.Value += finalAmount;

        while (RunExp.Value >= expToLevelUp)
        {
            RunExp.Value -= expToLevelUp;
            RunLevel.Value++;
        }
    }
}