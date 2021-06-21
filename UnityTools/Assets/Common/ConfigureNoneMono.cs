using System;
using UnityEngine;
using UnityTools.GUITool;

namespace UnityTools.Common
{
    public abstract class ConfigureNoneMono<T> : IConfigure<T>, IConfigureSerialize where T : new()
    {
        public T D => this.data ??= new T();
        public bool IsOpen => this.open;
        public bool IsSyncing=>this.isSyncing;

        public virtual string FilePath => ConfigureTool.GetFilePath(this.ToString(), this.SaveType, this.Preset);

        public virtual ConfigurePreset Preset => this.preset;

        public virtual FileTool.SerializerType SaveType => this.saveType;


        public virtual KeyCode OpenKey => this.openKey;

        public virtual KeyCode SaveKey => this.saveKey;

        public virtual KeyCode LoadKey => this.loadKey;

        [SerializeField] protected FileTool.SerializerType saveType = FileTool.SerializerType.XML;
        [SerializeField] protected KeyCode saveKey = KeyCode.None;
        [SerializeField] protected KeyCode openKey = KeyCode.None;
        [SerializeField] protected KeyCode loadKey = KeyCode.None;
        [SerializeField] protected ConfigurePreset preset = ConfigurePreset.Default;
		[SerializeField] protected bool isSyncing = false;

        [SerializeField] private T data;
        protected bool open = false;
        private GUIContainer guiContainer = null;

        public ConfigureNoneMono()
        {
            this.Initialize();
        }

        public virtual void OnConfigureChange(object sender, EventArgs args) { }

        public void Initialize()
        {
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

        public virtual void Start()
        {
            if (this.data == null) this.Initialize();
        }

        public virtual void Update()
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

        public virtual void OnGUIDraw()
        {
            if(!this.IsOpen) return;
            if(this.D is GUIContainer gui)
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

		public byte[] OnSerialize()
		{
            return Serialization.ObjectToByteArray(JsonUtility.ToJson(this.D));
		}

		public void OnDeserialize(byte[] data)
		{
            JsonUtility.FromJsonOverwrite(Serialization.ByteArrayToObject<string>(data), this.data);
		}
	}
}