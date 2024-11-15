using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ShelfPathRoaming : MonoBehaviour
{
    public Transform[] waypoints;         // Array of waypoints for the character to follow
    public float stoppingDistance = 0.5f; // Distance at which character stops at each waypoint
    public float idleTime = 2f;           // Idle time at each waypoint before moving to the next

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypointIndex = 0; // Index of the current waypoint

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Start the path-following coroutine
        StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        while (true)
        {
            // Set destination to the current waypoint
            agent.SetDestination(waypoints[currentWaypointIndex].position);

            // Update animator to play walk animation
            animator.SetFloat("Speed", agent.speed);

            // Wait until the agent reaches the waypoint
            yield return new WaitUntil(() => agent.remainingDistance <= stoppingDistance);

            // Play idle animation while waiting at the waypoint
            animator.SetFloat("Speed", 0f);

            // Wait for idle time at the waypoint
            yield return new WaitForSeconds(idleTime);

            // Move to the next waypoint, looping back to the start if at the end
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void Update()
    {
        // Update animator based on agent's movement speed
        float speedPercent = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("Speed", speedPercent);
    }
}