using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterNavigation : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    private List<Vector3> anchorPositions = new List<Vector3>();
    private HashSet<int> visitedAnchors = new HashSet<int>();  // Track visited anchors using their indices
    private int currentAnchorIndex = -1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Wait for AnchorManager to finish loading anchors and then load positions
        StartCoroutine(InitializeNavigation());
    }

    System.Collections.IEnumerator InitializeNavigation()
    {
        // Wait until AnchorManager is ready
        while (AnchorManager.Instance == null || AnchorManager.Instance.GetAnchorPositions().Count == 0)
        {
            yield return null;
        }

        // Load anchor positions
        anchorPositions = AnchorManager.Instance.GetAnchorPositions();

        // Start moving to the closest anchor if available
        if (anchorPositions.Count > 0)
        {
            MoveToClosestAnchor();
        }
    }

    void Update()
    {
        // Update the animator's speed parameter based on agent velocity
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        // Check if the agent has reached the current anchor
        if (!agent.pathPending && agent.remainingDistance < 0.5f && currentAnchorIndex != -1)
        {
            // Mark the current anchor as visited
            visitedAnchors.Add(currentAnchorIndex);
            // Move to the next closest unvisited anchor
            MoveToClosestAnchor();
        }
    }

    void MoveToClosestAnchor()
    {
        float minDistance = float.MaxValue;
        int closestAnchorIndex = -1;

        // Find the closest unvisited anchor
        for (int i = 0; i < anchorPositions.Count; i++)
        {
            if (visitedAnchors.Contains(i)) continue;  // Skip visited anchors

            float distance = Vector3.Distance(transform.position, anchorPositions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestAnchorIndex = i;
            }
        }

        // Move to the closest anchor if one was found
        if (closestAnchorIndex != -1)
        {
            agent.SetDestination(anchorPositions[closestAnchorIndex]);
            currentAnchorIndex = closestAnchorIndex;
        }
        else
        {
            Debug.Log("No more unvisited anchors available.");
        }
    }
}
