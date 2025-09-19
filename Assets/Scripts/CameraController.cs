using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;         // player target
    public Vector3 offset = new Vector3(0, 2, -4); // default camera offset
    public float smoothSpeed = 10f;  // smoothing for movement
    public float collisionBuffer = 0.2f; // how far from wall camera should stop

    void LateUpdate()
    {
        if (player == null) return;

        // Desired position
        Vector3 desiredPosition = player.position + player.TransformDirection(offset);

        // Raycast from player to desired camera position
        Vector3 direction = desiredPosition - player.position;
        float distance = direction.magnitude;
        RaycastHit hit;

        Vector3 finalPosition = desiredPosition;

        if (Physics.Raycast(player.position, direction.normalized, out hit, distance))
        {
            // If hit something, move camera in front of the hit point
            finalPosition = hit.point - direction.normalized * collisionBuffer;
        }

        // Smooth move camera
        transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * smoothSpeed);

        // Always look at the player
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}
