using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CoreHealth))]
public class CoreDebugDamageTester : NetworkBehaviour
{
    [SerializeField] private int debugDamage = 10;
    [SerializeField] private KeyCode damageKey = KeyCode.K;

    private CoreHealth coreHealth;

    private void Awake()
    {
        coreHealth = GetComponent<CoreHealth>();
    }

    private void Update()
    {
        if (!IsClient)
        {
            return;
        }

        if (Input.GetKeyDown(damageKey))
        {
            RequestDebugDamageServerRpc(debugDamage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDebugDamageServerRpc(int damage)
    {
        coreHealth.TakeDamageServer(damage);
    }
}