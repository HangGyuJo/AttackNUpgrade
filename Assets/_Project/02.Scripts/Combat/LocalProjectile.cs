using Unity.Netcode;
using UnityEngine;

public class LocalProjectileVisual : MonoBehaviour
{
    private PlayerAttack ownerAttack;
    private uint shotId;

    private Vector2 direction;
    private float speed;
    private float lifeTime;
    private float elapsedTime;

    private bool canReportHit;
    private bool initialized;
    private bool hasReportedHit;

    public void Initialize(
        PlayerAttack ownerAttack,
        uint shotId,
        Vector2 direction,
        float speed,
        float lifeTime,
        bool canReportHit)
    {
        this.ownerAttack = ownerAttack;
        this.shotId = shotId;
        this.direction = direction.normalized;
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.canReportHit = canReportHit;

        elapsedTime = 0f;
        initialized = true;
        hasReportedHit = false;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
        {
            return;
        }

        if (!canReportHit)
        {
            return;
        }

        if (hasReportedHit)
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
        {
            return;
        }

        NetworkObject enemyNetworkObject = enemyHealth.GetComponent<NetworkObject>();

        if (enemyNetworkObject == null)
        {
            Debug.LogWarning("[LocalProjectile] Enemy has no NetworkObject.");
            return;
        }

        hasReportedHit = true;

        Debug.Log(
            $"[LocalProjectile] Hit detected. ShotId: {shotId}, " +
            $"EnemyObjectId: {enemyNetworkObject.NetworkObjectId}, " +
            $"HitPos: {transform.position}"
        );

        if (ownerAttack != null)
        {
            ownerAttack.ReportLocalProjectileHit(
                shotId,
                enemyNetworkObject,
                transform.position
            );
        }
        else
        {
            Debug.LogWarning("[LocalProjectile] OwnerAttack is null.");
        }

        Destroy(gameObject);
    }
}