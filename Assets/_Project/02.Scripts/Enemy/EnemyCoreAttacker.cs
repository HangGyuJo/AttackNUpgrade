using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyCoreAttacker : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 1.2f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackInterval = 1f;

    private Rigidbody2D rb;
    private EnemyHealth enemyHealth;
    private float attackTimer;

    private bool isCoreTouched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;


        isCoreTouched = false;
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        if (enemyHealth.IsDead.Value)
        {
            return;
        }

        CoreHealth core = CoreHealth.Instance;

        if (core == null || core.IsDestroyed.Value)
        {
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 corePosition = core.transform.position;
        float distance = Vector2.Distance(currentPosition, corePosition);

        if (!isCoreTouched)
        {
            MoveToCore(currentPosition, corePosition);
        }
        else
        {
            AttackCore();
        }
    }

    private void MoveToCore(Vector2 currentPosition, Vector2 corePosition)
    {
        if (isCoreTouched) return;

        Vector2 direction = (corePosition - currentPosition).normalized;
        Vector2 nextPosition = currentPosition + direction * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }

    private void AttackCore()
    {
        attackTimer -= Time.fixedDeltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        attackTimer = attackInterval;

        CoreHealth core = CoreHealth.Instance;

        if (core != null)
        {
            core.TakeDamageServer(attackDamage);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Core"))
        {
            isCoreTouched = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Core"))
        {
            isCoreTouched = false;
        }
    }
}