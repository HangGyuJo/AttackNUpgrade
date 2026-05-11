using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerBossInvasion = 3;

    public NetworkVariable<int> ActiveEnemyCount { get; } = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private int lastSpawnedRound = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        GamePhaseManager phaseManager = GamePhaseManager.Instance;

        if (phaseManager == null)
        {
            return;
        }

        if (phaseManager.CurrentPhase.Value != GamePhase.BossInvasion)
        {
            return;
        }

        int currentRound = phaseManager.CurrentRound.Value;

        if (lastSpawnedRound == currentRound)
        {
            return;
        }

        lastSpawnedRound = currentRound;
        SpawnWave();
    }

    private void SpawnWave()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] Enemy Prefab is not assigned.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] Spawn Points are not assigned.");
            return;
        }

        ActiveEnemyCount.Value = 0;

        for (int i = 0; i < enemiesPerBossInvasion; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];

            NetworkObject enemyInstance = Instantiate(
                enemyPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

            enemyInstance.Spawn(true);
            ActiveEnemyCount.Value++;
        }

        Debug.Log($"[EnemySpawner] Spawned {enemiesPerBossInvasion} enemies.");
    }

    public void NotifyEnemyKilled(EnemyHealth enemy)
    {
        if (!IsServer)
        {
            return;
        }

        ActiveEnemyCount.Value = Mathf.Max(ActiveEnemyCount.Value - 1, 0);

        Debug.Log($"[EnemySpawner] Enemy killed. Remaining: {ActiveEnemyCount.Value}");

        GamePhaseManager phaseManager = GamePhaseManager.Instance;

        if (phaseManager == null)
        {
            return;
        }

        if (phaseManager.CurrentPhase.Value != GamePhase.BossInvasion)
        {
            return;
        }

        if (ActiveEnemyCount.Value <= 0)
        {
            phaseManager.CompleteBossInvasion();
        }
    }
}