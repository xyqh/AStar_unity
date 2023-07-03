using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node> {
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int heapIdx;
    public bool walkable;
    public int movementPenalty;

    public int HeapIndex
    {
        get
        {
            return heapIdx;
        }
        set
        {
            heapIdx = value;
        }
    }

    public float gCost;
    public float hCost;
    public float fCost {
        get
        {
            return gCost + hCost;
        }
    }
    public bool isInPath = false;

    public Node parent;

    public Node(Vector3 _worldPos, bool _walkable, int _gridx, int _gridy, int _movementpenalty)
    {
        worldPosition = _worldPos;
        gridX = _gridx;
        gridY = _gridy;
        walkable = _walkable;
        movementPenalty = _movementpenalty;
    }

    public override string ToString()
    {
        return string.Format("({0},0,{1})", gridX, gridY);
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
