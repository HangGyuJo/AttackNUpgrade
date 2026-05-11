using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RewardSelectionManager : NetworkBehaviour
{
    private class PlayerSelectionState
    {
        public int[] Choices;
        public bool HasSelected;
    }

    public static RewardSelectionManager Instance { get; private set; }

    [SerializeField] private AbilityData[] abilityPool;
    [SerializeField] private int choiceCount = 3;

    public event Action<int[]> ChoicesReceived;
    public event Action ChoiceConfirmed;
    public event Action SelectionClosed;

    private readonly Dictionary<ulong, PlayerSelectionState> selections = new();

    private void Awake()
    {
        Instance = this;
    }

    public AbilityData GetAbility(int index)
    {
        if (abilityPool == null)
        {
            return null;
        }

        if (index < 0 || index >= abilityPool.Length)
        {
            return null;
        }

        return abilityPool[index];
    }

    public void BeginSelectionServer()
    {
        if (!IsServer)
        {
            return;
        }

        if (abilityPool == null || abilityPool.Length < choiceCount)
        {
            Debug.LogError("[RewardSelection] Ability pool is too small.");
            return;
        }

        selections.Clear();

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            int[] choices = CreateRandomChoices();

            selections[clientId] = new PlayerSelectionState
            {
                Choices = choices,
                HasSelected = false
            };

            ClientRpcParams target = CreateTargetClientRpcParams(clientId);

            ShowChoicesClientRpc(
                choices[0],
                choices[1],
                choices[2],
                target
            );
        }

        Debug.Log("[RewardSelection] Selection started.");
    }

    public void SelectChoice(int slotIndex)
    {
        if (!IsClient)
        {
            return;
        }

        SubmitChoiceServerRpc(slotIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitChoiceServerRpc(
        int slotIndex,
        ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (!selections.TryGetValue(clientId, out PlayerSelectionState state))
        {
            return;
        }

        if (state.HasSelected)
        {
            return;
        }

        if (slotIndex < 0 || slotIndex >= state.Choices.Length)
        {
            return;
        }

        int abilityIndex = state.Choices[slotIndex];

        if (!ApplyAbilityToPlayerServer(clientId, abilityIndex))
        {
            return;
        }

        state.HasSelected = true;

        NotifyChoiceConfirmedClientRpc(
            CreateTargetClientRpcParams(clientId)
        );

        Debug.Log($"[RewardSelection] Client {clientId} selected ability index {abilityIndex}.");

        if (AreAllPlayersSelected())
        {
            CloseSelectionClientRpc();

            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.CompleteRewardSelection();
            }
        }
    }

    public void ForceSelectMissingServer()
    {
        if (!IsServer)
        {
            return;
        }

        foreach (KeyValuePair<ulong, PlayerSelectionState> pair in selections)
        {
            ulong clientId = pair.Key;
            PlayerSelectionState state = pair.Value;

            if (state.HasSelected)
            {
                continue;
            }

            if (state.Choices == null || state.Choices.Length == 0)
            {
                continue;
            }

            int abilityIndex = state.Choices[0];

            if (ApplyAbilityToPlayerServer(clientId, abilityIndex))
            {
                state.HasSelected = true;

                NotifyChoiceConfirmedClientRpc(
                    CreateTargetClientRpcParams(clientId)
                );
            }
        }

        CloseSelectionClientRpc();
    }

    private bool ApplyAbilityToPlayerServer(ulong clientId, int abilityIndex)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            return false;
        }

        NetworkObject playerObject = client.PlayerObject;

        if (playerObject == null)
        {
            return false;
        }

        PlayerStats stats = playerObject.GetComponent<PlayerStats>();

        if (stats == null)
        {
            return false;
        }

        AbilityData ability = GetAbility(abilityIndex);

        if (ability == null)
        {
            return false;
        }

        ability.ApplyToServer(stats);
        return true;
    }

    private bool AreAllPlayersSelected()
    {
        if (selections.Count == 0)
        {
            return false;
        }

        foreach (PlayerSelectionState state in selections.Values)
        {
            if (!state.HasSelected)
            {
                return false;
            }
        }

        return true;
    }

    private int[] CreateRandomChoices()
    {
        List<int> candidates = new();

        for (int i = 0; i < abilityPool.Length; i++)
        {
            candidates.Add(i);
        }

        int[] result = new int[choiceCount];

        for (int i = 0; i < choiceCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            result[i] = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);
        }

        return result;
    }

    private ClientRpcParams CreateTargetClientRpcParams(ulong clientId)
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };
    }

    [ClientRpc]
    private void ShowChoicesClientRpc(
        int first,
        int second,
        int third,
        ClientRpcParams clientRpcParams = default)
    {
        ChoicesReceived?.Invoke(new[] { first, second, third });
    }

    [ClientRpc]
    private void NotifyChoiceConfirmedClientRpc(
        ClientRpcParams clientRpcParams = default)
    {
        ChoiceConfirmed?.Invoke();
    }

    [ClientRpc]
    private void CloseSelectionClientRpc()
    {
        SelectionClosed?.Invoke();
    }
}