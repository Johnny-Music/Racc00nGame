using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;

    public float throwForce = 5f;
    public float pickUpRange = 5f;

    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private bool canDrop = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var movement = player.GetComponent<PlayerMovement>();

            if (heldObj == null)
            {
                Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, pickUpRange);

                foreach (Collider col in nearbyObjects)
                {
                    if (col.CompareTag("canPickUp"))
                    {
                        movement.wasOnHindLegsBeforePickup = movement.onHindLegs;
                        movement.SetStanding(true);
                        PickUpObject(col.gameObject);
                        break;
                    }
                }
            }
            else if (canDrop)
            {
                StopClipping();
                DropObject();
                movement.SetStanding(movement.wasOnHindLegsBeforePickup);
            }
        }

        if (heldObj != null)
        {
            MoveObject();

            if (Input.GetKeyDown(KeyCode.T) && canDrop)
            {
                StopClipping();
                ThrowObject();
            }
        }
    }

    void PickUpObject(GameObject pickUpObj)
    {
        Rigidbody rb = pickUpObj.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            heldObjRb = rb;
            heldObj = rb.gameObject;

            heldObjRb.isKinematic = true;
            heldObj.transform.parent = holdPos;

            Collider col = heldObj.GetComponent<Collider>();
            if (col != null)
                Physics.IgnoreCollision(col, player.GetComponent<Collider>(), true);

            Debug.Log("Picked up: " + heldObj.name);
        }
    }

    void DropObject()
    {
        if (heldObj == null) return;

        Collider col = heldObj.GetComponent<Collider>();
        if (col != null)
            Physics.IgnoreCollision(col, player.GetComponent<Collider>(), false);

        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;

        heldObj = null;
        heldObjRb = null;
    }

    void MoveObject()
    {
        if (heldObj != null)
            heldObj.transform.position = holdPos.position;
    }

    void ThrowObject()
    {
        if (heldObj == null) return;

        Collider col = heldObj.GetComponent<Collider>();
        if (col != null)
            Physics.IgnoreCollision(col, player.GetComponent<Collider>(), false);

        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;

        Vector3 moveDirection = player.GetComponent<PlayerMovement>().GetMovementDirection();
        if (moveDirection.magnitude < 0.1f)
            moveDirection = Vector3.up;

        Vector3 throwDirection = (moveDirection.normalized + Vector3.up * 1.2f).normalized;
        heldObjRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        heldObjRb.AddTorque(Random.onUnitSphere * 1f, ForceMode.Impulse);

        var movement = player.GetComponent<PlayerMovement>();
        movement.SetStanding(movement.wasOnHindLegsBeforePickup);

        heldObj = null;
        heldObjRb = null;
    }

    void StopClipping()
    {
        if (heldObj == null) return;

        float clipRange = Vector3.Distance(heldObj.transform.position, transform.position);
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, clipRange);
        if (hits.Length > 1)
        {
            heldObj.transform.position = transform.position + Vector3.down * 0.5f;
        }
    }

    public bool HoldingObject() => heldObj != null;
}
