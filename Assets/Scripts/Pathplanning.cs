using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class PathPlanning : MonoBehaviour
{
    public List<Transform> allWaypoints; // All waypoints tagged as "Waypoint"
    private NavMeshAgent agent;
    private HashSet<Transform> visitedWaypoints = new HashSet<Transform>();
    private List<Transform> blockedWaypoints = new List<Transform>(); // To store already visited waypoints
    private Transform currentTarget = null;
    public Transform targetA = null;
    public Transform targetB = null;
    private Transform origin = null;
    private Transform selectedTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Store the start position
        origin = transform;
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
                // Stop at the final target
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

    public void GotTargetA()
    {
        Debug.LogWarning("PathPlanning:GotTargetA");
        // Find all waypoints with the "Waypoint" tag
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        allWaypoints = new List<Transform>();
        foreach (GameObject waypoint in waypointObjects)
        {
            allWaypoints.Add(waypoint.transform);
        }

        GameObject[] gos = GameObject.FindGameObjectsWithTag("TargetA");
        if (gos.Length > 0)
        {
            targetA = gos[0].transform;
        }

        Debug.Assert(targetA != null);
        SelectTarget(targetA);
    }

    public void GotTargetB()
    {
        Debug.LogWarning("PathPlanning:GotTargetB");
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        allWaypoints = new List<Transform>();
        foreach (GameObject waypoint in waypointObjects)
        {
            allWaypoints.Add(waypoint.transform);
        }

        GameObject[] gos = GameObject.FindGameObjectsWithTag("TargetB");
        if (gos.Length > 0)
        {
            targetB = gos[0].transform;
        }

        Debug.Assert(targetB != null);
        SelectTarget(targetB);
    }

    public void GotTargetOrigin()
    {
        Debug.LogWarning("PathPlanning:GotTargetOrigin");
        SelectTarget(origin);
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
        // Check if we are already close enough to the final target
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            // Agent reached the final target, stop movement
            currentTarget = target;
            agent.SetDestination(target.position);
            GetComponent<Animator>().SetFloat("Speed", 0); // Trigger idle animation
            Debug.Log($"Reached final target: {target.name}");
            return;
        }

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
            // If no more waypoints, move directly to the final target
            currentTarget = target;
            agent.SetDestination(target.position);
            Debug.Log($"Moving directly to final target: {target.name}");
        }
    }
}
