using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("References")]
    public SpriteRenderer sr;
    public Animator animator;

    private Rigidbody rb;
    private Vector3 inputDirection;
    private bool onHindLegs = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
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
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        inputDirection = new Vector3(x, 0f, z);

        // Toggle standing
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            onHindLegs = !onHindLegs;
            animator.SetBool("isStanding", onHindLegs);
        }
    }

    void MovePlayer()
    {
        rb.linearVelocity = new Vector3(inputDirection.x * speed, rb.linearVelocity.y, inputDirection.z * speed);
    }

    void HandleAnimations()
    {
        // Update animator parameters
        animator.SetFloat("xVelocity", Mathf.Abs(inputDirection.x));
    }

    void HandleFlip()
    {
        if (inputDirection.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = (inputDirection.x < 0) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}