using Unity.Netcode;
using UnityEngine;

public class NetworkGamePhaseManager : NetworkBehaviour
{
    public static NetworkGamePhaseManager Instance { get; private set; }

    [Header("Phase Durations")]
    [SerializeField] private float preparingDuration = 10f;
    [SerializeField] private float farmingDuration = 30f;
    [SerializeField] private float bossWarningDuration = 10f;
    [SerializeField] private float bossInvasionDuration = 30f;
    [SerializeField] private float rewardSelectionDuration = 15f;
    [SerializeField] private float intermissionDuration = 10f;

    [Header("Run Rule")]
    [SerializeField] private int maxRounds = 2;

    public NetworkVariable<GamePhase> CurrentPhase { get; } = new NetworkVariable<GamePhase>(
        GamePhase.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> RemainingTime { get; } = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> CurrentRound { get; } = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<GameResult> CurrentResult { get; } = new NetworkVariable<GameResult>(
        GameResult.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private float phaseTimer;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        CurrentRound.Value = 1;
        CurrentResult.Value = GameResult.None;
        SetPhase(GamePhase.Preparing, preparingDuration);
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (CurrentPhase.Value == GamePhase.None || CurrentPhase.Value == GamePhase.Result)
        {
            return;
        }

        phaseTimer -= Time.deltaTime;
        RemainingTime.Value = Mathf.Max(phaseTimer, 0f);

        if (phaseTimer <= 0f)
        {
            MoveToNextPhase();
        }
    }

    public void EndRun(bool victory)
    {
        if (!IsServer)
        {
            return;
        }

        CurrentResult.Value = victory ? GameResult.Victory : GameResult.Defeat;
        SetPhase(GamePhase.Result, 0f);
    }

    private void SetPhase(GamePhase nextPhase, float duration)
    {
        CurrentPhase.Value = nextPhase;
        phaseTimer = duration;
        RemainingTime.Value = duration;

        Debug.Log($"[GamePhase] Round {CurrentRound.Value} ¡æ {nextPhase}");
    }

    private void MoveToNextPhase()
    {
        switch (CurrentPhase.Value)
        {
            case GamePhase.Preparing:
                SetPhase(GamePhase.Farming, farmingDuration);
                break;

            case GamePhase.Farming:
                SetPhase(GamePhase.BossWarning, bossWarningDuration);
                break;

            case GamePhase.BossWarning:
                SetPhase(GamePhase.BossInvasion, bossInvasionDuration);
                break;

            case GamePhase.BossInvasion:
                SetPhase(GamePhase.RewardSelection, rewardSelectionDuration);
                break;

            case GamePhase.RewardSelection:
                SetPhase(GamePhase.Intermission, intermissionDuration);
                break;

            case GamePhase.Intermission:
                AdvanceRoundOrFinish();
                break;
        }
    }

    private void AdvanceRoundOrFinish()
    {
        if (CurrentRound.Value >= maxRounds)
        {
            EndRun(true);
            return;
        }

        CurrentRound.Value++;
        SetPhase(GamePhase.Farming, farmingDuration);
    }
}