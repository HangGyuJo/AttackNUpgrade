using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyPlayerAttacker : NetworkBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        ReturnHome
    }

    [Header("Detection")]
    [SerializeField] private float detectionRange = 6f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 0.9f;
    [SerializeField] private float homeArriveDistance = 0.15f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackInterval = 1f;

    private Rigidbody2D rb;
    private EnemyHealth enemyHealth;

    private Vector2 homePosition;
    private EnemyState currentState = EnemyState.Idle;

    private float attackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        StopImmediately();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        homePosition = transform.position;
        currentState = EnemyState.Idle;
        attackTimer = 0f;

        StopImmediately();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            StopImmediately();
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        if (enemyHealth == null || enemyHealth.IsDead.Value)
        {
            StopImmediately();
            return;
        }

        PlayerStats target = FindNearestAlivePlayerInDetectionRange();

        if (target == null)
        {
            ReturnHome();
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = target.transform.position;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        if (distanceToTarget <= stopDistance)
        {
            currentState = EnemyState.Attack;
            StopImmediately();
            AttackTarget(target);
            return;
        }

        currentState = EnemyState.Chase;
        MoveTo(targetPosition, moveSpeed);
    }

    private PlayerStats FindNearestAlivePlayerInDetectionRange()
    {
        if (NetworkManager.Singleton == null)
        {
            return null;
        }

        PlayerStats nearestTarget = null;
        float nearestDistanceSqr = float.MaxValue;
        float detectionRangeSqr = detectionRange * detectionRange;

        Vector2 currentPosition = rb.position;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
            {
                continue;
            }

            PlayerStats stats = client.PlayerObject.GetComponent<PlayerStats>();

            if (stats == null)
            {
                continue;
            }

            if (stats.IsDead.Value)
            {
                continue;
            }

            Vector2 playerPosition = stats.transform.position;
            float distanceSqr = (playerPosition - currentPosition).sqrMagnitude;

            if (distanceSqr > detectionRangeSqr)
            {
                continue;
            }

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestTarget = stats;
            }
        }

        return nearestTarget;
    }

    private void ReturnHome()
    {
        Vector2 currentPosition = rb.position;
        float distanceToHome = Vector2.Distance(currentPosition, homePosition);

        if (distanceToHome <= homeArriveDistance)
        {
            currentState = EnemyState.Idle;
            StopImmediately();
            return;
        }

        currentState = EnemyState.ReturnHome;
        MoveTo(homePosition, moveSpeed);
    }

    private void MoveTo(Vector2 targetPosition, float speed)
    {
        Vector2 direction = targetPosition - rb.position;

        if (direction.sqrMagnitude <= 0.000001f)
        {
            StopImmediately();
            return;
        }

        direction.Normalize();

        rb.velocity = direction * speed;
        rb.angularVelocity = 0f;
    }

    private void AttackTarget(PlayerStats target)
    {
        if (target == null || target.IsDead.Value)
        {
            ReturnHome();
            return;
        }

        attackTimer -= Time.fixedDeltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        attackTimer = attackInterval;
        target.TakeDamageServer(attackDamage);
    }

    private void StopImmediately()
    {
        if (rb == null)
        {
            return;
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? homePosition : (Vector2)transform.position, homeArriveDistance);
    }
#endif
}