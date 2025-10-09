using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementCC : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float gravity = -9.81f;

    [Header("References")]
    public SpriteRenderer sr;
    public Animator animator;

    private CharacterController controller;
    private Vector3 inputDir;
    private Vector3 velocity;
    public bool onHindLegs = false;
    public bool wasOnHindLegsBeforePickup = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused) return;

        HandleInput();
        HandleAnimations();
        HandleFlip();
    }

    void FixedUpdate()
    {
        MovePlayer();
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

            if (isHolding)
                onHindLegs = true;
            else
                onHindLegs = !onHindLegs;

            animator.SetBool("isStanding", onHindLegs);
        }
    }

    void MovePlayer()
    {
        Vector3 moveDir = inputDir;

        // --- Slope handling ---
        if (controller.isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
            {
                Vector3 slopeNormal = hit.normal;
                moveDir = Vector3.ProjectOnPlane(moveDir, slopeNormal).normalized;

                float slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
                float slopeMultiplier = Mathf.Clamp01(1f - (slopeAngle / 60f));
                moveDir *= slopeMultiplier;
            }
        }

        // --- Smooth horizontal movement (time-independent) ---
        Vector3 targetVelocity = moveDir * speed;
        Vector3 currentHorizontal = new Vector3(velocity.x, 0, velocity.z);
        Vector3 smoothed = Vector3.Lerp(currentHorizontal, targetVelocity, 0.2f); // fixed smoothing factor

        // --- Gravity ---
        if (controller.isGrounded)
        {
            velocity.y = -2f; // small downward force to stay grounded
        }
        else
        {
            velocity.y += gravity * Time.fixedDeltaTime;
        }

        // --- Final velocity & move ---
        velocity = new Vector3(smoothed.x, velocity.y, smoothed.z);
        controller.Move(velocity * Time.fixedDeltaTime);
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

    public Vector3 GetMovementDirection()
    {
        return inputDir;
    }

    public void SetStanding(bool state)
    {
        onHindLegs = state;
        animator.SetBool("isStanding", state);
    }
}
