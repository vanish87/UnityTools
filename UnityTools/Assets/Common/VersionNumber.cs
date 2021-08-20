using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityTools.Common
{

	[System.Serializable]
	public class VersionInfo
	{
		[Header("Version.txt in StreamingAssets Folder")]
		public int major = 0;
		public int minor = 0;
		[Attributes.DisableEdit] public int build = 0;
		public string comment;
		[Attributes.DisableEdit] public string buildTime;
		[Attributes.DisableEdit] public string buildDeviceName;
		[Attributes.DisableEdit] public string lastCommit;

	}

    #if UNITY_EDITOR
	public class VersionNumber : UnityEditor.Build.IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;
		private readonly string fileName = "Version.xml";
		private string gitCommit;
		private int gitCommitCount = 0;

		public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
		{
			// if(report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) return;

			var launchers = ObjectTool.FindAllObject<ILauncher>();
			foreach (var l in launchers)
			{
				if (l.IsGlobal == false) continue;

				var env = l.RunTime;
				if (env.appSetting.useVersionNum)
				{
					this.GetGitCommit();

					var vname = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + fileName;
					var path = System.IO.Path.Combine(Application.streamingAssetsPath, vname);
					var data = new VersionInfo() { buildTime = DateTime.Now.ToString(), buildDeviceName = SystemInfo.deviceName };
					if (File.Exists(path))
					{
						var comment = env.appSetting.versionInfo.comment;
						data = FileTool.Read<VersionInfo>(path, FileTool.SerializerType.XML);
						if (data == null) data = new VersionInfo();
						var versionUpdated = data.major != env.appSetting.versionInfo.major || data.minor != env.appSetting.versionInfo.minor;

						data.major = env.appSetting.versionInfo.major;
						data.minor = env.appSetting.versionInfo.minor;
						data.build = Mathf.Max(data.build, env.appSetting.versionInfo.build);
						data.comment = comment;
						data.buildTime = DateTime.Now.ToString();
						data.buildDeviceName = SystemInfo.deviceName;
						data.lastCommit = this.gitCommit;

						if (versionUpdated) data.build = 0;
						data.build++;
					}
					FileTool.Write(path, data, FileTool.SerializerType.XML);
					env.appSetting.versionInfo = data;
				}
			}
		}
		protected void GetGitCommit()
		{
			this.gitCommitCount = 2;
			this.gitCommit = "Last " + this.gitCommitCount + " commits" + "\n";
			var command = new RunCommand.CommandInfo();
			command.exe = "git.exe";
			command.args = "log";
			command.workingDirectory = Application.streamingAssetsPath;
			command.async = false;
			command.exit = this.Exit;
			command.output = this.Output;
			command.enableLogging = false;
            RunCommand.RunCommandInProcess(command);
		}
		protected void Output(string message)
		{
			if (message.Contains("commit")) this.gitCommitCount--;
			if (this.gitCommitCount < 0) return;

			this.gitCommit += message + "\n";
		}
		protected void Exit(string cmd, string args, int exitCode)
		{
		}
	}
    #endif
}