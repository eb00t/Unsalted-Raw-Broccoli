using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInfo : MonoBehaviour
{
    public bool hasDoor = false;
    private RoomInfo _roomInfo;

    public void CheckDoors()
    {
        StartCoroutine(WaitASec());
    }
    private void Start()
    {
        _roomInfo = transform.root.GetComponent<RoomInfo>();
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
                _roomInfo.attachedConnectors.Add(hit.transform.gameObject);
                hasDoor = true;
            } 
            else if (hit.transform.gameObject.tag.Contains("Door"))
            {
                var transformPosition = hit.transform.position;
                if (transformPosition.y == gameObject.transform.position.y)
                {
                    hasDoor = true;
                    hit.transform.gameObject.GetComponent<DoorInfo>().hasDoor = true;
                }
            }
        }

        if (hasDoor) //TODO: Animate this
        {
            Vector3 transformPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 3f);
            transform.position = transformPos;
        }
    }
}
