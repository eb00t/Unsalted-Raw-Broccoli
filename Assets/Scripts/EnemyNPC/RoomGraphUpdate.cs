using Pathfinding;
using UnityEngine;

public class RoomGraphUpdater : MonoBehaviour
{
    private void Start()
    {
        var gus = GetComponentInChildren<GraphUpdateScene>();
        if (gus != null)
        {
            gus.Apply();
        }
    }
}

