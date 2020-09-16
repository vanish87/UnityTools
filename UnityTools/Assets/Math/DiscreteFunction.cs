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
        public enum EvaluateMode
        {
            Repeat,
            Clamp,
        }
        protected FiniteDifferenceType fdType = FiniteDifferenceType.Central;
        protected List<Tuple<XValue, YValue>> valueMap;

        public Tuple<XValue, YValue> Start { get => this.valueMap.Count > 0 ? this.valueMap[0] : null; }
        public Tuple<XValue, YValue> End { get => this.valueMap.Count > 0 ? this.valueMap[this.valueMap.Count - 1] : null; }
        public int SampleNum { get => this.valueMap.Count; }

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
            foreach (var d in this.valueMap)
            {
                dynamic time = d.Item1;
                dynamic value = d.Item2;
                dynamic dev = this.Derivate(index);
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
            foreach (var y in this.valueMap)
            {
                ret[count++] = y.Item2;
            }
            return ret;
        }
        public void Append(XValue xValue, YValue yValue)
        {
            this.valueMap.Add(new Tuple<XValue, YValue>(xValue, yValue));
        }

        public DiscreteFunction(AnimationCurve from)
        {
            this.valueMap = new List<Tuple<XValue, YValue>>();

            foreach (var key in from.keys)
            {
                dynamic t = key.time;
                dynamic v = key.value;
                this.Append(t, v);
            }
        }

        public DiscreteFunction(XValue start, XValue end, Vector<YValue> from)
        {
            this.valueMap = new List<Tuple<XValue, YValue>>();

            dynamic s = start;
            dynamic e = end;
            int count = from.Size;
            var h = (e - s) / count;
            for (var i = 0; i < count; ++i)
            {
                this.Append(s + h * i, from[i]);
            }
        }

        public DiscreteFunction(Tuple<XValue, YValue> start = null, Tuple<XValue, YValue> end = null, int sampleNum = 1)
        {
            LogTool.LogAssertIsTrue(sampleNum > 0, "Sample size should none 0");
            LogTool.LogAssertIsTrue(SupportedTypes.Contains(typeof(XValue)), typeof(XValue) + " for XValue is not supported");

            start = start ?? new Tuple<XValue, YValue>(default, default);
            end = end ?? new Tuple<XValue, YValue>(default, default);

            this.valueMap = new List<Tuple<XValue, YValue>>();
            this.InitValues(start, end, sampleNum);
        }
        public void ResetValues()
        {
            for (var i = 0; i < this.valueMap.Count; ++i)
            {
                this.valueMap[i] = new Tuple<XValue, YValue>(this.valueMap[i].Item1, default(YValue));
            }
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
        public YValue Evaluate(float t, EvaluateMode eMode = EvaluateMode.Clamp, Lerp lerp = null)
        {
            dynamic s = this.Start.Item1;
            dynamic e = this.End.Item1;
            dynamic range = e - s;

            t = eMode == EvaluateMode.Clamp ? math.clamp(t, s, e) : t;

            var x = (t % range);
            var index = this.GetIndexForXvalue(x);
            var from = index - 1;
            var to = index;
            var yfrom = this[from];
            var yto = this[to];

            return lerp != null ? lerp(yfrom, yto, index - from) : math.lerp(yfrom, yto, index - from);
        }

        public YValue Derivate(int index)
        {
            dynamic prev = this[index - 1];
            dynamic next = this[index + 1];
            dynamic current = this[index];
            dynamic dh = this.GetH(index);

            switch (this.fdType)
            {
                case FiniteDifferenceType.Forward: return (next - current) / dh;//dh is correct
                case FiniteDifferenceType.Central: return (next - prev) / (2 * dh);
                case FiniteDifferenceType.Backward: return (current - prev) / dh;//dh is correct
                default: return default;
            }
        }
        public YValue Derivate2(int index)
        {
            //TODO add FiniteDifferenceType switch for dev2
            dynamic prev = this[index - 1];
            dynamic next = this[index + 1];
            dynamic current = this[index];
            dynamic dh = this.GetH(index);
            return (prev + next - (2 * current)) / (dh * dh);
        }
        protected void InitValues(Tuple<XValue, YValue> start, Tuple<XValue, YValue> end, int count)
        {
            dynamic s = start.Item1;
            dynamic e = end.Item1;
            dynamic h = (e - s) / count;

            for (var i = 0; i < count; ++i)
            {
                this.Append(s + i * h, default(YValue));
            }

            this.valueMap[0] = start;
            this.valueMap[count - 1] = end;
        }
        protected XValue GetH(int index)
        {
            dynamic px = this.GetValueX(index - 1);
            dynamic nx = this.GetValueX(index + 1);
            dynamic dh = (nx - px) / 2f;

            return dh;
        }
        protected int GetIndexForXvalue(XValue x)
        {
            var start = 0;
            var end = this.valueMap.Count - 1;
            dynamic xv = x;
            while (start <= end)
            {
                var mid = start + (end - start) / 2;
                dynamic midv = this.valueMap[mid].Item1;
                if (midv < x) start = mid + 1;
                else end = mid - 1;
            }
            return start;
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
        public virtual void RandomValues(float start = 0, float end = 1)
        {
            for (var i = 0; i < this.valueMap.Count; ++i)
            {
                var n = this.valueMap[i].Item1;
                this.valueMap[i] = new Tuple<X, float>(n, math.lerp(start, end, ThreadSafeRandom.NextFloat()));
            }
        }
    }
}