using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Algorithm;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Math
{
    [Serializable]
    public class DiscreteFunction<XValue, YValue>
    {
        public enum FiniteDifferenceType
        {
            Central,
            Forward,
            Backward,
        }
        protected FiniteDifferenceType fdType = FiniteDifferenceType.Central;
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
            var index = 0;
            foreach(var d in this.valueMap)
            {
                dynamic time = d.Item1;
                dynamic value = d.Item2;
                dynamic dev = this.Devrivate(index);
                var key = new Keyframe() { time = time, value = value };
                key.weightedMode = WeightedMode.None;
                key.inTangent = key.outTangent = dev;
                ret.AddKey(key);

                index++;
            }

            return ret;
        }
        public Vector<XValue> ToXVector()
        {
            var ret = new Vector<XValue>(this.SampleNum);
            var count = 0;
            foreach (var y in this.valueMap)
            {
                ret[count++] = y.Item1;
            }
            return ret;
        }
        public Vector<YValue> ToYVector()
        {
            var ret = new Vector<YValue>(this.SampleNum);
            var count = 0;
            foreach(var y in this.valueMap)
            {
                ret[count++] = y.Item2;
            }
            return ret;
        }
        public DiscreteFunction(AnimationCurve from)
        {
            this.valueMap = new List<Tuple<XValue, YValue>>();
            this.sampleNum = from.keys.Length;

            foreach (var key in from.keys)
            {
                dynamic t = key.time;
                dynamic v = key.value;
                this.AddValue(new Tuple<XValue, YValue>(t, v));
            }

            this.start = this.valueMap[0];
            this.end = this.valueMap[this.valueMap.Count-1];

            this.InitH();
        }

        public DiscreteFunction(XValue start, XValue end, Vector<YValue> from)
        {
            this.valueMap = new List<Tuple<XValue, YValue>>();
            this.start  = new Tuple<XValue, YValue>(start, from[0]);
            this.end    = new Tuple<XValue, YValue>(end, from[from.Size-1]);
            this.sampleNum = from.Size;

            this.InitH();
            this.InitValues(from);
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

            t = clamp? math.clamp(t, s, e):t;

            var index = (t % range) / h;
            var from = Mathf.FloorToInt(index);
            var to = Mathf.CeilToInt(index);
            var yfrom = this[from];
            var yto = this[to];

            return lerp != null ? lerp(yfrom, yto, index - from) : math.lerp(yfrom, yto, index - from);
        }
        protected virtual void InitValues(Vector<YValue> y = null)
        {
            dynamic s = this.Start.Item1;
            dynamic dh = this.h;

            this.AddValue(this.Start);
            for (var i = 1; i < this.sampleNum - 1; ++i)
            {
                this.AddValue(new Tuple<XValue, YValue>(s + i * dh, y != null ? y[i] : default(YValue)));
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
            if (this.valueMap.Count < this.sampleNum)
            {
                this.valueMap.Add(value);
                //this.valueMap.Next.Add(value);
            }
            else
            {
                LogTool.Log("Sample num is bigger than " + this.sampleNum, LogLevel.Warning);
            }
        }

        public YValue Devrivate(int index)
        {
            return this.Devrivate(index, this.h);
        }
        public YValue Devrivate2(int index)
        {
            return this.Devrivate2(index, this.h);
        }
        public YValue Devrivate(int index, XValue h)
        {
            dynamic prev = this[index - 1];
            dynamic next = this[index + 1];
            dynamic current = this[index];
            dynamic dh = h;

            switch (this.fdType)
            {
                case FiniteDifferenceType.Forward:  return (next - current) / dh;//dh is correct
                case FiniteDifferenceType.Central:  return (next - prev) / (2 * dh);
                case FiniteDifferenceType.Backward: return (current - prev) / dh;//dh is correct
                default: return default;
            }
        }
        public YValue Devrivate2(int index, XValue h)
        {
            //TODO add FiniteDifferenceType switch for dev2
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
        public X2FDiscreteFunction(AnimationCurve from) : base(from) { }
        public X2FDiscreteFunction(X start, X end, Vector<float> from) : base(start, end, from) { }
        public X2FDiscreteFunction(Tuple<X, float> start, Tuple<X, float> end, int sampleNum) : base(start, end, sampleNum)
        {

        }
        public virtual void RandomValues()
        {
            for (var i = 0; i < this.valueMap.Count; ++i)
            {
                var n = this.valueMap[i].Item1;
                this.valueMap[i] = new Tuple<X, float>(n, ThreadSafeRandom.NextFloat());
            }
        }
    }
}