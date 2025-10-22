using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float airControlMultiplier = 0.6f;
    public float stopLerpFactor = 0.15f;

    [Header("References")]
    public SpriteRenderer sr;
    public Animator animator;
    public Transform groundCheck;
    public LayerMask groundMask;

    private Rigidbody rb;
    private Vector3 inputDir;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;
    private float groundRayLength = 0.3f;

    [Header("Standing State")]
    public bool onHindLegs = false;
    public bool wasOnHindLegsBeforePickup = false;

    [Header("Step Climbing Settings")]
    public float maxStepHeight = 0.4f;         // Maximum height the player can step over
    public float stepCheckDistance = 0.3f;     // Distance to check forward for steps
    
    [Header("Pulling Mode")]
    public bool isPulling = false;
    public float pullSpeed = 2f;   // speed when dragging
    private Vector3 lastMoveDir = Vector3.zero;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        HandleAnimations();
        HandleFlip();
        CheckGrounded();
    }

    void FixedUpdate()
    {
        MovePlayer();

        if (isGrounded)
            rb.AddForce(Vector3.down * 5f, ForceMode.Acceleration);

        HandleStepClimbing(); // ✅ Call step climbing here
    }

    void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        inputDir = new Vector3(x, 0f, z).normalized;

        if (inputDir.magnitude < 0.1f)
            inputDir = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            bool isHolding = GetComponent<ItemPickup>().HoldingObject();
            onHindLegs = isHolding ? true : !onHindLegs;
            animator.SetBool("isStanding", onHindLegs);
        }
    }

    void MovePlayer()
    {
        float control = isGrounded ? 1f : airControlMultiplier;
        Vector3 targetVelocity = inputDir * speed * control;

        if (isGrounded)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, groundRayLength, groundMask))
            {
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, hit.normal);
            }
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 newHorizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, stopLerpFactor);

        rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
    }

    void CheckGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        bool sphereHit = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        bool rayHit = Physics.Raycast(origin, Vector3.down, groundRayLength, groundMask);
        isGrounded = sphereHit || rayHit;
    }

    void HandleAnimations()
    {
        animator.SetFloat("xVelocity", Mathf.Abs(inputDir.x));
        animator.SetFloat("zVelocity", Mathf.Abs(inputDir.z));
    }

    void HandleFlip()
    {
        if (inputDir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = (inputDir.x < 0) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    // ✅ Step Climbing Function
    void HandleStepClimbing()
    {
        if (!isGrounded || inputDir.magnitude == 0) return;

        Vector3 moveDir = inputDir.normalized;
        float capsuleRadius = 0.3f;
        float padding = 0.02f; // small buffer to ensure the player clears the step

        // Bottom and top of capsule at current height (foot level)
        Vector3 bottom = transform.position + Vector3.up * 0.05f;
        Vector3 top = bottom + Vector3.up * 0.05f; // thin slice at feet

        // 1) Check if horizontal movement at foot level is blocked
        bool blocked = Physics.CapsuleCast(bottom, top, capsuleRadius, moveDir, stepCheckDistance, groundMask);
        if (!blocked) return;

        // 2) Check if space above the obstacle is free for stepping (at maxStepHeight)
        Vector3 stepCheckBottom = bottom + Vector3.up * maxStepHeight;
        Vector3 stepCheckTop = top + Vector3.up * maxStepHeight;

        if (!Physics.CapsuleCast(stepCheckBottom, stepCheckTop, capsuleRadius, moveDir, stepCheckDistance, groundMask))
        {
            // Safe to step up → move Rigidbody by maxStepHeight + padding
            rb.position += Vector3.up * (maxStepHeight + padding);
            rb.position += moveDir * 0.05f; // small forward nudge
        }
    }

    public Vector3 GetMovementDirection() => inputDir;

    public void SetStanding(bool state)
    {
        onHindLegs = state;
        animator.SetBool("isStanding", state);
    }
}
