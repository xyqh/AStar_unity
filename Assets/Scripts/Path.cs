using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {

    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;

    public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst)
    {
        lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        // waypoints里不包含startPos
        Vector2 previousPoint = V3ToV2(startPos);
        for(int i = 0; i < lookPoints.Length; ++i)
        {
            Vector2 currentPoint = V3ToV2(lookPoints[i]);
            // 上一个点到当前点的方向向量
            Vector2 dirToCuttentPoint = (currentPoint - previousPoint).normalized;
            // 开始转向的点，如果是最后一个点就直接取，否则取这个点往上一个点的方向偏移turnDst距离的点
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCuttentPoint * turnDst;
            // 根据转向点及上一个点同样往dirToCuttentPoint反方向偏移turnDst距离的点获得一条直线
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCuttentPoint * turnDst);
            previousPoint = turnBoundaryPoint;
        }

        // 计算开始减速的点，通过给定距离来处理
        float dstFromEndPoint = 0;
        for (int i = lookPoints.Length - 1; i > 0; --i)
        {
            dstFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);
            if (dstFromEndPoint > stoppingDst)
            {
                slowDownIndex = i;
                break;
            }
        }
    }

    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    public void DrawWithGizmos()
    {
        Gizmos.color = Color.black;
        foreach(Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.up, Vector3.one);
        }

        Gizmos.color = Color.white;
        foreach(Line l in turnBoundaries)
        {
            l.DrawWithGizmos(10);
        }
    }
}
