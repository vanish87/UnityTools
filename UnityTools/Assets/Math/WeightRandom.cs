using System.Collections.Generic;
using System.Linq;
using UnityTools.Debuging;

namespace UnityTools.Math
{
	public class WeightRandom
	{
		protected List<int> sum = new List<int>();
		public WeightRandom(List<int> weights)
		{
			this.sum.Clear();
			foreach (var i in Enumerable.Range(0, weights.Count))
			{
				LogTool.AssertIsTrue(weights[i] > 0);
				var s = weights[i] + (i > 0 ? this.sum[i - 1] : 0);
				this.sum.Add(s);
			}
		}

		public int Random()
		{
			LogTool.AssertIsTrue(this.sum.Count > 0);

			var total = this.sum.Last();
			var rand = UnityEngine.Random.Range(0, total);

			var l = 0;
			var r = this.sum.Count;
			while (l < r)
			{
				var mid = l + (r - l) / 2;
				if (this.sum[mid] < rand) l = mid + 1;
				else r = mid;
			}

			return r;
		}

	}

}