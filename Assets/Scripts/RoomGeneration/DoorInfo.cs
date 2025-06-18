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
    private RoomScripting _roomScripting;
    private Vector3 _initialPosition;
    public bool closed;
    private float _lerpTime;
    private Renderer _renderer;
    private bool horiz;
    private bool pos;
    private Animator _doorAnimator;
    public SpriteRenderer doorIcon;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _roomInfo = transform.root.GetComponent<RoomInfo>();
        _roomScripting = transform.root.GetComponent<RoomScripting>();
        _initialPosition = transform.position;
        _doorAnimator = gameObject.GetComponent<Animator>();
        doorIcon = GetComponentInChildren<SpriteRenderer>();
        if (_roomScripting != null)
        {
            doorIcon.enabled = false;
        }
    }

    public void
        CheckDoors() // Check if a connector or door (from another room) is nearby, and open it up. Also contains code to instantiate connectors to distant rooms.
    {
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
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Connector Intersection Checker") &&
                !hit.transform.gameObject.tag.Contains("Door"))
            {
                Debug.Log(name + " hit a connector! " + "(" + hit.transform.gameObject.name + ")");
                if (!_roomInfo.attachedConnectors.Contains(hit.transform.gameObject))
                {
                    _roomInfo.attachedConnectors.Add(hit.transform.gameObject);
                }

                _connectorRoomInfo = hit.transform.GetComponent<ConnectorRoomInfo>();

                if (!_connectorRoomInfo.attachedRooms.Contains(transform.root.gameObject))
                {
                    _connectorRoomInfo.attachedRooms.Add(transform.root.gameObject);
                    _connectorRoomInfo.attachedDoors.Add(gameObject);
                }

                if (!_roomInfo.usableDoors.Contains(gameObject))
                {
                    _roomInfo.usableDoors.Add(gameObject);
                }

                OpenDoor();
            }
            else if (hit.transform.gameObject.tag.Contains("Door") && hit.transform.gameObject.layer !=
                     LayerMask.NameToLayer("Connector Intersection Checker"))
            {
                var transformPosition = hit.transform.position;
                if (transformPosition.y == gameObject.transform.position.y)
                {
                    //Debug.Log(name + " hit another door! " + "(" + hit.transform.gameObject.name + ")");
                    OpenDoor();
                    hit.transform.gameObject.GetComponent<DoorInfo>().hasDoor = true;
                    _roomInfo.usableDoors.Add(gameObject);
                }
            }
            else if (!hit.transform.gameObject.tag.Contains("Door") && hit.transform.gameObject.layer !=
                     LayerMask.NameToLayer("Connector Intersection Checker"))
            {
                CloseDoor();
            }
            else
            {
                CloseDoor();
            }

            /* if ((!hit.transform.gameObject.tag.Contains("Wall") || !hit.transform.gameObject.tag.Contains("Door")) && _roomInfo.bossRoom == false)
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
         else
         {
             CloseDoor();
         }
     }*/
        }
    }

    public void OpenDoor()
    {
        hasDoor = true;
        doorIcon.color = Color.grey;
        Debug.Log("Opening door (" + gameObject.name + ") in " + transform.root.name);

    }

    public void CloseDoor()
    {
        doorIcon.color = Color.white;
        hasDoor = false;
        Debug.Log("Closing door (" + gameObject.name + ") in " + transform.root.name);
    }

    public void PlaySlamSound()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.DoorSlam, transform.position);
    }

    private void Update()
    {
        _doorAnimator.SetBool(CloseDoors, !hasDoor);
        if (_roomScripting != null)
        {
            if (_roomScripting.playerHasEnteredRoom && doorIcon.enabled == false)
            {
                doorIcon.enabled = true;
            }
        }
    }
}
