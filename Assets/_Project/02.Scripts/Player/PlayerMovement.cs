using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementEnableDelay = 0.2f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float skinWidth = 0.02f;
    [SerializeField] private int maxSlideIterations = 4;

    [Header("Collision Tuning")]
    [SerializeField] private float blockingNormalThreshold = -0.01f;

    private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private ContactFilter2D movementFilter;
    private PlayerStats stats;

    private Vector2 moveInput;
    private bool canMove;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        stats = GetComponent<PlayerStats>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.interpolation = RigidbodyInterpolation2D.None;

        movementFilter = new ContactFilter2D();
        movementFilter.useLayerMask = true;
        movementFilter.useTriggers = false;
        movementFilter.SetLayerMask(collisionMask);
    }

    public override void OnNetworkSpawn()
    {
        if (stats != null)
        {
            stats.IsDead.OnValueChanged += OnDeadChanged;
        }

        if (!IsLocalPlayer)
        {
            return;
        }

        StartCoroutine(EnableMovementAfterDelay());
    }

    public override void OnNetworkDespawn()
    {
        if (stats != null)
        {
            stats.IsDead.OnValueChanged -= OnDeadChanged;
        }
    }

    private IEnumerator EnableMovementAfterDelay()
    {
        canMove = false;
        StopLocalMovement();

        yield return new WaitForSeconds(movementEnableDelay);

        if (stats != null && stats.IsDead.Value)
        {
            canMove = false;
            StopLocalMovement();
            yield break;
        }

        canMove = true;
    }

    private void Update()
    {
        if (!IsLocalPlayer || !canMove)
        {
            StopLocalMovement();
            return;
        }

        if (stats != null && stats.IsDead.Value)
        {
            canMove = false;
            StopLocalMovement();
            return;
        }

        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        Move(Time.deltaTime);
    }

    private void OnDeadChanged(bool previousValue, bool newValue)
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if (!newValue)
        {
            return;
        }

        canMove = false;
        StopLocalMovement();
    }

    private void StopLocalMovement()
    {
        moveInput = Vector2.zero;

        if (rb == null)
        {
            return;
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void Move(float deltaTime)
    {
        float moveSpeed = stats != null ? stats.MoveSpeed.Value : 5f;
        Vector2 desiredDelta = moveInput * moveSpeed * deltaTime;

        if (desiredDelta.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        Vector2 resolvedDelta = ResolveMovementWithSlide(desiredDelta);

        if (resolvedDelta.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        transform.position += (Vector3)resolvedDelta;
    }

    private Vector2 ResolveMovementWithSlide(Vector2 desiredDelta)
    {
        Vector2 totalDelta = Vector2.zero;
        Vector2 remainingDelta = desiredDelta;

        for (int i = 0; i < maxSlideIterations; i++)
        {
            if (remainingDelta.sqrMagnitude <= 0.000001f)
            {
                break;
            }

            Vector2 direction = remainingDelta.normalized;
            float distance = remainingDelta.magnitude;

            bool hasHit = CastMovement(
                direction,
                distance,
                out RaycastHit2D nearestHit
            );

            if (!hasHit)
            {
                totalDelta += remainingDelta;
                break;
            }

            float moveDistance = Mathf.Max(nearestHit.distance - skinWidth, 0f);
            Vector2 moveDelta = direction * moveDistance;

            totalDelta += moveDelta;

            Vector2 leftoverDelta = remainingDelta - moveDelta;

            if (leftoverDelta.sqrMagnitude <= 0.000001f)
            {
                break;
            }

            Vector2 clippedDelta = ClipDeltaByNormal(leftoverDelta, nearestHit.normal);

            if (clippedDelta.sqrMagnitude <= 0.000001f)
            {
                break;
            }

            remainingDelta = clippedDelta;
        }

        return totalDelta;
    }

    private bool CastMovement(
        Vector2 direction,
        float distance,
        out RaycastHit2D nearestHit)
    {
        nearestHit = default;

        int hitCount = bodyCollider.Cast(
            direction,
            movementFilter,
            castHits,
            distance + skinWidth
        );

        if (hitCount <= 0)
        {
            return false;
        }

        float nearestDistance = float.MaxValue;
        bool foundValidHit = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = castHits[i];

            if (hit.collider == null)
            {
                continue;
            }

            if (hit.collider.transform.root == transform.root)
            {
                continue;
            }

            float normalDot = Vector2.Dot(direction, hit.normal);

            if (normalDot >= blockingNormalThreshold)
            {
                continue;
            }

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestHit = hit;
                foundValidHit = true;
            }
        }

        return foundValidHit;
    }

    private Vector2 ClipDeltaByNormal(Vector2 delta, Vector2 normal)
    {
        float dot = Vector2.Dot(delta, normal);

        if (dot >= 0f)
        {
            return delta;
        }

        return delta - normal * dot;
    }
}