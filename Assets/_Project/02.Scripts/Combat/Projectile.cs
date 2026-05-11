using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifeTime;
    private int damage;

    private float elapsedTime;
    private bool initialized;
    private bool hasHit;

    public void SetupServer(Vector2 shootDirection, float projectileSpeed, float projectileLifeTime, int projectileDamage)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
        lifeTime = projectileLifeTime;
        damage = projectileDamage;

        elapsedTime = 0f;
        initialized = true;
        hasHit = false;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (!initialized)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= lifeTime)
        {
            DespawnServer();
            return;
        }

        Vector3 moveDelta = (Vector3)(direction * speed * Time.deltaTime);
        transform.position += moveDelta;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer)
        {
            return;
        }

        if (!initialized || hasHit)
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
        {
            return;
        }

        if (enemyHealth.IsDead.Value)
        {
            return;
        }

        hasHit = true;

        DespawnServer();
    }

    private void DespawnServer()
    {
        if (!IsServer)
        {
            return;
        }

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}