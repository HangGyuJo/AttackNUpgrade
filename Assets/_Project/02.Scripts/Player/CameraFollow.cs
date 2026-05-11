using Unity.Netcode;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.12f;

    private Transform target;
    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            TryBindLocalPlayer();
            return;
        }

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private void TryBindLocalPlayer()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (!NetworkManager.Singleton.IsClient)
        {
            return;
        }

        NetworkObject localPlayerObject =
            NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

        if (localPlayerObject == null)
        {
            return;
        }

        target = localPlayerObject.transform;
    }
}