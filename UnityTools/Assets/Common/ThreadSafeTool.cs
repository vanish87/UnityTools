using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    public class ThreadSafeRandom
    {
        private static readonly System.Random global = new System.Random();
        [System.ThreadStatic] private static System.Random local;
        public static float NextFloat()
        {
            return Next() * 1.0f / System.Int32.MaxValue;
        }
        public static int Next()
        {
            if (local == null)
            {
                lock (global)
                {
                    if (local == null)
                    {
                        int seed = global.Next();
                        local = new System.Random(seed);
                    }
                }
            }

            return local.Next();
        }
    }
    public class ThreadSafeTool
    {
    }
}