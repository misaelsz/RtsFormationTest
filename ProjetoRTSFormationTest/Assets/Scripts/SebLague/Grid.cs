//with the exception of some minor changes, this is not the work of GameDevStudent. The code was downloaded from here: https://github.com/SebLague/Pathfinding (Episode 5)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform playerTransform;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public Node[,] grid;
    //an extra variable for an alternate 'findNodeFromWorldPoint' method that I created. 
    public Vector3[,] nodePositions;

    public GameObject experimentCube;
    public int incrementerX, incrementerY;
    public GameObject wall;
    public GameObject building;

    public float nodeDiameter;
    public int gridSizeX, gridSizeY;
    public Vector3 moveCube;

    public static Grid instance; 

    public bool onlyDisplayPathGizmos;

    private void Awake()
    {
        instance = this; 
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        nodePositions = new Vector3[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint =
                    worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) +
                    Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
                nodePositions[x, y] = worldPoint;
            }
        }
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                nodePositions[x, y].x = (int)(nodePositions[x, y].x);
                nodePositions[x, y].z = (int)(nodePositions[x, y].z);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        /* float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;

         Debug.Log("worldPosition.x: " + worldPosition.x);
         Debug.Log("gridWorldSize.x: " + gridWorldSize.x);
         Debug.Log("worldPosition.x + gridWordlSize.x: " + (worldPosition.x + gridWorldSize.x));
         Debug.Log("worldPosition.x + (gridWordlSize.x / 2): " + (worldPosition.x + gridWorldSize.x / 2));
         Debug.Log("(worldPosition.x + (gridWordlSize.x / 2) / gridWorldSize.x): " + ((worldPosition.x + gridWorldSize.x / 2)/ gridWorldSize.x));

         float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;


         percentX = Mathf.Clamp01(percentX);
         Debug.Log("clamped percent x: " + percentX);
         percentY = Mathf.Clamp01(percentY);

         int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);

         Debug.Log("Rounded to int and multipled by xpercent x: " + x); 
         int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
         Debug.Log("node x: " + x + " node y: " + y); 

         return grid[x, y]; */

        //I changed the method here because this just makes more sense to me. Feel free to comment it out if you prefer Sebastian Lague's. 
        int posX = (int)worldPosition.x;
        int posZ = (int)worldPosition.z;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (nodePositions[x, y].x == posX && nodePositions[x, y].z == posZ)
                {
                    return grid[x, y];
                }

            }
        }
        return null;
    }

    public List<Node> GetNeighbours(Node centreNode)
    {
        List<Node> nodeNeighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = centreNode.gridX + x;
                int checkY = centreNode.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    nodeNeighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return nodeNeighbours;
    }

    public List<Node> path;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (onlyDisplayPathGizmos)
        {
            if (path != null)
            {
                foreach (Node n in path)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.nodeWorldPosition, Vector3.one * (nodeDiameter - .1f));
                }
            }
        }
        else
        {
            if (grid != null)
            {
                Node playerNode = NodeFromWorldPoint(playerTransform.position);
                foreach (Node n in grid)
                {
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    if (playerNode == n)
                    {
                        Gizmos.color = Color.green;
                    }
                    if (path != null)
                    {
                        if (path.Contains(n))
                        {
                            Gizmos.color = Color.black;
                        }
                    }
                    Gizmos.DrawCube(n.nodeWorldPosition, Vector3.one * (nodeDiameter - .5f));
                }
            }
        }
    }
}

