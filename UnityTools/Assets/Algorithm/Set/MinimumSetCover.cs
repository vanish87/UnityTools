using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UnityTools.Algorithm
{
    public class MinimumSetCover
    {
        public interface ICost
        {
            float Cost { get; }
        }
        public delegate ISet<T> GetCandidate<T>(ISet<T> universe, ISet<T> i, Dictionary<ISet<T>, float> costMap);
        public static ISet<ISet<T>> GetMinimunSetCover<T>(ISet<T> universe, ISet<ISet<T>> from, GetCandidate<T> getCandidate = null) where T : ICost
        {
            var ret = new HashSet<ISet<T>>();
            var i = new HashSet<T>();
            var costMap = new Dictionary<ISet<T>, float>();
            foreach(var s in from)
            {
                costMap.Add(s, s.Select(se => se.Cost).Aggregate((r, c) => r + c));
            }
            
            while(!i.SetEquals(universe))
            {
                var candidate = getCandidate != null ? getCandidate(universe, i, costMap) : costMap.OrderBy(c => c.Value / (c.Key.Count - i.Count)).First().Key;
                costMap.Remove(candidate);
                i.UnionWith(candidate);

                ret.Add(candidate);
            }

            return ret;
        }
    }
}