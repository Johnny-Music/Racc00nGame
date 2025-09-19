using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
        HandleInput();
        HandleAnimations();
        HandleFlip();
        MovePlayer();
    }

    void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        inputDir = new Vector3(x, 0f, z).normalized;

        // Snap to 0 if input is tiny (avoids drifting)
        if (inputDir.magnitude < 0.1f)
            inputDir = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // Check if holding an object
            bool isHolding = GetComponent<ItemPickup>().HoldingObject();

            if (isHolding)
            {
                // Force standing if holding something
                onHindLegs = true;
            }
            else
            {
                // Toggle if not holding
                onHindLegs = !onHindLegs;
            }

            animator.SetBool("isStanding", onHindLegs);
        }


    }

    void MovePlayer()
    {
        Vector3 moveDir = inputDir;

        // Slope adjustment
        RaycastHit hit;
        if (controller.isGrounded && Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            Vector3 slopeNormal = hit.normal;

            // Project movement onto slope
            Vector3 slopeDir = Vector3.ProjectOnPlane(moveDir, slopeNormal).normalized;

            // Check if the player is trying to walk uphill
            float uphillFactor = Vector3.Dot(slopeNormal, Vector3.up); // 1 = flat, 0 = vertical wall
            float slopeAngle = Vector3.Angle(slopeNormal, Vector3.up); // in degrees

            // Dot product between input and slope direction
            bool isUphill = Vector3.Dot(moveDir, slopeNormal) < 0;

            if (isUphill)
            {
                // Apply slowdown based on steepness
                float slopeMultiplier = Mathf.Clamp01(1f - (slopeAngle / 60f)); // tune 60f as needed
                moveDir = slopeDir * slopeMultiplier;
            }
            else
            {
                // If not uphill, just project input onto slope without slowdown
                moveDir = slopeDir;
            }
        }

        // Smooth horizontal movement
        Vector3 targetVelocity = moveDir * speed;
        Vector3 currentHorizontal = new Vector3(velocity.x, 0, velocity.z);
        Vector3 smoothed = Vector3.Lerp(currentHorizontal, targetVelocity, Time.deltaTime * 10f);

        // Apply gravity
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Update full velocity
        velocity = new Vector3(smoothed.x, velocity.y, smoothed.z);
        controller.Move(velocity * Time.deltaTime);
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
