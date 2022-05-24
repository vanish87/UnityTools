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
		public bool IsOpen { get => this.open; set => this.open = value; }
        public bool IsSyncing=>this.isSyncing;

        public virtual string FilePath => ConfigureTool.GetFilePath(this.ToString(), this.SaveType, this.PresetToString(this.Preset));

        public virtual ConfigurePreset Preset => this.preset;

        public virtual FileTool.SerializerType SaveType => this.saveType;


        public virtual KeyCode OpenKey => this.openKey;

        public virtual KeyCode SaveKey => this.saveKey;

        public virtual KeyCode LoadKey => this.loadKey;

		public bool Inited => this.inited;

		[SerializeField] protected ConfigureSaveMode saveMode = ConfigureSaveMode.SaveEditorValueWhenExitingPlayMode;
        [SerializeField] protected FileTool.SerializerType saveType = FileTool.SerializerType.XML;
        [SerializeField] protected KeyCode saveKey = KeyCode.None;
        [SerializeField] protected KeyCode openKey = KeyCode.None;
        [SerializeField] protected KeyCode loadKey = KeyCode.None;
        [SerializeField] protected ConfigurePreset preset = ConfigurePreset.Default;
		[SerializeField] protected bool isSyncing = false;

        [SerializeField] private T data;
        protected bool open = false;
        private GUIContainer guiContainer = null;
        private bool inited = false;
		protected virtual string PresetToString(ConfigurePreset preset) => preset.ToString();

        public virtual void OnConfigureChange(object sender, EventArgs args) { }

		public void Init(params object[] parameters)
		{
            if(this.Inited) return;
            //do not load for prefab object
            if (this.gameObject.scene.rootCount == 0) return;

            this.Load();
            this.NotifyChange();

            this.inited = true;
		}

		public void Deinit(params object[] parameters)
		{
            this.inited = false;
		}
        public virtual void Save()
        {
            FileTool.Write(this.FilePath, this.D, this.SaveType);
        }

        public virtual void Load()
        {
            var data = FileTool.Read<T>(this.FilePath, this.SaveType);
            if (data != null) this.data = data;
            this.guiContainer = null;
        }

        public virtual void OpenFile()
        {
            Application.OpenURL(this.FilePath);
        }
        public void NotifyChange()
        {
            this.OnConfigureChange(this, null);
        }

		public virtual byte[] OnSerialize()
		{
            return Serialization.ObjectToByteArray(JsonUtility.ToJson(this.D));
		}

		public virtual void OnDeserialize(byte[] data)
		{
            JsonUtility.FromJsonOverwrite(Serialization.ByteArrayToObject<string>(data), this.data);
		}
        protected virtual void OnEnable()
        {
            this.Init();
        }
        protected virtual void OnDisable()
        {
            if (Application.isEditor && this.saveMode == ConfigureSaveMode.SaveEditorValueWhenExitingPlayMode)
            {
                this.Save();
                //LogTool.Log("Configure " + this.name + " Saved", LogLevel.Verbose, LogChannel.IO);
            }
			this.Deinit();
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
        protected virtual void OnGUI()
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