using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathPlanning : MonoBehaviour
{
    public List<Transform> allWaypoints; // All waypoints tagged as "Waypoint"
    private NavMeshAgent agent;
    private HashSet<Transform> visitedWaypoints = new HashSet<Transform>();
    private List<Transform> blockedWaypoints = new List<Transform>(); // To store already visited waypoints
    private Transform currentTarget;
    public Transform targetA;
    public Transform targetB;
    public Transform origin;
    private Transform selectedTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Find all waypoints with the "Waypoint" tag
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        allWaypoints = new List<Transform>();
        foreach (GameObject waypoint in waypointObjects)
        {
            allWaypoints.Add(waypoint.transform);
        }

        Debug.Log("PathPlanning script initialized. Waiting for user input to set the target.");
    }

    void Update()
    {
        // Handle user input to select a new target dynamically
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Press 1 to set Target A
        {
            SelectTarget(targetA);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // Press 2 to set Target B
        {
            SelectTarget(targetB);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0)) // Press 0 to return to Origin
        {
            SelectTarget(origin);
        }

        // Check if the agent has reached the current waypoint
        if (currentTarget != null && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (currentTarget == selectedTarget)
            {
                Debug.Log($"Reached final target: {selectedTarget.name}");
                GetComponent<Animator>().SetFloat("Speed", 0); // Trigger idle animation
                currentTarget = null; // Stop moving
            }
            else
            {
                visitedWaypoints.Add(currentTarget);
                blockedWaypoints.Add(currentTarget); // Add the visited waypoint to blocked waypoints
                SetNextOptimalWaypoint(selectedTarget);
            }
        }
    }

    void SelectTarget(Transform target)
    {
        Debug.Log($"Selecting new target: {target.name}");
        selectedTarget = target;
        visitedWaypoints.Clear(); // Reset visited waypoints
        blockedWaypoints.Clear(); // Reset blocked waypoints
        SetNextOptimalWaypoint(selectedTarget);
    }

    void SetNextOptimalWaypoint(Transform target)
    {
        float minDistanceFromAgent = Mathf.Infinity;
        Transform closestWaypoint = null;

        // Find the closest unvisited waypoint that also brings the agent closer to the target
        foreach (Transform waypoint in allWaypoints)
        {
            if (!visitedWaypoints.Contains(waypoint))
            {
                float distanceFromAgent = Vector3.Distance(transform.position, waypoint.position);
                float distanceToTarget = Vector3.Distance(waypoint.position, target.position);

                // Prioritize waypoints that bring the agent closer to the target and are closest to the agent
                if (distanceFromAgent < minDistanceFromAgent && distanceToTarget < Vector3.Distance(transform.position, target.position))
                {
                    minDistanceFromAgent = distanceFromAgent;
                    closestWaypoint = waypoint;
                }
            }
        }

        // Set the new target waypoint
        if (closestWaypoint != null)
        {
            currentTarget = closestWaypoint;
            agent.SetDestination(currentTarget.position);
            GetComponent<Animator>().SetFloat("Speed", 1); // Trigger walking animation
            Debug.Log($"Setting destination to waypoint: {currentTarget.name}");
        }
        else
        {
            Debug.Log("No optimal waypoint found. Stopping movement.");
            GetComponent<Animator>().SetFloat("Speed", 0); // Trigger idle animation
        }
    }
}

