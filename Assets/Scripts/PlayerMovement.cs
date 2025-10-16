using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;                    
    public float airControlMultiplier = 0.6f;   
    public float stopLerpFactor = 0.15f;        

    [Header("Step Settings")]
    public float maxStepHeight = 0.35f;         // Max height player can step up
    public float stepCheckDistance = 0.5f;      // How far forward to check for steps

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
        HandleStepClimbing();

        // Keep player grounded
        if (isGrounded)
            rb.AddForce(Vector3.down * 5f, ForceMode.Acceleration);
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
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, groundRayLength, groundMask))
            {
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, hit.normal);
            }
        }

        // Smooth horizontal velocity for soft stop
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 newHorizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, stopLerpFactor);

        rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
    }

    void HandleStepClimbing()
    {
        if (!isGrounded || inputDir.magnitude == 0) return;

        Vector3 moveDir = inputDir.normalized;
        float capsuleRadius = 0.3f;
        float stepTopHeight = maxStepHeight;

        Vector3 bottom = transform.position + Vector3.up * 0.05f;
        Vector3 top = bottom + Vector3.up * maxStepHeight;

        // 1️⃣ Check if horizontal movement is already free
        if (!Physics.CapsuleCast(bottom, top, capsuleRadius, moveDir, stepCheckDistance, groundMask))
        {
            // Nothing blocking at current height, no need to step up
            return;
        }

        // 2️⃣ Check if there is space above the step
        Vector3 stepCheckBottom = bottom + Vector3.up * maxStepHeight;
        Vector3 stepCheckTop = top + Vector3.up * maxStepHeight;
        if (!Physics.CapsuleCast(stepCheckBottom, stepCheckTop, capsuleRadius, moveDir, stepCheckDistance, groundMask))
        {
            // Safe to step up, move Rigidbody by maxStepHeight
            rb.position += Vector3.up * maxStepHeight;
        }
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

    public Vector3 GetMovementDirection() => inputDir;

    public void SetStanding(bool state)
    {
        onHindLegs = state;
        animator.SetBool("isStanding", state);
    }
}
