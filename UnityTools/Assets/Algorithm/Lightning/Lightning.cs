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
        [System.Serializable]
        public class LightningCSContainer : ComputeShaderParameterContainer
        {
            public ComputeShaderParameterTexture potentialTex = new ComputeShaderParameterTexture("_Potential");
            public ComputeShaderParameterTexture potentialOutputTex = new ComputeShaderParameterTexture("_PotentialOutput");
            public ComputeShaderParameterVector textureSize = new ComputeShaderParameterVector("_TextureSize");
            public ComputeShaderParameterTexture potentialBoundaryTex = new ComputeShaderParameterTexture("_PotentialBoundary");
            public ComputeShaderParameterTexture potentialBoundaryInputTex = new ComputeShaderParameterTexture("_PotentialBoundaryInput");

            public ComputeShaderParameterBuffer adjacentWeight = new ComputeShaderParameterBuffer("_AdjacentWeight");

            public ComputeShaderParameterVector targetPixel = new ComputeShaderParameterVector("_TargetPixel");

            public ComputeShaderParameterTexture displayTex = new ComputeShaderParameterTexture("_DisplayTex");
            public ComputeShaderParameterTexture dartLeaderTex = new ComputeShaderParameterTexture("_DartLeaderTex");
            public ComputeShaderParameterFloat chargeDensity = new ComputeShaderParameterFloat("_ChargeDensity", 1000 / (4 * Mathf.PI));


            public ComputeShaderParameterTexture obstacleTex = new ComputeShaderParameterTexture("_ObstacleTex");

        }

        public enum Stage
        {
            Init,
            DartLeader
        }

        [SerializeField] protected Stage stage = Stage.Init;
        [SerializeField, Range(0,10)] protected int ita = 1;
        [SerializeField] protected int iteration = 100;
        [SerializeField] protected int2 textureSize = new int2(256, 256);
        [SerializeField] protected CricleRT electricalPotentialGrid;
        [SerializeField] protected ComputeShader cs;
        [SerializeField] protected ComputeShaderDispatcher dispatcher;
        [SerializeField] protected LightningCSContainer parameter = new LightningCSContainer();

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

            this.dispatcher.AddParameter("InitDart", this.parameter);
            this.dispatcher.AddParameter("InitDartLeaderChargeDensity", this.parameter);
            this.dispatcher.AddParameter("UpdateDartLeader", this.parameter);
            

            this.parameter.potentialBoundaryTex.Value = TextureManager.Create(desc);
            this.parameter.dartLeaderTex.Value = TextureManager.Create(desc);

            var count = this.textureSize.x * this.textureSize.y;
            this.parameter.adjacentWeight.Value = new ComputeBuffer(count, Marshal.SizeOf<float>());


            this.parameter.potentialBoundaryInputTex.Value = TextureManager.Create(this.textureSize.x, this.textureSize.y, TextureFormat.RGFloat, false, true);
            (this.parameter.potentialBoundaryInputTex.Value as Texture2D).Apply();

            this.parameter.displayTex.Value = TextureManager.Create(this.textureSize.x, this.textureSize.y, TextureFormat.ARGB32);

            this.parameter.obstacleTex.Value = TextureManager.Create(this.textureSize.x, this.textureSize.y, TextureFormat.RFloat, false, true);
        }

        protected void OnDisable()
        {
            this.electricalPotentialGrid.Dispose();
            this.parameter.ReleaseBuffer();
            this.parameter.ReleaseTexture();
        }

        protected void InitDart()
        {
            this.dispatcher.Dispatch("InitDart", this.textureSize.x, this.textureSize.y);
        }

        protected void Init()
        {
            this.ClearTexture(this.parameter.displayTex.Value as Texture2D);

            this.parameter.potentialTex.Value = this.electricalPotentialGrid.Current;
            this.dispatcher.Dispatch("Init", this.textureSize.x, this.textureSize.y);
        }

        protected void ClearTexture(Texture2D tex)
        {
            for (int i = 0; i < this.textureSize.x; ++i)
            {
                for (int j = 0; j < this.textureSize.y; ++j)
                {
                    tex.SetPixel(i, j, Color.black);
                }
            }

            tex.Apply();
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

            if (stage == Stage.Init)
            {
                this.dispatcher.Dispatch("JacobiStep", this.textureSize.x, this.textureSize.y);
            }
            else
            {
                this.dispatcher.Dispatch("UpdateDartLeader", this.textureSize.x, this.textureSize.y);
            }

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

            float rand = UnityEngine.Random.Range(0, sum[sum.Length-1]);

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

            var t2d = this.parameter.displayTex.Value as Texture2D;
            //this.pointList.Add(idx);
            t2d.SetPixel((int)idx.x, (int)idx.y, Color.white);
            t2d.Apply();
        }

        void Start()
        {
            LogTool.Log("Move this to a separated repo", LogLevel.Warning);

            this.InitDart();
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKey(KeyCode.Mouse0))
            {
                var rect = new Rect(20, 25 + this.textureSize.x * 2, this.textureSize.y, this.textureSize.y);
                var mpos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                if(rect.Contains(mpos))
                {
                    var t2d = this.parameter.potentialBoundaryInputTex.Value as Texture2D;
                    mpos = mpos - rect.min;
                    var size = 5;
                    for (var i = -size; i < size; ++i)
                    {
                        for (var j = -size; j < size; ++j)
                        {
                            var pos = new Vector2Int((int)mpos.x + i, this.textureSize.y - (int)mpos.y + j);
                            pos.Clamp(Vector2Int.zero, new Vector2Int(this.textureSize.x, this.textureSize.y));
                            t2d.SetPixel(pos.x, pos.y, Color.red);
                        }
                    }
                    t2d.Apply();
                }
            }
            if (Input.GetKey(KeyCode.Mouse1))
            {
                var isObstacle = Input.GetKey(KeyCode.O);
                var brect = new Rect(20, 25 + this.textureSize.x * 2, this.textureSize.x, this.textureSize.y);
                var orect = new Rect(25 + this.textureSize.x, 20, this.textureSize.x, this.textureSize.y);
                var rect = isObstacle ? orect : brect;
                var mpos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                if (rect.Contains(mpos))
                {
                    var t2d = (isObstacle?
                        this.parameter.obstacleTex.Value:
                        this.parameter.potentialBoundaryInputTex.Value) as Texture2D;
                    mpos = mpos - rect.min;
                    var size = 5;
                    for(var i = -size; i < size; ++i)
                    {
                        for(var j = -size; j < size; ++j)
                        {
                            var pos = new Vector2Int((int)mpos.x + i, this.textureSize.y - (int)mpos.y + j);
                            pos.Clamp(Vector2Int.zero, new Vector2Int(this.textureSize.x, this.textureSize.y));
                            t2d.SetPixel(pos.x, pos.y, isObstacle?Color.red:Color.green);
                        }
                    }
                    t2d.Apply();
                }
            }
            if(Input.GetKeyDown(KeyCode.N))
            {
                this.stage = this.stage == Stage.Init ? Stage.DartLeader : Stage.Init;
            }
            if(Input.GetKeyDown(KeyCode.D))
            {
                this.dispatcher.Dispatch("InitDartLeaderChargeDensity", this.textureSize.x, this.textureSize.y);
            }

            if (Input.GetKey(KeyCode.R))
            {
                this.Init();
            }
            if(Input.GetKeyDown(KeyCode.C))
            {
                this.ClearTexture(this.parameter.potentialBoundaryInputTex.Value as Texture2D);
                if (Input.GetKey(KeyCode.O))
                {
                    this.ClearTexture(this.parameter.obstacleTex.Value as Texture2D);
                }
            }
            var count = 0;
            while (count++ < this.iteration)
            {
                this.JacobiStep();
            }

            this.GrowthStep();
        }

        protected void OnGUI()
        {
            GUI.DrawTexture(new Rect(20, 0, this.textureSize.x, this.textureSize.y), this.electricalPotentialGrid.Current, ScaleMode.ScaleToFit);
            GUI.DrawTexture(new Rect(20, 10 + this.textureSize.x, this.textureSize.y, this.textureSize.y), this.parameter.potentialBoundaryTex, ScaleMode.ScaleToFit);
            GUI.DrawTexture(new Rect(20, (10 + this.textureSize.x)*2, this.textureSize.y, this.textureSize.y), this.parameter.potentialBoundaryInputTex, ScaleMode.ScaleToFit);
            GUI.DrawTexture(new Rect(20, (10 + this.textureSize.x)*3, this.textureSize.y, this.textureSize.y), this.parameter.dartLeaderTex, ScaleMode.ScaleToFit);
            GUI.DrawTexture(new Rect(25 + this.textureSize.x, 0, this.textureSize.y, this.textureSize.y), this.parameter.obstacleTex, ScaleMode.ScaleToFit);
            //GUI.DrawTexture(new Rect(20 + this.textureSize.x, 20, this.textureSize.x, this.textureSize.y), this.potentialLapTex, ScaleMode.ScaleToFit);


            GUI.DrawTexture(new Rect(300, 300, 1000-300, 1000-300), this.parameter.displayTex, ScaleMode.ScaleToFit);
        }
    }
}
