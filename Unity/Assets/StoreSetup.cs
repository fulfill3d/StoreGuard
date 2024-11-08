using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class BasicStoreSetup : MonoBehaviour
{
    public Vector3 storeDimensions = new Vector3(15f, 3.5f, 25f);
    public float wallHeight = 3.5f;
    public float doorWidth = 1.5f;
    
    public float shelfHeight = 2f;
    public float shelfWidth = 4f;
    public float shelfDepth = 1f;
    public float shelfSpacing = 5f;

    public GameObject character;  // Reference to the character GameObject with ShelfPathRoaming script
    private Transform[] waypoints;  // Waypoints for the path

    private void Start()
    {
        CreateFloor();
        CreateWalls();
        CreateShelves();
        CreateWaypoints();
        BakeNavMesh();

        // Assign the waypoints to the character's ShelfPathRoaming script
        AssignWaypointsToCharacter();
    }

    // Create a floor plane and extend it outside the store entrance
    private void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localScale = new Vector3(storeDimensions.x, 0.1f, storeDimensions.z + 2f);  // Extend by 2m outside
        floor.transform.position = new Vector3(0f, 0f, -1f);  // Position slightly forward to account for extension
        floor.name = "Floor";
        floor.GetComponent<Renderer>().material.color = Color.gray;

        // Add NavMeshSurface component to make it walkable
        NavMeshSurface navMeshSurface = floor.AddComponent<NavMeshSurface>();
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
    }

    // Create four side walls, with an opening for a door in the front wall
    private void CreateWalls()
    {
        var wallThickness = 0.1f;
        var wallScaleX = new Vector3((storeDimensions.x - doorWidth) / 2, wallHeight, wallThickness);  // Width walls for split front wall
        Vector3 wallScaleZ = new Vector3(wallThickness, wallHeight, storeDimensions.z);  // Depth walls

        // Create split front wall with door opening
        GameObject leftFrontWall = CreateWall(wallScaleX, new Vector3(-doorWidth / 2 - wallScaleX.x / 2, wallHeight / 2, -storeDimensions.z / 2), "Left Front Wall");
        GameObject rightFrontWall = CreateWall(wallScaleX, new Vector3(doorWidth / 2 + wallScaleX.x / 2, wallHeight / 2, -storeDimensions.z / 2), "Right Front Wall");

        // Create back wall (full width)
        GameObject backWall = CreateWall(new Vector3(storeDimensions.x, wallHeight, wallThickness), new Vector3(0, wallHeight / 2, storeDimensions.z / 2), "Back Wall");

        // Create left and right side walls
        GameObject leftWall = CreateWall(wallScaleZ, new Vector3(-storeDimensions.x / 2, wallHeight / 2, 0), "Left Wall");
        GameObject rightWall = CreateWall(wallScaleZ, new Vector3(storeDimensions.x / 2, wallHeight / 2, 0), "Right Wall");
    }

    // Helper function to create a wall with a collider and a NavMeshObstacle
    private GameObject CreateWall(Vector3 scale, Vector3 position, string name)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.localScale = scale;
        wall.transform.position = position;
        wall.name = name;
        wall.GetComponent<Renderer>().material.color = Color.white;

        // Add NavMeshObstacle to treat it as an obstacle for the NavMesh
        NavMeshObstacle obstacle = wall.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;  // Enables carving to dynamically remove this area from the NavMesh
        obstacle.size = scale;

        return wall;
    }

    // Create shelves within the store
    private void CreateShelves()
    {
        for (int i = 0; i < 5; i++)
        {
            var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.transform.localScale = new Vector3(shelfWidth, shelfHeight, shelfDepth);
            shelf.transform.position = new Vector3(-2f, shelfHeight / 2, -storeDimensions.z / 2 + 5f + i * shelfSpacing);  // Position shelves in a row
            shelf.name = $"Shelf_{i + 1}";
            shelf.GetComponent<Renderer>().material.color = Color.white;

            // Add NavMeshObstacle to make shelves hittable as well
            NavMeshObstacle shelfObstacle = shelf.AddComponent<NavMeshObstacle>();
            shelfObstacle.carving = true;
            shelfObstacle.size = shelf.transform.localScale;
        }

        var perpendicularShelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        perpendicularShelf.transform.localScale = new Vector3(shelfDepth, shelfHeight, shelfWidth * 2);  // Rotated dimensions
        perpendicularShelf.transform.position = new Vector3(4f, shelfHeight / 2, -storeDimensions.z / 2 + 10f);  // Position it in front of other shelves
        perpendicularShelf.name = "Shelf_6";
        perpendicularShelf.GetComponent<Renderer>().material.color = Color.white;

        NavMeshObstacle perpendicularShelfObstacle = perpendicularShelf.AddComponent<NavMeshObstacle>();
        perpendicularShelfObstacle.carving = true;
        perpendicularShelfObstacle.size = perpendicularShelf.transform.localScale;
    }

    // Create waypoints for the character to follow between shelves
    private void CreateWaypoints()
    {
        waypoints = new Transform[7];

        waypoints[0] = CreateWaypoint(new Vector3(6f, 0f, -11f), "Entrance Waypoint");
        waypoints[1] = CreateWaypoint(new Vector3(0f, 0f, -11f), "Door Waypoint");
        waypoints[2] = CreateWaypoint(new Vector3(0f, 0f, -9f), "Aisle Start");
        waypoints[3] = CreateWaypoint(new Vector3(-2f, 0f, -1f), "Aisle Mid");
        waypoints[4] = CreateWaypoint(new Vector3(-2f, 0f, 4f), "Aisle End");
        waypoints[5] = CreateWaypoint(new Vector3(2f, 0f, -6f), "Aisle 2 Start");
        waypoints[6] = CreateWaypoint(new Vector3(2f, 0f, 4f), "Aisle 2 End");
    }

    // Helper function to create a waypoint at a specific position
    private Transform CreateWaypoint(Vector3 position, string name)
    {
        GameObject waypoint = new GameObject(name);
        waypoint.transform.position = position;
        return waypoint.transform;
    }

    // Assign the generated waypoints to the character's roaming script
    private void AssignWaypointsToCharacter()
    {
        if (character != null)
        {
            ShelfPathRoaming roamingScript = character.GetComponent<ShelfPathRoaming>();
            if (roamingScript != null)
            {
                roamingScript.waypoints = waypoints;  // Assign waypoints to character's roaming script
            }
        }
    }

    // Bake the NavMesh to ensure the floor is walkable
    private void BakeNavMesh()
    {
        NavMeshSurface navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }
}
