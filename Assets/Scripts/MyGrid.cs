﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid : MonoBehaviour {
    public bool displayGridGizmos;
    public LayerMask layerMask;
    public Vector2 gridWorldSize;
	public float nodeRadius;

    public List<Node> path;

    float nodeDiameter;
    int gridSizeX, gridSizeY;
    Node[,] grid;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.CeilToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.CeilToInt(gridWorldSize.y / nodeDiameter);
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
                bool walkable = !Physics.CheckSphere(worldPos, nodeRadius, layerMask);
                grid[i, j] = new Node(worldPos, walkable, i, j);
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
				Gizmos.color = (n.walkable)?Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
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
}