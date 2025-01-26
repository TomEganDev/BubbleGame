using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointMovement))]
public class WaypointMovementEditor : Editor
{
    private void OnSceneGUI()
    {
        var waypointMovement = target as WaypointMovement;
        if (waypointMovement == null || waypointMovement.Waypoints == null)
        {
            return;
        }

        foreach (var waypoint in waypointMovement.Waypoints)
        {
            if (waypoint == null)
            {
                continue;
            }
            waypoint.position = Handles.PositionHandle(waypoint.position, Quaternion.identity);
        }
        
        // todo - easy create new waypoint
    }
}
