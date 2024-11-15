using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public List<Node> neighbors; // List of neighboring nodes
    public string nodeId; // Unique identifier for the node

    private void Awake()
    {
        // If nodeId is not set manually, generate a GUID automatically
        if (string.IsNullOrEmpty(nodeId))
        {
            nodeId = System.Guid.NewGuid().ToString();
        }
    }

    // Draw lines to visualize connections between nodes in the scene
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (Node neighbor in neighbors)
        {
            if (neighbor != null)
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}