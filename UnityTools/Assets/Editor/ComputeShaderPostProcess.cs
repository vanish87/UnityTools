using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class ComputeShaderPostProcess : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("MyCustomBuildProcessor.OnPostprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
    }
}