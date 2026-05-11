using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FarmingEnemySpawner : NetworkBehaviour
{
    [Header("Spawn")]
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int spawnCountPerInterval = 2;
    [SerializeField] private int maxAliveEnemies = 12;

    [Header("Phase")]
    [SerializeField] private bool despawnEnemiesWhenFarmingEnds = true;

    private readonly HashSet<ulong> spawnedEnemyIds = new();

    private float spawnTimer;
    private GamePhase previousPhase = GamePhase.None;

    public NetworkVariable<int> AliveCount { get; } = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

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

        GamePhase currentPhase = phaseManager.CurrentPhase.Value;

        HandlePhaseChanged(currentPhase);

        if (currentPhase != GamePhase.Farming)
        {
            return;
        }

        TickSpawn();
    }

    private void HandlePhaseChanged(GamePhase currentPhase)
    {
        if (previousPhase == currentPhase)
        {
            return;
        }

        GamePhase oldPhase = previousPhase;
        previousPhase = currentPhase;

        if (oldPhase == GamePhase.Farming && currentPhase != GamePhase.Farming)
        {
            spawnTimer = 0f;

            if (despawnEnemiesWhenFarmingEnds)
            {
                DespawnAllSpawnedEnemiesServer();
            }
        }

        if (currentPhase == GamePhase.Farming)
        {
            spawnTimer = 0f;
        }
    }

    private void TickSpawn()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        if (AliveCount.Value >= maxAliveEnemies)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
        {
            return;
        }

        spawnTimer = spawnInterval;

        int availableSlots = maxAliveEnemies - AliveCount.Value;
        int spawnAmount = Mathf.Min(spawnCountPerInterval, availableSlots);

        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnEnemyServer();
        }
    }

    private void SpawnEnemyServer()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        NetworkObject enemyInstance = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        enemyInstance.Spawn(true);

        spawnedEnemyIds.Add(enemyInstance.NetworkObjectId);
        AliveCount.Value = spawnedEnemyIds.Count;
    }

    private void OnEnemyDiedServer(EnemyHealth enemy, ulong killerClientId)
    {
        if (enemy == null || enemy.NetworkObject == null)
        {
            return;
        }

        ulong enemyId = enemy.NetworkObject.NetworkObjectId;

        if (!spawnedEnemyIds.Remove(enemyId))
        {
            return;
        }

        AliveCount.Value = spawnedEnemyIds.Count;
    }

    private void DespawnAllSpawnedEnemiesServer()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        List<ulong> enemyIds = new(spawnedEnemyIds);

        foreach (ulong enemyId in enemyIds)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyId, out NetworkObject enemyObject))
            {
                continue;
            }

            if (enemyObject != null && enemyObject.IsSpawned)
            {
                enemyObject.Despawn(true);
            }
        }

        spawnedEnemyIds.Clear();
        AliveCount.Value = 0;
    }
}