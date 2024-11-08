using UnityEngine;
using UnityEngine.AI;

public class CharacterRoaming : MonoBehaviour
{
    public Transform[] waypoints;
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();  // Get the Animator component

        // Check if there are waypoints assigned
        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned to CharacterRoaming.");
            return;
        }

        // Set the initial destination
        SetNextWaypoint();
    }

    void Update()
    {
        // Check if NavMeshAgent and Animator are set up correctly
        if (agent == null || animator == null)
            return;

        // Set the Speed parameter in the Animator based on the NavMeshAgent's speed
        animator.SetFloat("Speed", agent.velocity.magnitude);

        // Check if the character has reached the destination
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetNextWaypoint();
        }
    }

    void SetNextWaypoint()
    {
        if (waypoints.Length == 0)
            return;

        // Move to the next waypoint in sequence
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        // Move to the next waypoint in the array
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;  // Loop back to the first waypoint
    }
}