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

    // Update is called once per frame
    void Update () {
		
	}
}
