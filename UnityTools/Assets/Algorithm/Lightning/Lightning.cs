using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.ComputeShaderTool;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Algorithm
{    
    public class Lightning : MonoBehaviour
    {

        public class LightningCSContainer : ComputeShaderParameterContainer
        {
            public ComputeShaderParameterTexture potentialTex = new ComputeShaderParameterTexture("_Potential");
            public ComputeShaderParameterTexture potentialOutputTex = new ComputeShaderParameterTexture("_PotentialOutput");
            public ComputeShaderParameterVector textureSize = new ComputeShaderParameterVector("_TextureSize");
            public ComputeShaderParameterTexture potentialBoundaryTex = new ComputeShaderParameterTexture("_PotentialBoundary");

            public ComputeShaderParameterBuffer adjacentWeight = new ComputeShaderParameterBuffer("_AdjacentWeight");

            public ComputeShaderParameterVector targetPixel = new ComputeShaderParameterVector("_TargetPixel");


        }

        [SerializeField] protected int ita = 1;
        [SerializeField] protected int2 textureSize = new int2(256, 256);
        [SerializeField] protected CricleRT electricalPotentialGrid;
        [SerializeField] protected ComputeShader cs;
        [SerializeField] protected ComputeShaderDispatcher dispatcher;
        [SerializeField] protected LightningCSContainer parameter = new LightningCSContainer();


        [SerializeField] protected List<Vector2> pointList = new List<Vector2>();
        [SerializeField] protected Texture2D displayTex;

        protected void OnEnable()
        {
            var desc = new RenderTextureDescriptor(this.textureSize.x, this.textureSize.y, RenderTextureFormat.RFloat, 0, 0);
            desc.enableRandomWrite = true;
            desc.sRGB = false;
            this.electricalPotentialGrid = new CricleRT(2, desc);

            this.dispatcher = new ComputeShaderDispatcher(this.cs);
            this.dispatcher.AddParameter("Init", this.parameter);
            this.dispatcher.AddParameter("JacobiStep", this.parameter);
            this.dispatcher.AddParameter("WeightSum", this.parameter);
            this.dispatcher.AddParameter("SetBoundary", this.parameter);

            this.parameter.potentialBoundaryTex.Value = TextureManager.Create(desc);

            var count = this.textureSize.x * this.textureSize.y;
            this.parameter.adjacentWeight.Value = new ComputeBuffer(count, Marshal.SizeOf<float>());


            this.pointList.Clear();

            this.displayTex = TextureManager.Create(this.textureSize.x, this.textureSize.y, TextureFormat.ARGB32);
            for (int i = 0; i < this.textureSize.x; ++i)
            {
                for (int j = 0; j < this.textureSize.y; ++j)
                {
                    this.displayTex.SetPixel(i, j, Color.black);
                }
            }

            this.displayTex.Apply();
        }

        protected void OnDisable()
        {
            this.electricalPotentialGrid.Dispose();
            this.parameter.potentialBoundaryTex.Value.DestoryObj();
            this.parameter.adjacentWeight.Release();

            this.displayTex.DestoryObj();
        }

        protected void Init()
        {
            this.parameter.potentialTex.Value = this.electricalPotentialGrid.Current;
            this.dispatcher.Dispatch("Init", this.textureSize.x, this.textureSize.y);
        }

        /*protected void CalLap()
        {
            this.parameter.potentialTex.Value = this.electricalPotentialGrid.Current;
            this.parameter.potentialLapTex.Value = this.potentialLapTex;

            this.dispatcher.Dispatch("Lap", this.textureSize.x, this.textureSize.y);
        }*/

        protected void JacobiStep()
        {
            this.parameter.potentialTex.Value = this.electricalPotentialGrid.Current;
            this.parameter.potentialOutputTex.Value = this.electricalPotentialGrid.Next;
            this.parameter.textureSize.Value = new Vector4(this.textureSize.x, this.textureSize.y);

            this.dispatcher.Dispatch("JacobiStep", this.textureSize.x, this.textureSize.y);

            this.electricalPotentialGrid.MoveToNext();

        }

        protected void GrowthStep()
        {
            //float[] sum = { 0 };
            //this.parameter.adjacentWeightSum.Value.SetData(sum);

            this.dispatcher.Dispatch("WeightSum", this.textureSize.x, this.textureSize.y);

            float[] weights = new float[this.textureSize.x * this.textureSize.y];
            float[] sum = new float[this.textureSize.x * this.textureSize.y];
            this.parameter.adjacentWeight.Value.GetData(weights);

            sum[0] = Mathf.Pow(weights[0], ita);
            for (var i = 1; i < weights.Length; ++i)
            {
                var w = Mathf.Pow(weights[i], ita);
                sum[i] = sum[i-1] + w;
            }

            float rand = UnityEngine.Random.value * sum[sum.Length-1];

            int left = 0;
            int right = sum.Length - 1;
            while (left < right)
            {
                int mid = left + (right - left) / 2;
                if (sum[mid] < rand) left = mid + 1;
                else right = mid;
            }

            var idx = new Vector4(right % this.textureSize.x, right / this.textureSize.x);

            this.parameter.targetPixel.Value =idx;
            this.dispatcher.Dispatch("SetBoundary", this.textureSize.x, this.textureSize.y);

            //this.pointList.Add(idx);
            this.displayTex.SetPixel((int)idx.x, (int)idx.y, Color.white);
            this.displayTex.Apply();
        }

        void Start()
        {
            LogTool.Log("Move this to a separated repo", LogLevel.Warning);

            this.Init();
        }

        // Update is called once per frame
        void Update()
        {
            var count = 0;
            while (count++ < 1000)
            //if(Input.GetKey(KeyCode.S))
            {
                this.JacobiStep();
            }

            this.GrowthStep();
        }

        protected void OnDrawGizmos()
        {
            for(var i = 0; i < this.pointList.Count; ++i)
            {
                Gizmos.DrawSphere(this.pointList[i], 1);
            }
        }

        protected void OnGUI()
        {
            GUI.DrawTexture(new Rect(20, 20, this.textureSize.x, this.textureSize.y), this.electricalPotentialGrid.Current, ScaleMode.ScaleToFit);
            GUI.DrawTexture(new Rect(20, 25 + this.textureSize.x, this.textureSize.y, this.textureSize.y), this.parameter.potentialBoundaryTex, ScaleMode.ScaleToFit);
            //GUI.DrawTexture(new Rect(20 + this.textureSize.x, 20, this.textureSize.x, this.textureSize.y), this.potentialLapTex, ScaleMode.ScaleToFit);


            GUI.DrawTexture(new Rect(200, 200, 1024-256, 1024-256), this.displayTex, ScaleMode.ScaleToFit);
        }
    }
}
