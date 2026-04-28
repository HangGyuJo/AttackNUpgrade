using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerInitializer : NetworkBehaviour
{
    [SerializeField]
    private Vector2[] spawnPositions =
    {
        new Vector2(-2f, 0f),
        new Vector2(2f, 0f),
        new Vector2(0f, 2f),
        new Vector2(0f, -2f)
    };

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int index = (int)(OwnerClientId % (ulong)spawnPositions.Length);
            transform.position = spawnPositions[index];
        }

        if (IsOwner && CameraFollow.Instance != null)
        {
            CameraFollow.Instance.SetTarget(transform);
        }
    }
}