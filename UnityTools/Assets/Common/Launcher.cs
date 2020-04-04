using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    [Serializable]
    public class PCInfo
    {
        public enum Role
        {
            Server,
            Client,
            Devlopment,
        }
        public string name = "OutputPC";
        public string ipAddress = "127.0.0.1";
        public Role role = Role.Client;

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
        }

        public Runtime runtime = Runtime.Debug;
        public ApplicationSetting appSetting = new ApplicationSetting();
    }

    
    public class Launcher<T> : MonoBehaviour where T : class, new()
    {
        public interface ILauncherUser
        {
            void OnInit(T data);
            void OnDeinit(T data);
            void OnReload(T data);

            Environment RuntimEnvironment { get; set; }

            //higher order of user executes after than lower order user
            //see LauncherOrder for default setting
            int Order { get; }
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

            foreach (var u in this.userList) u.RuntimEnvironment = this.environment;

            this.userList = this.userList.OrderBy(ul => ul.Order).ToList();
            foreach (var u in this.userList)
            {
                LogTool.Log("Init order " + u.Order + " " + u.ToString());
                u.OnInit(this.data);
            }
        }
        protected virtual void OnDisable()
        {
            foreach (var u in this.userList)
            {
                u.OnDeinit(this.data);
            }
            this.CleanUp();
        }

        protected virtual void OnReload()
        {
            foreach (var u in this.userList)
            {
                u.OnReload(this.data);
            }
        }

        protected void CleanUp()
        {
            this.userList.Clear();
        }
    }
}
