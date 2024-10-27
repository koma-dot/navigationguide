using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointNavigator : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;
    private Animator animator;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Initialize the agent with the first waypoint position
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    private void Update()
    {
        // Check if the agent is close to the current waypoint
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }

        // Update animation parameters based on speed
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }
}
