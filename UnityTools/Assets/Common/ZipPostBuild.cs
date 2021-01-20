using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    #if UNITY_EDITOR
    public class ZipPostBuild : UnityEditor.Build.IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            // if(report.summary.result != BuildResult.Succeeded) return;

            var launchers = ObjectTool.FindAllObject<ILauncher>();
            foreach (var l in launchers)
            {
                if(l.IsGlobal == false) continue;
                var env = l.RunTime;
                if (env.appSetting.zipAfterBuild)
                {
                    var outputPath = report.summary.outputPath;
                    // FileTool.ReplaceOrRename(outputPath + "new", outputPath);

                    
                    // System.IO.Compression.ZipFile.CreateFromDirectory(outputPath , outputPath+".zip");

                }
            }
        }

        
    }
    #endif
}
