using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointNavigator : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints for navigation
    private int currentWaypointIndex = 0; // Index to track the current waypoint
    private NavMeshAgent agent; // Reference to the NavMeshAgent component
    private Animator animator; // Reference to the Animator component

    void Start()
    {
        // Get the NavMeshAgent component attached to the character
        agent = GetComponent<NavMeshAgent>();

        // Get the Animator component attached to the character
        animator = GetComponent<Animator>();

        // Start moving towards the first waypoint if waypoints exist
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        // Update the animator's speed parameter based on agent velocity
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        // Check if the agent has reached its current waypoint
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToNextWaypoint();
        }
    }

    void MoveToNextWaypoint()
    {
        // Check if the current waypoint is not the last one
        if (currentWaypointIndex < waypoints.Length - 1)
        {
            currentWaypointIndex++; // Increment to the next waypoint
            agent.SetDestination(waypoints[currentWaypointIndex].position); // Set new destination
        }
        else
        {
            // Stop the agent when it reaches the last waypoint
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f); // Set speed to 0 to trigger idle animation
        }
    }
}
