using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

public class CommandExample : MonoBehaviour
{
    protected void Output(string message)
    {
        Debug.Log("Output/Error " + message);
    }
    protected void Exit(string cmd, string args, int exitCode)
    {
        Debug.Log("Exit " + exitCode);
    }
    // Use this for initialization
    void Start ()
    {
        var command = new RunCommand.CommandInfo();
        command.exe = "cmd.exe";
        command.args = "/C print.bat";
        command.workingDirectory = Application.streamingAssetsPath;
        command.async = true;
        command.exit = this.Exit;
        command.output = this.Output;
        command.enableLogging = true;
        RunCommand.RunCommandInProcess(command);
    }

    protected void RunBuildAssetBundle()
    {
        var exe = "C:\\Program Files\\Unity2017.4.3f1\\Editor\\Unity.exe";
        var projectPath = "C:\\UnityTools\\UnityTools";
        var logPath = "C:\\logFile";
        var functionName = "BuildAssetBundle.BuildToAssets";

        var logFile = logPath + "\\buildAssets_" + string.Format("{0:yyyyMMddHmmss}", System.DateTime.Now) + ".log";

        var args = string.Format("-force-free -quit -batchmode -projectPath {0} -executeMethod {1} -logFile {2}", projectPath, functionName, logFile);

        var command = new RunCommand.CommandInfo();
        command.exe = exe;
        command.args = args;
        command.workingDirectory = Application.streamingAssetsPath;
        command.async = false;
        command.exit = this.Exit;
        command.output = this.Output;
        command.enableLogging = true;
        RunCommand.RunCommandInProcess(command);
    }

    
}
