using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    public interface ILauncher
    {
        Environment RunTime { get; }
        bool IsGlobal { get; }

    }
    [Serializable]
    public class PCInfo
    {
        public enum Role
        {
            None,
            Server,
            Client,
            Development,
        }
        [Serializable]
        public class Port
        {
            public string name;
            public short port;

            public void OnGUIDraw()
            {
                GUILayout.Label("port " + name + " " + port);
            }
        }
        public string name = "OutputPC";
        public string ipAddress = "127.0.0.1";
        public Role role = Role.None;
        public List<Port> ports = new List<Port>();

        public void OnGUIDraw()
        {
            GUILayout.Label(this.name + " " + this.role + " " + this.ipAddress);
            foreach(var p in this.ports) p.OnGUIDraw();
        }
    }
    [Serializable]
    public class Environment
    {
        public enum Runtime
        {
            Debug,//local editor debug
            DebugBuild,//production pc debug, disable data sim functions, but allow configuring locally
            GUIConfigure,//GUI Configure mode that only display gui
            Production,// production
        }

        [Serializable]
        public class ApplicationSetting
        {
            public int vsync = 0;
            public int targetFPS = 30;
            public bool zipAfterBuild = false;
            public bool useVersionNum = true;
            public VersionInfo versionInfo = new VersionInfo();
        }

        public Runtime runtime = Runtime.Debug;
        public ApplicationSetting appSetting = new ApplicationSetting();
    }

    
    public class Launcher<T> : MonoBehaviour, ILauncher, GUITool.GUIMenuGroup.IGUIHandler where T : class, new()
    {
        public enum LaunchEvent
        {
            Init = 0,
            DeInit,
            Reload,
        }
        public interface ILauncherUser
        {
            void OnLaunchEvent(T data, LaunchEvent levent);

            Environment Runtime { get; set; }

            //higher order of user executes after than lower order user
            //see LauncherOrder for default setting
            int Order { get; }
            LauncherState CurrentState { get; set; }
        }

        public enum LauncherState
        {
            Undefined,
            Inited,
            DeInited,

        }

        public enum LauncherOrder
        {
            LowLevel    = 0,
            Network     = 100,
            Default     = 1000,
            Application = 2000,
        }

        [SerializeField] protected bool global = false;
        [SerializeField] protected T data = new T();
        [SerializeField] protected Environment environment = new Environment();
        protected List<ILauncherUser> userList = new List<ILauncherUser>();

        public Environment RunTime => this.environment;
        public bool IsGlobal => this.global;

        public string Title => "Application Information";

        public KeyCode OpenKey => KeyCode.F12;

        protected virtual void ConfigureEnvironment()
        {
            var logConfigure = FindObjectOfType<LogConfigure>();
            if(logConfigure != null)
            {
                logConfigure.SetupChannel();
                logConfigure.SetupLog();
            }

            Application.targetFrameRate = this.environment.appSetting.targetFPS;
            QualitySettings.vSyncCount = this.environment.appSetting.vsync;
        }

        protected virtual Environment OnCreateEnv()
        {
            return this.environment;
        }

        protected virtual void OnEnable()
        {
            this.CleanUp();
            if (this.global)
            {
                this.userList.AddRange(ObjectTool.FindAllObject<ILauncherUser>());
            }
            else
            {
                this.userList.AddRange(this.GetComponentsInChildren<ILauncherUser>());
            }

            this.environment = this.OnCreateEnv();
            this.ConfigureEnvironment();

            foreach (var u in this.userList) u.Runtime = this.environment;

            this.userList = this.userList.OrderBy(ul => ul.Order).ToList();
            foreach (var u in this.userList)
            {
                LogTool.Log("Init order " + u.Order + " " + u.ToString(), LogLevel.Dev);
                u.OnLaunchEvent(this.data, LaunchEvent.Init);
                u.CurrentState = LauncherState.Inited;
            }
        }
        protected virtual void OnDisable()
        {
            foreach (var u in this.userList)
            {
                u.OnLaunchEvent(this.data, LaunchEvent.DeInit);
                u.CurrentState = LauncherState.DeInited;
            }
            this.CleanUp();
        }

        protected virtual void OnReload()
        {
            foreach (var u in this.userList)
            {
                u.OnLaunchEvent(this.data, LaunchEvent.Reload);
            }
        }

        protected void CleanUp()
        {
            this.userList.Clear();
        }

        public virtual void OnDrawGUI()
        {
            var env = this.RunTime;
            var ver = env.appSetting.versionInfo;
            GUILayout.Label(string.Format("version: {0}.{1}.{2} build on {3}", ver.major, ver.minor, ver.build, ver.buildTime));
            GUILayout.Label(ver.buildDeviceName);
            GUILayout.Label(ver.comment);
        }
    }
}
