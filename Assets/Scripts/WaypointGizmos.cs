using UnityEngine;

public class WaypointGizmos : MonoBehaviour
{
    //public Transform nextWaypoint;  // Reference to the next waypoint (optional)
    public float gizmoRadius = 0.3f;  // Size of the waypoint sphere

    void OnDrawGizmos()
    {
        // Draw a sphere at the transform's position
        Gizmos.color = Color.red;  // Red color to mark waypoints
        Gizmos.DrawSphere(transform.position, gizmoRadius);

        //// Draw a line to the next waypoint if assigned
        //if (nextWaypoint != null)
        //{
        //    Gizmos.color = Color.yellow;  // Yellow color for the connecting line
        //    Gizmos.DrawLine(transform.position, nextWaypoint.position);
        //}
    }
}
