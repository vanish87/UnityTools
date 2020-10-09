using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Test;

namespace UnityTools.Algorithm
{
    public class GeometryTools
    {
        private static float Norm2(float3 v)
        {
            return math.dot(v, v);
        }
        public static bool IsLineSegmentIntersection(IPoint a1, IPoint a2, IPoint b1, IPoint b2)
        {
            LogTool.AssertIsTrue(a1 != a2);
            LogTool.AssertIsTrue(b1 != b2);

            LogTool.AssertIsFalse(a1 == b1 && a2 == b2);

            var da = a2.Position - a1.Position;
            var db = b2.Position - b1.Position;
            var dc = b1.Position - a1.Position;

            if (math.dot(dc, math.cross(da, db)) != 0) return false;

            var n2 = Norm2(math.cross(da, db));

            var s = math.dot(math.cross(dc, db), math.cross(da, db)) / n2;
            var t = math.dot(math.cross(dc, da), math.cross(da, db)) / n2;
            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                return true;
            }


            return false;

        }
        //Line segment-line segment intersection in 2d space by using the dot product
        //p1 and p2 belongs to line 1, and p3 and p4 belongs to line 2 
        public static bool AreLineSegmentsIntersectingDotProduct(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            bool isIntersecting = false;

            if (IsPointsOnDifferentSides(p1, p2, p3, p4) && IsPointsOnDifferentSides(p3, p4, p1, p2))
            {
                isIntersecting = true;
            }

            return isIntersecting;
        }

        //Are the points on different sides of a line?
        private static bool IsPointsOnDifferentSides(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            bool isOnDifferentSides = false;

            //The direction of the line
            Vector3 lineDir = p2 - p1;

            //The normal to a line is just flipping x and z and making z negative
            Vector3 lineNormal = new Vector3(-lineDir.z, lineDir.y, lineDir.x);

            //Now we need to take the dot product between the normal and the points on the other line
            float dot1 = Vector3.Dot(lineNormal, p3 - p1);
            float dot2 = Vector3.Dot(lineNormal, p4 - p1);

            //If you multiply them and get a negative value then p3 and p4 are on different sides of the line
            if (dot1 * dot2 < 0f)
            {
                isOnDifferentSides = true;
            }

            return isOnDifferentSides;
        
        }

public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
{
	//To avoid floating point precision issues we can add a small value
	float epsilon = 0.00001f;

	bool isIntersecting = false;

	float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

	//Make sure the denominator is > 0, if not the lines are parallel
	if (denominator != 0f)
	{
		float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
		float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

		//Are the line segments intersecting if the end points are the same
		if (shouldIncludeEndPoints)
		{
			//Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
			if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
			{
				isIntersecting = true;
			}
		}
		else
		{
			//Is intersecting if u_a and u_b are between 0 and 1
			if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
			{
				isIntersecting = true;
			}
		}
	}

	return isIntersecting;
}
    }

    public class GeometryToolsTest : ITest
    {
        public string Name => this.ToString();

        public void Report()
        {
        }

        public void RunTest()
        {
            var p1 = new Point() { Position = new float3(0, 0, 0) };
            var p2 = new Point() { Position = new float3(1, 1, 0) };
            var p3 = new Point() { Position = new float3(0, 1, 0) };
            var p4 = new Point() { Position = new float3(1, 0, 0) };


            var p5 = new Point() { Position = new float3(1, 0, 0) };
            var p6 = new Point() { Position = new float3(2, 0, 0) };

            LogTool.AssertIsTrue(GeometryTools.IsLineSegmentIntersection(p1, p2, p3, p4));
            LogTool.AssertIsFalse(GeometryTools.IsLineSegmentIntersection(p1, p2, p5, p6));
        }
    }
}