using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class RandomRoaming : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    public Node startingNode; // Starting waypoint node
    private Node currentNode;
    private string previousNodeId;
    private bool isMoving = false; // Track if agent is currently moving
    private Node nextNode; // Store the next node to move towards
    private bool isRotating = false; // Track if the character is currently rotating

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Set the starting node and initialize the first movement
        if (startingNode != null)
        {
            currentNode = startingNode;
            SetNextDestination();
        }
        else
        {
            Debug.LogWarning("No starting node set for the character.");
        }
    }

    void Update()
    {
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed); // Ensure this is updating in real-time
        }

        // Check if the character has reached the current destination and is ready for the next
        if (isMoving && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isMoving = false; // Destination reached, ready to select the next node
            StartCoroutine(WaitAndSetNextDestination()); // Delay before setting next destination
        }

        // Handle rotation before setting the destination
        if (isRotating)
        {
            RotateTowards(nextNode.transform.position);
        }
    }


    // Coroutine to add a brief delay before selecting the next destination
    private IEnumerator WaitAndSetNextDestination()
    {
        yield return new WaitForSeconds(1.0f); // Add a delay for a natural pause
        SetNextDestination();
    }

    // Set the next destination for the agent
    void SetNextDestination()
    {
        if (currentNode == null || currentNode.neighbors.Count == 0)
            return;

        nextNode = ChooseNextNode();
        if (nextNode != null)
        {
            // Begin rotating towards the next node before starting movement
            isRotating = true;
            previousNodeId = currentNode.nodeId;
            currentNode = nextNode;
        }
    }

    // Rotate the character to face the next destination
    void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);

        // Check if rotation is nearly complete to proceed with movement
        if (Quaternion.Angle(transform.rotation, lookRotation) < 1f)
        {
            isRotating = false; // End rotation
            agent.SetDestination(nextNode.transform.position); // Now move towards the next node
            isMoving = true;
        }
    }

    // Select the next node excluding the previous node
    Node ChooseNextNode()
    {
        List<Node> possibleNodes = new List<Node>();

        foreach (Node neighbor in currentNode.neighbors)
        {
            if (neighbor.nodeId != previousNodeId)
            {
                possibleNodes.Add(neighbor);
            }
        }

        if (possibleNodes.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleNodes.Count);
            return possibleNodes[randomIndex];
        }

        return null; // No valid moves without backtracking
    }
}
