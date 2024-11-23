using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInfo : MonoBehaviour
{
    public bool hasDoor = false;

    public void CheckDoors()
    {
        StartCoroutine(WaitASec());
    }

    IEnumerator WaitASec()
    {
        yield return new WaitForSeconds(1.5f);
        Vector3 direction;
        switch (tag)
        {
            case "Left Door":
                direction = Vector3.left;
                break;
            case "Right Door":
                direction = Vector3.right;
                break;
            case "Top Door":
                direction = Vector3.up;
                break;
            case "Bottom Door":
                direction = Vector3.down;
                break;
            default:
                direction = Vector3.zero;
                break;
        }

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 1))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Intersection Checker"))
            {
                //Debug.Log(name + " has a connector attached!");
                hasDoor = true;
            }
        }

        if (hasDoor) //TODO: Animate this
        {
            Vector3 transformPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 3f);
            transform.position = transformPos;
        }
    }
}
