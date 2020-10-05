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

            if(math.dot(dc, math.cross(da,db)) != 0) return false;

            var s = math.dot(math.cross(dc, db), math.cross(da, db)) / math.dot(math.cross(da, db), math.cross(da, db));
            if(s > 0 && s < 1)
            {
                return true;
            }


            return false;

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

            LogTool.AssertIsTrue(GeometryTools.IsLineSegmentIntersection(p1,p2,p3,p4));
            LogTool.AssertIsFalse(GeometryTools.IsLineSegmentIntersection(p1,p2,p5,p6));
        }
    }
}