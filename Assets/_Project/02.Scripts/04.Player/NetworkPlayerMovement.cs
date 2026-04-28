using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        input = Vector2.ClampMagnitude(input, 1f);

        SubmitMoveInputServerRpc(input);
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        Vector2 nextPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    [ServerRpc]
    private void SubmitMoveInputServerRpc(Vector2 input)
    {
        moveInput = Vector2.ClampMagnitude(input, 1f);
    }
}