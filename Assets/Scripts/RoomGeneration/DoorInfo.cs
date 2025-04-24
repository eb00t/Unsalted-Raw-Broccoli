using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DoorInfo : MonoBehaviour
{
    private static readonly int CloseDoors = Animator.StringToHash("closeDoors");
    public bool hasDoor = false;
    private RoomInfo _roomInfo;
    private ConnectorRoomInfo _connectorRoomInfo;
    private Vector3 _initialPosition;
    public bool closed;
    private float _lerpTime;
    private Renderer _renderer;
    private bool horiz;
    private bool pos;
    private Animator _doorAnimator;
private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _roomInfo = transform.root.GetComponent<RoomInfo>();
        _initialPosition = transform.position;
        _doorAnimator = gameObject.GetComponent<Animator>();
    }
    public void CheckDoors()
    {
        Debug.Log("Checking for doors");
        Vector3 direction;
        switch (tag)
        {
            case "Left Door":
                direction = Vector3.left;
                horiz = true;
                pos = false;
                break;
            case "Right Door":
                direction = Vector3.right;
                horiz = true;
                pos = true;
                break;
            case "Top Door":
                direction = Vector3.up;
                horiz = false;
                pos = true;
                break;
            case "Bottom Door":
                direction = Vector3.down;
                horiz = false;
                pos = false;
                break;
            default:
                direction = Vector3.zero;
                break;
        }
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 1))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Connector Intersection Checker"))
            {
                Debug.Log(name + " hit a connector! " + "(" + hit.transform.gameObject.name + ")");
                _roomInfo.attachedConnectors.Add(hit.transform.gameObject);
                _connectorRoomInfo = hit.transform.GetComponent<ConnectorRoomInfo>();
                _connectorRoomInfo.attachedRooms.Add(transform.root.gameObject);
                hasDoor = true;
                _roomInfo.usableDoors.Add(gameObject);
            } 
            else if (hit.transform.gameObject.tag.Contains("Door"))
            {
                var transformPosition = hit.transform.position;
                if (transformPosition.y == gameObject.transform.position.y)
                {
                    //Debug.Log(name + " hit another door! " + "(" + hit.transform.gameObject.name + ")");
                    hasDoor = true;
                    hit.transform.gameObject.GetComponent<DoorInfo>().hasDoor = true;
                    _roomInfo.usableDoors.Add(gameObject);
                }
            }

            if (hit.transform.gameObject == null && _roomInfo.bossRoom == false)
            {
                if (Physics.Raycast(transform.position, direction, out RaycastHit hit2, 10))
                {
                    var transformPosition = hit2.transform.position;
                    GameObject newConnector = null;
                    float dist = Vector3.Distance(transformPosition, transform.position);
                    Vector3 newSpawnPoint = Vector3.zero;
                    switch (pos)
                    {
                        case true when horiz:
                            newSpawnPoint = new Vector3(transform.position.x + 2.5f, transform.position.y, transform.position.z);
                            break;
                        case false when horiz:
                            newSpawnPoint = new Vector3(transform.position.x - 2.5f, transform.position.y, transform.position.z);
                            break;
                        case true when horiz is false:
                            newSpawnPoint = new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z);
                            break;
                        case false when horiz is false:
                            newSpawnPoint = new Vector3(transform.position.x, transform.position.y - 2.5f, transform.position.z);
                            break;
                    }
                    switch (horiz)
                    {
                        case true:
                            if (transformPosition.y == gameObject.transform.position.y)
                            {
                                switch (dist)
                                {
                                    case 5:
                                        newConnector = Resources.Load<GameObject>("Room Layouts/Connectors/ConnectorShortHoriz");
                                        break;
                                    case 10:
                                        newConnector = Resources.Load<GameObject>("Room Layouts/Connectors/ConnectorLongHoriz");
                                        break;
                                }
                            }
                            break;
                        case false:
                            if (transformPosition.x == gameObject.transform.position.x)
                            {
                                switch (dist)
                                {
                                    case 5:
                                        newConnector = Resources.Load<GameObject>("Room Layouts/Connectors/ConnectorShortVerti");
                                        break;
                                    case 10:
                                        newConnector = Resources.Load<GameObject>("Room Layouts/Connectors/ConnectorLongVerti");
                                        break;
                                }
                            }
                            break; 
                    }

                    if (newConnector != null && newSpawnPoint != Vector3.zero && (dist == 5 || dist == 10))
                    {
                        Instantiate(newConnector, newSpawnPoint, Quaternion.identity);
                    }
                       
                }
            }
        }
        if (hasDoor)
        {
            OpenDoor();
        }
        _roomInfo.gameObject.GetComponent<IntersectionRaycast>().FixDoorLayers();
    }

   public void OpenDoor()
   { 
       _doorAnimator.SetBool(CloseDoors, false);
       Debug.Log("Opening door (" + gameObject.name + ") in " + transform.root.name);
      
   }

   public void CloseDoor()
   {
       _doorAnimator.SetBool(CloseDoors, true);
      Debug.Log("Closing door (" + gameObject.name + ") in " + transform.root.name);
     
       
   }
   public void PlaySlamSound()
   {
       AudioManager.Instance.PlayOneShot(FMODEvents.Instance.DoorSlam, transform.position);
   }

}
