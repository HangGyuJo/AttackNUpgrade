using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    private struct ServerShotRecord
    {
        public Vector2 Origin;
        public Vector2 Direction;
        public float FireTime;
        public float ExpireTime;
        public int Damage;
        public float ProjectileSpeed;
        public ulong AttackerClientId;
    }

    [Header("Projectile Visual")]
    [SerializeField] private GameObject projectileVisualPrefab;
    [SerializeField] private float projectileLifeTime = 2f;

    [Header("Attack")]
    [SerializeField] private float spawnOffset = 0.6f;

    [Header("Server Validation")]
    [SerializeField] private float originTolerance = 30f;
    [SerializeField] private float hitPathTolerance = 20f;
    [SerializeField] private float hitEnemyDistanceTolerance = 50f;

    private uint localShotSequence;
    private float nextLocalAttackTime;
    private float nextServerAttackTime;

    private readonly Dictionary<uint, ServerShotRecord> serverActiveShots = new();


    private PlayerStats stats;

    private float CurrentAttackCooldown =>
    stats != null ? stats.AttackCooldown.Value : 0.25f;

    private int CurrentAttackDamage =>
        stats != null ? stats.AttackDamage.Value : 10;

    private float CurrentProjectileSpeed =>
        stats != null ? stats.ProjectileSpeed.Value : 100f;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (IsServer)
        {
            CleanupExpiredServerShots();
        }

        if (!IsLocalPlayer)
        {
            return;
        }

        if (!Input.GetMouseButton(0))
        {
            return;
        }

        float attackCooldown = CurrentAttackCooldown;

        if (Time.time < nextLocalAttackTime)
        {
            return;
        }

        Vector2 direction = GetAimDirection();

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        //// 발사 요청 직전에 이동 위치 보고
        //if (playerMovement != null)
        //{
        //    playerMovement.ReportNow();
        //}

        nextLocalAttackTime = Time.time + attackCooldown;

        uint shotId = ++localShotSequence;
        Vector2 origin = (Vector2)transform.position + direction * spawnOffset;

        SpawnLocalProjectileVisual(
            shotId,
            origin,
            direction,
            CurrentProjectileSpeed,
            canReportHit: true
        );

        RequestShootServerRpc(shotId, origin, direction);
    }

    private Vector2 GetAimDirection()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return Vector2.right;
        }

        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;

        return ((Vector2)worldPosition - (Vector2)transform.position).normalized;
    }

    private void SpawnLocalProjectileVisual(
    uint shotId,
    Vector2 origin,
    Vector2 direction,
    float projectileSpeed,
    bool canReportHit)
    {
        if (projectileVisualPrefab == null)
        {
            Debug.LogError("[PlayerAttack] Projectile Visual Prefab is not assigned.");
            return;
        }

        GameObject projectileObject = Instantiate(
            projectileVisualPrefab,
            origin,
            Quaternion.identity
        );

        LocalProjectileVisual projectileVisual =
            projectileObject.GetComponent<LocalProjectileVisual>();

        if (projectileVisual == null)
        {
            Debug.LogError("[PlayerAttack] LocalProjectileVisual component is missing.");
            Destroy(projectileObject);
            return;
        }

        projectileVisual.Initialize(
            this,
            shotId,
            direction,
            projectileSpeed,
            projectileLifeTime,
            canReportHit
        );
    }

    [ServerRpc]
    private void RequestShootServerRpc(uint shotId, Vector2 requestedOrigin, Vector2 requestedDirection)
    {
        Debug.Log($"[Server] RequestShoot received. ShotId: {shotId}");

        float attackCooldown = CurrentAttackCooldown;

        if (Time.time < nextServerAttackTime)
        {
            Debug.LogWarning($"[Server] Shot rejected by cooldown. ShotId: {shotId}");
            return;
        }

        requestedDirection = requestedDirection.normalized;

        if (requestedDirection.sqrMagnitude <= 0.001f)
        {
            Debug.LogWarning($"[Server] Shot rejected by invalid direction. ShotId: {shotId}");
            return;
        }

        Vector2 expectedOrigin = (Vector2)transform.position + requestedDirection * spawnOffset;
        float originDistance = Vector2.Distance(expectedOrigin, requestedOrigin);

        if (originDistance > originTolerance)
        {
            Debug.LogWarning(
                $"[Server] Shot rejected by origin. ShotId: {shotId}, " +
                $"OriginDistance: {originDistance}, " +
                $"Expected: {expectedOrigin}, Requested: {requestedOrigin}"
            );
            return;
        }

        nextServerAttackTime = Time.time + attackCooldown;


        serverActiveShots[shotId] = new ServerShotRecord
        {
            Origin = requestedOrigin,
            Direction = requestedDirection,
            FireTime = Time.time,
            ExpireTime = Time.time + projectileLifeTime,
            Damage = CurrentAttackDamage,
            ProjectileSpeed = CurrentProjectileSpeed,
            AttackerClientId = OwnerClientId
        };

        Debug.Log($"[Server] Shot registered. ShotId: {shotId}");

        SpawnProjectileVisualClientRpc(
            OwnerClientId,
            shotId,
            requestedOrigin,
            requestedDirection,
            CurrentProjectileSpeed
        );
    }

    [ClientRpc]
    private void SpawnProjectileVisualClientRpc(
    ulong shooterClientId,
    uint shotId,
    Vector2 origin,
    Vector2 direction,
    float projectileSpeed)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (NetworkManager.Singleton.LocalClientId == shooterClientId)
        {
            return;
        }

        SpawnLocalProjectileVisual(
            shotId,
            origin,
            direction,
            projectileSpeed,
            canReportHit: true
        );
    }

    public void ReportLocalProjectileHit(
        uint shotId,
        NetworkObject enemyNetworkObject,
        Vector2 hitPosition)
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if (enemyNetworkObject == null)
        {
            return;
        }

        NetworkObjectReference enemyReference = enemyNetworkObject;

        ReportProjectileHitServerRpc(
            shotId,
            enemyReference,
            hitPosition
        );
    }

    [ServerRpc]
    private void ReportProjectileHitServerRpc(
       uint shotId,
       NetworkObjectReference enemyReference,
       Vector2 reportedHitPosition,
       ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[Server] Hit report received. ShotId: {shotId}");

        if (!serverActiveShots.TryGetValue(shotId, out ServerShotRecord shot))
        {
            Debug.LogWarning($"[Server] Hit rejected. Shot not found. ShotId: {shotId}");
            return;
        }

        if (Time.time > shot.ExpireTime)
        {
            Debug.LogWarning($"[Server] Hit rejected. Shot expired. ShotId: {shotId}");
            serverActiveShots.Remove(shotId);
            return;
        }

        if (!enemyReference.TryGet(out NetworkObject enemyObject))
        {
            Debug.LogWarning($"[Server] Hit rejected. Enemy reference invalid. ShotId: {shotId}");
            return;
        }

        EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            Debug.LogWarning($"[Server] Hit rejected. EnemyHealth missing. ShotId: {shotId}");
            return;
        }

        if (enemyHealth.IsDead.Value)
        {
            Debug.LogWarning($"[Server] Hit rejected. Enemy already dead. ShotId: {shotId}");
            return;
        }

        if (!ValidateHit(shot, enemyObject.transform.position, reportedHitPosition))
        {
            Debug.LogWarning(
                $"[Server] Hit rejected by validation. ShotId: {shotId}, " +
                $"EnemyPos: {enemyObject.transform.position}, ReportedHitPos: {reportedHitPosition}"
            );
            return;
        }

        serverActiveShots.Remove(shotId);

        enemyHealth.TakeDamageServer(shot.Damage, shot.AttackerClientId);

        ulong attackerClientId = rpcParams.Receive.SenderClientId;

        enemyHealth.TakeDamageServer(shot.Damage, attackerClientId);
    }

    private bool ValidateHit(
    ServerShotRecord shot,
    Vector2 enemyPosition,
    Vector2 reportedHitPosition)
    {
        float maxDistance = shot.ProjectileSpeed * projectileLifeTime;

        float distanceFromOrigin = Vector2.Distance(
            shot.Origin,
            reportedHitPosition
        );

        if (distanceFromOrigin > maxDistance + hitPathTolerance)
        {
            return false;
        }

        float distanceFromPath = DistancePointToSegment(
            reportedHitPosition,
            shot.Origin,
            shot.Origin + shot.Direction * maxDistance
        );

        if (distanceFromPath > hitPathTolerance)
        {
            return false;
        }

        return true;
    }

    private float DistancePointToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;

        float segmentLengthSquared = segment.sqrMagnitude;

        if (segmentLengthSquared <= 0.0001f)
        {
            return Vector2.Distance(point, segmentStart);
        }

        float t = Vector2.Dot(point - segmentStart, segment) / segmentLengthSquared;
        t = Mathf.Clamp01(t);

        Vector2 closestPoint = segmentStart + segment * t;

        return Vector2.Distance(point, closestPoint);
    }

    private void CleanupExpiredServerShots()
    {
        if (serverActiveShots.Count == 0)
        {
            return;
        }

        List<uint> expiredShotIds = null;

        foreach (KeyValuePair<uint, ServerShotRecord> pair in serverActiveShots)
        {
            if (Time.time <= pair.Value.ExpireTime)
            {
                continue;
            }

            expiredShotIds ??= new List<uint>();
            expiredShotIds.Add(pair.Key);
        }

        if (expiredShotIds == null)
        {
            return;
        }

        foreach (uint shotId in expiredShotIds)
        {
            serverActiveShots.Remove(shotId);
        }
    }
}