using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line {

    const float verticalLineGradient = 1e5f; // 定义为x=n的斜率

    float gradient; // 梯度/斜率
    float y_intercept; // 直线方程中y=ax+b的截距b

    Vector2 pointOnLine_1;
    Vector2 pointOnLine_2;

    float gradientPerpendicular; // 梯度的垂线

    bool approachSide;

    // 寻路中的两个点。(A->B)，pointOnLine是B-(AB)*turnDst，pointPerpendicularToLine是A-(AB)*turnDst
    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
    {
        // 先得到AB直线的斜率
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if(dx == 0)
        {
            // 当dx==0即与y轴平行，此时垂线设为verticalLineGradient
            gradientPerpendicular = verticalLineGradient;
        }
        else
        {
            gradientPerpendicular = dy / dx;
        }

        // 在得到垂直于AB的直线L的斜率
        if(gradientPerpendicular == 0)
        {
            gradient = verticalLineGradient;
        }
        else
        {
            gradient = -1 / gradientPerpendicular;
        }

        // 计算直线L截距
        y_intercept = pointOnLine.y - gradient * pointOnLine.x;

        // 得到直线L的两个点
        pointOnLine_1 = pointOnLine;
        // 点1沿L方向前进一段距离得到的点2
        pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

        approachSide = false;
        approachSide = GetSide(pointPerpendicularToLine);
    }

    bool GetSide(Vector2 p)
    {
        // 向量叉乘结果 A:p1->p, B:p1->p2。(A x B)的结果>0时，B在A的逆时针方向；=0时共线；<0时B在A的顺时针方向。x1*y2-x2*y1
        // 这里判断在向量B的某一边，直到跨到另一边
        return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
    }

    public bool HasCrossedLine(Vector2 p)
    {
        // 判断是否跨过向量
        return GetSide(p) != approachSide;
    }

    public float DistanceFromPoint(Vector2 p)
    {
        float yInterceptPerpendicular = p.y - gradientPerpendicular * p.x;
        float intersectX = (yInterceptPerpendicular - y_intercept) / (gradient - gradientPerpendicular);
        float intersectY = gradient * intersectX + y_intercept;
        return Vector2.Distance(p, new Vector2(intersectX, intersectY));
    }

    public void DrawWithGizmos(float length)
    {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
        Vector3 lineCentre = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
        Gizmos.DrawLine(lineCentre - lineDir * length / 2f, lineCentre + lineDir * length / 2f);
    }
}
