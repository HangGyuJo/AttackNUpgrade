using Unity.Netcode;
using UnityEngine;

public class PlayerInitializer : NetworkBehaviour
{
    [SerializeField]
    private Vector2[] spawnPositions =
    {
        new Vector2(-2f, 0f),
        new Vector2(2f, 0f),
        new Vector2(0f, 2f),
        new Vector2(0f, -2f)
    };

    /// <summary>
    /// 서버/호스트가 플레이어 스폰 위치만 정한다.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        int index = (int)(OwnerClientId % (ulong)spawnPositions.Length);
        transform.position = spawnPositions[index];
    }
}