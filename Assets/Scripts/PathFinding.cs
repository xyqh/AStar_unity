using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PathFinding : MonoBehaviour {

    Vector3 begin, end;
    MyGrid grid;
    PathRequestManager requestManager;

	// Use this for initialization
	void Awake () {
        grid = GetComponent<MyGrid>();
    }

    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        Heap<Node> openList = new Heap<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        openList.Add(startNode);
        visited.Add(startNode);
        while (openList.Count > 0)
        {
            Node node = openList.Top();
            openList.Pop();
            closedList.Add(node);

            if (node == targetNode)
            {
                sw.Stop();
                pathSuccess = true;
                break;
            }

            List<Node> neighbours = grid.GetNeighbours(node);
            for (int i = 0; i < neighbours.Count; ++i)
            {
                Node neighbour = neighbours[i];
                if (!neighbour.walkable || closedList.Contains(neighbour)) continue;

                float gCost = node.gCost + GetDistance(node, neighbour) + node.movementPenalty;
                float hCost = GetDistance(neighbour, targetNode);
                if (gCost + hCost < neighbour.fCost || !visited.Contains(neighbour))
                {
                    // 更新之后没有调整堆，导致错误
                    neighbour.gCost = gCost;
                    neighbour.hCost = hCost;
                    neighbour.parent = node;

                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        openList.Add(neighbour);
                    }
                    else
                    {
                        openList.UpdateItem(neighbour);
                    }
                }
            }
        }

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Length > 0;
        }
        callback(new PathResult(waypoints, pathSuccess, request.callback));
    }

    public Vector3[] RetracePath(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != null && currentNode != startNode) // 这里!=的判断会去掉起始点，是合理的
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        
        // 这一行决定了能不能走到目标点上跟目标重合
        if(path.Count > 0) waypoints.Add(path[0].worldPosition);

        for (int i = 1; i < path.Count; ++i)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if(directionOld != directionNew)
            {
                directionOld = directionNew;
                waypoints.Add(path[i].worldPosition);
            }
        }
        return waypoints.ToArray();
    }

    private float GetDistance(Node a, Node b)
    {
        int deltax = Mathf.Abs(a.gridX - b.gridX);
        int deltay = Mathf.Abs(a.gridY - b.gridY);
        if (deltax < deltay)
        {
            return 14 * deltax + 10 * (deltay - deltax);
        }
        return 14 * deltay + 10 * (deltax - deltay);
    }

    //void printFCost(Heap<Node> heap)
    //{
    //    string str = "";
    //    for(int i = 0; i < heap.array.Count; ++i)
    //    {
    //        str += heap.array[i].fCost + " ";
    //    }
    //    Debug.Log("printFCost:" + str);
    //}
}
