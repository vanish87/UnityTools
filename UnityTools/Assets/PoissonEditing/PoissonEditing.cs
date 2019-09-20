using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools;
using UnityTools.Rendering;

public class PoissonEditing : MonoBehaviour
{
    [SerializeField] protected Texture2D sourceImage = null;
    [SerializeField] protected Texture2D targetImage = null;
    [SerializeField] protected Texture2D maskImage = null;
    [SerializeField] protected RenderTexture lapTex = null;
    [SerializeField] protected RenderTexture sourceLapTex = null;
    [SerializeField] protected RenderTexture outputImage = null;
    [SerializeField] protected RenderTexture outputImage1 = null;

    [SerializeField] protected RenderTexture gradientX = null;
    [SerializeField] protected RenderTexture gradientY = null;



    [SerializeField] protected ComputeShader blendCS = null;

    protected int kernal = -1;
    protected int solverKernal = -1;

    [SerializeField] protected int count = 0;
    [SerializeField] protected int maxCount = 50000;


    // Start is called before the first frame update
    void Start()
    {
        Assert.IsNotNull(this.sourceImage);
        Assert.IsNotNull(this.targetImage);
        Assert.IsNotNull(this.maskImage);
        Assert.IsNotNull(this.blendCS);

        var desc = new RenderTextureDescriptor();
        desc.width = this.sourceImage.width;
        desc.height = this.sourceImage.height;
        desc.colorFormat = RenderTextureFormat.ARGBFloat;
        desc.enableRandomWrite = true;

        this.outputImage = TextureManager.Create(desc);
        this.outputImage1 = TextureManager.Create(desc);
        this.lapTex = TextureManager.Create(desc);
        this.sourceLapTex = TextureManager.Create(desc);


        this.gradientX = TextureManager.Create(desc);
        this.gradientY = TextureManager.Create(desc);

        this.kernal = this.blendCS.FindKernel("Lap");
        this.solverKernal = this.blendCS.FindKernel("Solver");

        var gx = this.blendCS.FindKernel("GradientX");
        var gy = this.blendCS.FindKernel("GradientY");

        this.blendCS.SetTexture(gx, "_SourceTex", this.sourceImage);
        this.blendCS.SetTexture(gx, "_TargetTex", this.targetImage);
        this.blendCS.SetTexture(gx, "_MaskTex", this.maskImage);
        this.blendCS.SetTexture(gx, "_Result", this.gradientX);
        this.blendCS.Dispatch(gx, this.sourceImage.width, this.sourceImage.height, 1);


        this.blendCS.SetTexture(gy, "_SourceTex", this.sourceImage);
        this.blendCS.SetTexture(gy, "_TargetTex", this.targetImage);
        this.blendCS.SetTexture(gy, "_MaskTex", this.maskImage);
        this.blendCS.SetTexture(gy, "_Result", this.gradientY);
        this.blendCS.Dispatch(gy, this.sourceImage.width, this.sourceImage.height, 1);


        /*
       var lapc = this.blendCS.FindKernel("LapCombine");
        this.blendCS.SetTexture(lapc, "_SourceTex", this.gradientX);
        this.blendCS.SetTexture(lapc, "_TargetTex", this.gradientY);
        this.blendCS.SetTexture(lapc, "_MaskTex", this.maskImage);
        this.blendCS.SetTexture(lapc, "_LapTex", this.lapTex);
        this.blendCS.Dispatch(lapc, this.sourceImage.width, this.sourceImage.height, 1);
        Graphics.Blit(this.sourceImage, this.outputImage);
       */

        this.blendCS.SetTexture(this.kernal, "_SourceTex", this.sourceImage);
        this.blendCS.SetTexture(this.kernal, "_TargetTex", this.targetImage);
        this.blendCS.SetTexture(this.kernal, "_MaskTex", this.maskImage);
        this.blendCS.SetTexture(this.kernal, "_Result", this.outputImage);
        this.blendCS.SetTexture(this.kernal, "_LapTex", this.lapTex);
        this.blendCS.SetTexture(this.kernal, "_SourceLapTex", this.sourceLapTex);
        this.blendCS.Dispatch(this.kernal, this.sourceImage.width, this.sourceImage.height, 1);


        //Graphics.Blit(this.sourceImage, this.outputImage);


        this.StartCoroutine(Solver());
    }

    IEnumerator Solver()
    {
        while (count++ < maxCount)
        {
            //yield return new WaitForEndOfFrame();
            //var size = new Vector2(1, 1);
            this.blendCS.SetTexture(this.solverKernal, "_SourceTex", this.outputImage);
            this.blendCS.SetTexture(this.solverKernal, "_SourceLapTex", this.sourceLapTex);
            this.blendCS.SetTexture(this.solverKernal, "_LapTex", this.lapTex);
            this.blendCS.SetTexture(this.solverKernal, "_MaskTex", this.maskImage);
            this.blendCS.SetTexture(this.solverKernal, "_Result", this.outputImage1);
            //this.blendCS.SetVector("_SizeOffset", new Vector4(size.x, 0, 0, size.y));

            this.blendCS.Dispatch(this.solverKernal, this.sourceImage.width, this.sourceImage.height, 1);

            var temp = this.outputImage;
            this.outputImage = this.outputImage1;
            this.outputImage1 = temp;
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnGUI()
    {
        if(this.outputImage != null)
        {
            GUI.DrawTexture(new Rect(10, 10, this.outputImage.width, this.outputImage.height), this.outputImage);
            GUI.DrawTexture(new Rect(10 + this.outputImage.width, 10, 256, 128), this.targetImage);
            GUI.DrawTexture(new Rect(10 + this.outputImage.width, 138, 256, 128), this.maskImage);
            
            GUI.DrawTexture(new Rect(10 + this.outputImage.width, 266, 256, 256), this.lapTex);

            GUILayout.BeginArea(new Rect(10, 10, 100, 100));
            GUILayout.TextField(this.count.ToString());
            GUILayout.EndArea();
        }
    }
}
