using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.GUITool;

namespace UnityTools.Common
{
    public abstract class ConfigureSingleton<T> : SingletonMonoBehaviour<ConfigureSingleton<T>>, IConfigure<T>, IConfigureSerialize where T : new()
    {
        public T D => this.data ??= new T();
        public bool IsOpen => this.open;

        public virtual string FilePath => ConfigureTool.GetFilePath(this.ToString(), this.SaveType, this.Preset);

        public virtual ConfigurePreset Preset => this.preset;

        public virtual FileTool.SerializerType SaveType => this.saveType;


        public virtual KeyCode OpenKey => this.openKey;

        public virtual KeyCode SaveKey => this.saveKey;

        public virtual KeyCode LoadKey => this.loadKey;
        [SerializeField] protected ConfigureSaveMode saveMode = ConfigureSaveMode.UseEditorValue;
        [SerializeField] protected FileTool.SerializerType saveType = FileTool.SerializerType.XML;
        [SerializeField] protected KeyCode saveKey = KeyCode.None;
        [SerializeField] protected KeyCode openKey = KeyCode.None;
        [SerializeField] protected KeyCode loadKey = KeyCode.None;
        [SerializeField] protected ConfigurePreset preset = ConfigurePreset.Default;

        [SerializeField] private T data;
        protected bool open = false;
        private GUIContainer guiContainer = null;

        public virtual void OnConfigureChange(object sender, EventArgs args) { }

        public void Initialize()
        {
            //do not load for prefab object
            if (this.gameObject.scene.rootCount == 0) return;

            this.Load();
            this.NotifyChange();
        }

        public void Save()
        {
            FileTool.Write(this.FilePath, this.D, this.SaveType);
        }

        public void Load()
        {
            var data = FileTool.Read<T>(this.FilePath, this.SaveType);
            if (data != null) this.data = data;
            this.guiContainer = null;
        }

        public void NotifyChange()
        {
            this.OnConfigureChange(this, null);
        }

        protected virtual void Start()
        {
            if (this.data == null) this.Initialize();
        }
        protected virtual void OnDisable()
        {
            if (Application.isEditor && this.saveMode == ConfigureSaveMode.UseEditorValue)
            {
                this.Save();
                //LogTool.Log("Configure " + this.name + " Saved", LogLevel.Verbose, LogChannel.IO);
            }
        }
        protected virtual void Update()
        {
            if (Input.GetKeyDown(this.OpenKey))
            {
                this.open = !this.IsOpen;
            }
            if (Input.GetKeyDown(this.LoadKey))
            {
                this.Load();
                this.NotifyChange();
            }
            if (Input.GetKeyDown(this.SaveKey))
            {
                this.Save();
            }
        }
        private Rect windowRect;
        protected virtual void OnGUIDrawWindow()
        {
            if(!this.IsOpen) return;
            this.windowRect =
                GUILayout.Window(
                    //GUIUtil.ResizableWindow(
                    GetHashCode(), this.windowRect, (id) =>
                    {
                        this.OnGUIDraw();
                        GUI.DragWindow();
                    },
            "",
            GUILayout.MinWidth(500));
        }
        public virtual void OnGUIDraw()
        {
            if(!this.IsOpen) return;
            if (this.D is GUIContainer gui)
            {
                gui.OnGUI();
            }
            else
            {
                this.guiContainer ??= new GUIContainer(typeof(T), this.D);
                this.guiContainer.OnGUI();
            }
            this.OnGUISaveLoadButton();
        }
        protected virtual void OnGUISaveLoadButton()
        {
            ConfigureGUI.OnGUIEnum(ref this.preset, "Preset");
            if (GUILayout.Button("Save")) this.Save();
            if (GUILayout.Button("Load")) { this.Load(); this.NotifyChange(); }
        }
    }
}