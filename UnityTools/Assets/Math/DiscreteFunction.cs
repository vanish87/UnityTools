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
        protected List<Tuple<XValue, YValue>> valueMap;
        protected Tuple<XValue, YValue> start;
        protected Tuple<XValue, YValue> end;
        protected XValue h;
        protected int sampleNum;

        public Tuple<XValue, YValue> Start { get => this.start; }
        public Tuple<XValue, YValue> End { get => this.end; }
        public int SampleNum { get => this.sampleNum; }

        private static readonly List<Type> SupportedTypes =
            new List<Type>() {
                typeof(int), typeof(float), typeof(double),
                typeof(float2), typeof(float3), typeof(float4),
                typeof(double2),typeof(double3),typeof(double4)
            };
        private static readonly List<Type> SupportedAnimationTypes =
            new List<Type>() {
                typeof(int), typeof(float), typeof(double)
            };

        public AnimationCurve ToAnimationCurve()
        {
            LogTool.LogAssertIsTrue(SupportedAnimationTypes.Contains(typeof(XValue)), typeof(XValue) + " for XValue is not supported");
            LogTool.LogAssertIsTrue(SupportedAnimationTypes.Contains(typeof(YValue)), typeof(YValue) + " for YValue is not supported");

            var ret = new AnimationCurve();
            foreach(var d in this.valueMap)
            {
                dynamic time = d.Item1;
                dynamic value = d.Item2;
                ret.AddKey(new Keyframe() { time = time, value = value });
            }

            return ret;
        }
        public DiscreteFunction(AnimationCurve from)
        {
            foreach (var key in from.keys)
            {
                dynamic t = key.time;
                dynamic v = key.value;
                this.AddValue(new Tuple<XValue, YValue>(t, v));
            }

            this.start = this.valueMap[0];
            this.end = this.valueMap[this.valueMap.Count-1];
            this.sampleNum = this.valueMap.Count;

            this.InitH();
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

            this.InitH();
            this.InitValues();
        }
        public void ResetValues()
        {
            this.valueMap.Clear();
            this.InitValues();
        }
        public YValue this[int index]
        {
            get 
            {
                var x = math.clamp(index, 0, this.valueMap.Count - 1);
                return this.valueMap[x].Item2;
            }
            set
            {
                var x = math.clamp(index, 0, this.valueMap.Count - 1);
                var old = this.valueMap[x];
                this.valueMap[x] = new Tuple<XValue, YValue>(old.Item1, value);
            }
        }
        public XValue GetValueX(int index)
        {
            var x = math.clamp(index, 0, this.valueMap.Count - 1);
            return this.valueMap[x].Item1;
        }

        public delegate YValue Lerp(YValue from, YValue to, float t);
        public YValue Evaluate(float t, bool clamp = false, Lerp lerp = null)
        {
            dynamic s = this.Start.Item1;
            dynamic e = this.End.Item1;
            dynamic range = e - s;
            dynamic dh = this.h;

            t = clamp? math.clamp(t, s, e):t;

            var index = (t % range) / h;
            var from = Mathf.FloorToInt(index);
            var to = Mathf.CeilToInt(index);
            var yfrom = this[from];
            var yto = this[to];

            return lerp != null ? lerp(yfrom, yto, index - from) : math.lerp(yfrom, yto, index - from);
        }
        protected virtual void InitValues()
        {
            dynamic s = this.Start.Item1;
            dynamic dh = this.h;

            this.AddValue(this.Start);
            for (var i = 1; i < this.sampleNum - 1; ++i)
            {
                this.AddValue(new Tuple<XValue, YValue>(s + i * dh, default(YValue)));
            }
            this.AddValue(this.End);

            LogTool.LogAssertIsTrue(this.valueMap.Count == sampleNum, "Sample size inconstant");
        }
        protected void InitH()
        {
            dynamic s = this.Start.Item1;
            dynamic e = this.End.Item1;
            dynamic range = e - s;

            LogTool.LogAssertIsFalse(range == 0, "range is 0");

            this.h = range / this.sampleNum;
        }
        protected void AddValue(Tuple<XValue, YValue> value)
        {
            this.valueMap.Add(value);
            //this.valueMap.Next.Add(value);
        }

        public YValue Devrivate(int index, XValue h)
        {
            dynamic prev = this[index - 1];
            dynamic next = this[index + 1];
            dynamic dh = h;
            return (prev + next) / (2 * dh);
        }
        public YValue Devrivate2(int index, XValue h)
        {
            dynamic prev = this[index - 1];
            dynamic next = this[index + 1];
            dynamic current = this[index];
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