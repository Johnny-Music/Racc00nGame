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
            if (heldObj == null)
            {
                Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, pickUpRange);

                foreach (Collider col in nearbyObjects)
                {
                    if (col.CompareTag("canPickUp"))
                    {
                        var movement = player.GetComponent<PlayerMovementCC>();
                        movement.wasOnHindLegsBeforePickup = movement.onHindLegs;
                        movement.SetStanding(true);

                        PickUpObject(col.gameObject);
                        break;
                    }
                }
            }
            else
            {
                if (canDrop)
                {
                    StopClipping();
                    DropObject();
                }
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
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            heldObj = pickUpObj;
            heldObjRb = pickUpObj.GetComponent<Rigidbody>();
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos.transform;

            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        }
    }

    void DropObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;

        heldObj = null;
        heldObjRb = null;

        player.GetComponent<PlayerMovementCC>().SetStanding(
            player.GetComponent<PlayerMovementCC>().wasOnHindLegsBeforePickup
        );
    }

    void MoveObject()
    {
        heldObj.transform.position = holdPos.transform.position;
    }

    void ThrowObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);

        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;

        Vector3 moveDirection = player.GetComponent<PlayerMovementCC>().GetMovementDirection();

        if (moveDirection.magnitude < 0.1f)
            moveDirection = Vector3.up;

        Vector3 throwDirection = (moveDirection.normalized + Vector3.up * 1.2f).normalized;

        heldObjRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        heldObjRb.AddTorque(Random.onUnitSphere * 1f, ForceMode.Impulse);

        heldObj = null;
        heldObjRb = null;

        player.GetComponent<PlayerMovementCC>().SetStanding(
            player.GetComponent<PlayerMovementCC>().wasOnHindLegsBeforePickup
        );
    }

    void StopClipping()
    {
        float clipRange = Vector3.Distance(heldObj.transform.position, transform.position);
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);

        if (hits.Length > 1)
        {
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
        }
    }

    public bool HoldingObject()
    {
        return heldObj != null;
    }
}