using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointNavigator : MonoBehaviour
{
    public List<Transform> pathAWaypoints; // Waypoints for Path A
    public List<Transform> pathBWaypoints; // Waypoints for Path B

    private List<Transform> currentPath; // Current list of waypoints the agent will follow
    private int currentWaypointIndex = 0; // Current waypoint index

    private NavMeshAgent agent; // Reference to the NavMeshAgent component
    private Animator animator; // Reference to the Animator component

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Set the agent to idle initially
        agent.isStopped = true;
        animator.SetFloat("Speed", 0);
    }

    void Update()
    {
        // Input for switching paths
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartPath(pathAWaypoints);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartPath(pathBWaypoints);
        }

        // Move the agent if a path is set and there are still waypoints left
        if (currentPath != null && currentWaypointIndex < currentPath.Count)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // If we are at the last waypoint, stop moving and set to idle
                if (currentWaypointIndex == currentPath.Count - 1)
                {
                    agent.isStopped = true; // Stop the agent
                    animator.SetFloat("Speed", 0); // Set animator speed to 0 for idle state
                    return;
                }

                // Otherwise, move to the next waypoint
                MoveToNextWaypoint();
            }

            // Set the Speed parameter for the animator based on agent velocity
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    void StartPath(List<Transform> path)
    {
        // Set the current path and start moving
        currentPath = path;
        currentWaypointIndex = 0;

        if (currentPath.Count > 0)
        {
            agent.isStopped = false;
            agent.SetDestination(currentPath[currentWaypointIndex].position);
        }
    }

    void MoveToNextWaypoint()
    {
        if (currentWaypointIndex < currentPath.Count - 1)
        {
            currentWaypointIndex++;
            agent.SetDestination(currentPath[currentWaypointIndex].position);
        }
        else
        {
            // Stop the agent at the last waypoint
            agent.isStopped = true;
            animator.SetFloat("Speed", 0); // Set animator to idle state
            Debug.Log("Agent has reached the last waypoint and stopped.");
        }
    }
}
