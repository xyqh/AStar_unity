using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid : MonoBehaviour {
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
	public float nodeRadius;
    public TerrainType[] walkableRegions;
    public Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    public List<Node> path;
    LayerMask walkableMask = 0;

    float nodeDiameter;
    int gridSizeX, gridSizeY;
    Node[,] grid;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;
    public int blurSizeCustom = 3;
    public int obstacleProximityPenalty = 10;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.CeilToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.CeilToInt(gridWorldSize.y / nodeDiameter);
        for(int i = 0; i < walkableRegions.Length; ++i)
        {
            walkableMask.value |= walkableRegions[i].terrianMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(walkableRegions[i].terrianMask.value, 2), walkableRegions[i].terrianPenalty);
        }
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = this.transform.position - Vector3.right * gridSizeX / 2 - Vector3.forward * gridSizeY / 2;

        for (int i = 0; i < gridSizeX; ++i)
        {
            for(int j = 0; j < gridSizeY; ++j)
            {
                Vector3 worldPos = worldBottomLeft + Vector3.right * (i * nodeDiameter + nodeRadius) + Vector3.forward * (j * nodeDiameter + nodeRadius);
                bool walkable = !Physics.CheckSphere(worldPos, nodeRadius, unwalkableMask);
                int movementPenalty = 0;

                // raycast
                Ray ray = new Ray(worldPos + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, walkableMask))
                {
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }

                if (!walkable)
                {
                    movementPenalty += obstacleProximityPenalty;
                }

                grid[i, j] = new Node(worldPos, walkable, i, j, movementPenalty);
            }
        }

        BlurPenaltyMap(blurSizeCustom);
    }

    // 均值滤波
    void BlurPenaltyMap(int blurSize)
    {
        int kernalSize = 2 * blurSize + 1;
        int kernalExtens = blurSize;

        int[,] newGrid = new int[gridSizeX, gridSizeY];
        for(int y = 0; y < gridSizeY; ++y)
        {
            int filter = 0;
            for(int x = -kernalExtens; x < kernalExtens; ++x)
            {
                int _x = Mathf.Clamp(x, 0, gridSizeX - 1);
                filter += grid[_x, y].movementPenalty;
            }

            for(int x = 0; x < gridSizeX; ++x)
            {
                int addIndex = Mathf.Clamp(x + kernalExtens, 0, gridSizeX - 1);
                filter += grid[addIndex, y].movementPenalty;
                newGrid[x, y] = filter;
                int reduceIndex = Mathf.Clamp(x - kernalExtens, 0, gridSizeX - 1);
                filter -= grid[reduceIndex, y].movementPenalty;
            }
        }

        for (int x = 0; x < gridSizeX; ++x)
        {
            int filter = 0;
            for (int y = -kernalExtens; y < kernalExtens; ++y)
            {
                int _y = Mathf.Clamp(y, 0, gridSizeY - 1);
                filter += newGrid[x, _y];
            }

            for (int y = 0; y < gridSizeY; ++y)
            {
                int addIndex = Mathf.Clamp(y + kernalExtens, 0, gridSizeY - 1);
                filter += newGrid[x, addIndex];
                int penalty = Mathf.FloorToInt((float)filter/kernalSize/kernalSize);
                grid[x, y].movementPenalty = penalty;
                if(penalty < penaltyMin)
                {
                    penaltyMin = penalty;
                }
                if (penalty > penaltyMax)
                {
                    penaltyMax = penalty;
                }
                int reduceIndex = Mathf.Clamp(y - kernalExtens, 0, gridSizeY - 1);
                filter -= newGrid[x, reduceIndex];
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach(Node n in grid)
            {
				Gizmos.color = Color.Lerp (Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
                Gizmos.color = n.walkable ? Gizmos.color : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeDiameter);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 position)
    {
        float percentX = (position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (position.z + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int gridx = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int gridy = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        
        return grid[gridx, gridy];
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for(int x = -1; x <= 1; ++x)
        {
            for(int y = -1; y <= 1; ++y)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                if(checkX < 0 || checkX >= gridSizeX || checkY < 0 || checkY >= gridSizeY)
                {
                    continue;
                }

                neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrianMask;
        public int terrianPenalty;
    }
}
