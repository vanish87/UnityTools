using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Math
{
    public interface IFunctionProvider
    {
        Function<Vector<float>, float> Function { get; }
    }
    public class FunctionVisualizer : MonoBehaviour
    {
        protected Function<Vector<float>, float> function;
        protected DisposableMaterial matrial;

        protected void Start()
        {
            this.function = this.GetComponentInChildren<IFunctionProvider>()?.Function;
            LogTool.LogAssertIsTrue(this.function != null, "Cannot find function");

            this.matrial = new DisposableMaterial(new Material(Shader.Find("Diffuse")));
            this.matrial.Data.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            var meshFiter = this.gameObject.FindOrAddTypeInComponentsAndChildren<MeshFilter>();
            var meshRender = this.gameObject.FindOrAddTypeInComponentsAndChildren<MeshRenderer>();
            meshRender.material = this.matrial;


            var mesh = FunctionTool.GenerateFunctionMesh(this.function);
            meshFiter.mesh = mesh;
        }

        protected void OnDisable()
        {
            this.matrial.Dispose();
        }
    }
}