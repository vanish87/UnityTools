using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Math
{
    [Serializable]

    public class DiscreteFunction<XValue, YValue>
    {
        [SerializeField] protected List<Tuple<XValue, YValue>> valueMap;
        [SerializeField] protected Tuple<XValue, YValue> start;
        [SerializeField] protected Tuple<XValue, YValue> end;
        [SerializeField] protected int sampleNum;

        public Tuple<XValue, YValue> Start { get => this.start; }
        public Tuple<XValue, YValue> End { get => this.end; }
        public int SampleNum { get => this.sampleNum; }


        private static readonly List<Type> SupportedTypes =
            new List<Type>() {
                typeof(int), typeof(float), typeof(double),
                typeof(float2), typeof(float3), typeof(float4),
                typeof(double2),typeof(double3),typeof(double4)
            };

        protected virtual void InitValues()
        {
            dynamic s = this.start.Item1;
            dynamic e = this.end.Item1;

            var h = (e - s) / this.sampleNum;

            this.AddValue(this.start);
            for (var i = 1; i < this.sampleNum - 1; ++i)
            {
                this.AddValue(new Tuple<XValue, YValue>(s + i * h, default(YValue)));
            }
            this.AddValue(this.end);

            LogTool.LogAssertIsTrue(this.valueMap.Count == sampleNum, "Sample size inconstant");
        }


        public DiscreteFunction(Tuple<XValue, YValue> start = null, Tuple<XValue, YValue> end = null, int sampleNum = 1)
        {
            LogTool.LogAssertIsTrue(sampleNum > 0, "Sample size should none 0");
            LogTool.LogAssertIsTrue(SupportedTypes.Contains(typeof(XValue)), typeof(XValue) + " for XValue is not supported");

            start = start ?? new Tuple<XValue, YValue>(default, default);
            end = end ?? new Tuple<XValue, YValue>(default, default);

            //this.valueMap = new CricleData<List<Tuple<XValue, YValue>>, int>(2);
            this.valueMap = new List<Tuple<XValue, YValue>>();
            this.start = start;
            this.end = end;
            this.sampleNum = sampleNum;

            this.InitValues();
        }
        public void ResetValues()
        {
            this.valueMap.Clear();
            this.InitValues();
        }
        
        public XValue GetValueX(int index)
        {
            var x = math.clamp(index, 0, this.valueMap.Count - 1);
            return this.valueMap[x].Item1;
        }
        public YValue GetValueY(int index)
        {
            var x = math.clamp(index, 0, this.valueMap.Count - 1);
            return this.valueMap[x].Item2;
        }

        public void SetValueY(int index, YValue value)
        {
            var x = math.clamp(index, 0, this.valueMap.Count - 1);
            var old = this.valueMap[x];
            this.valueMap[x] = new Tuple<XValue, YValue>(old.Item1, value);
        }
        public YValue Evaluate(float t)
        {
            dynamic s = this.start.Item1;
            dynamic e = this.end.Item1;

            var range = e - s;
            LogTool.LogAssertIsFalse(range == 0, "range is 0");

            if (range == 0) return default;
            var h = range / this.valueMap.Count;

            var index = (t % range) / h;
            var from = Mathf.FloorToInt(index);
            var to = Mathf.CeilToInt(index);
            var yfrom = this.EvaluateIndex(from);
            var yto = this.EvaluateIndex(to);

            return math.lerp(yfrom, yto, index - from);
        }

        public YValue EvaluateIndex(int index)
        {
            var x = math.clamp(index, 0, this.valueMap.Count - 1);
            return this.valueMap[x].Item2;
        }

        protected void AddValue(Tuple<XValue, YValue> value)
        {
            this.valueMap.Add(value);
            //this.valueMap.Next.Add(value);
        }

        public YValue Devrivate(int index, XValue h)
        {
            dynamic prev = this.EvaluateIndex(index - 1);
            dynamic next = this.EvaluateIndex(index + 1);
            dynamic dh = h;
            return (prev + next) / (2 * dh);
        }
        public YValue Devrivate2(int index, XValue h)
        {
            dynamic prev = this.EvaluateIndex(index - 1);
            dynamic next = this.EvaluateIndex(index + 1);
            dynamic current = this.EvaluateIndex(index);
            dynamic dh = h;
            return (prev + next - (2 * current)) / (dh * dh);
        }
    }

    [Serializable]
    public class X2FDiscreteFunction<X> : DiscreteFunction<X, float>
    {
        public X2FDiscreteFunction(Tuple<X, float> start, Tuple<X, float> end, int sampleNum) : base(start, end, sampleNum)
        {

        }
        public virtual void RandomValues()
        {
            for (var i = 0; i < this.valueMap.Count; ++i)
            {
                var n = this.valueMap[i].Item1;
                this.valueMap[i] = new Tuple<X, float>(n, UnityEngine.Random.value);
            }
        }
    }
}