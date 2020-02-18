using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UnityTools.Common
{
    public class Launcher<T> : MonoBehaviour where T : class, new()
    {
        [Serializable]
        public class PCInfo
        {
            public string name = "OutputPC";
            public string ipAddress = "127.0.0.1";
            public bool isServer = false;
        }
        public interface ILauncherUser
        {
            void OnInit(T data);
            void OnDeinit(T data);
            void OnReload(T data);
        }

        [SerializeField] protected bool global = false;
        [SerializeField] protected T data = new T();
        protected List<ILauncherUser> userList = new List<ILauncherUser>();

        protected virtual void OnEnable()
        {
            this.CleanUp();
            if (this.global)
            {
                foreach (var g in ObjectTool.FindRootObject())
                {
                    this.userList.AddRange(g.GetComponents<ILauncherUser>());
                    this.userList.AddRange(g.GetComponentsInChildren<ILauncherUser>());
                }
            }
            else
            {
                this.userList.AddRange(this.GetComponents<ILauncherUser>());
                this.userList.AddRange(this.GetComponentsInChildren<ILauncherUser>());
            }

            foreach(var u in this.userList)
            {
                u.OnInit(this.data);
            }
        }
        protected virtual void OnDisable()
        {
            foreach(var u in this.userList)
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
