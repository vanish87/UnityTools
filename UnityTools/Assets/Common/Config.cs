using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityTools.Common;

namespace UnityTools
{
    public interface IConfigure<T> where T : class, new()
    {
        //not used
        void OnConfigureChange(Config<T> sender, EventArgs args);
    }

    public interface IConfigureSerialize
    {
        FileTool.SerializerType SaveType { get; } 
    }

    public enum ConfigureSaveMode
    {
        None = 0,
        UseEditorValue,
        UseFileValue,
    }

    public enum ConfigurePreset
    {
        Default,
        Preset_1,
        Preset_2,
        Preset_3,
        Preset_4,
        Preset_5,
    }
    //same as XmlConfig, but is not subclass of mono
    //Note it could be multiple instance but they will save/load to same file
    public abstract class ConfigNoneMono<T> : IConfigureSerialize where T : class, new()
    {
        private readonly Dictionary<FileTool.SerializerType, string> typeToExtension = new Dictionary<FileTool.SerializerType, string>()
        {
            {FileTool.SerializerType.XML, ".xml"},
            {FileTool.SerializerType.Json, ".json"},
            {FileTool.SerializerType.Binary, ".bin"},

        };
        public abstract T D { get; set; }
        public bool Open { get { return this.open; } set { this.open = value; } }
        protected virtual string filePath { get { return System.IO.Path.Combine(Application.persistentDataPath, "config_" + SceneManager.GetActiveScene().name + ".xml"); } }

        protected bool inited = false;

        protected Rect windowRect = new Rect();
        protected bool open = false;
        protected virtual float MinWidth { get { return 500f; } }

        public FileTool.SerializerType SaveType =>this.saveType;

        [SerializeField] protected FileTool.SerializerType saveType = FileTool.SerializerType.XML;
        [SerializeField] protected KeyCode saveKey = KeyCode.None;
        [SerializeField] protected KeyCode openKey = KeyCode.None;
        [SerializeField] protected KeyCode loadKey = KeyCode.None;
        [SerializeField] protected ConfigurePreset preset = ConfigurePreset.Default;

        #region IConfigure
        protected event EventHandler OnConfigureChanged;
        public void RegisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; this.OnConfigureChanged += handler; }
        public void UnregisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; }
        protected void OnConfigureChange() { this.OnConfigureChanged?.Invoke(this, new EventArgs()); }

        /*protected event EventHandler OnConfigureGUI;
        public void RegisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; this.OnConfigureGUI += handler; }
        public void UnregisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; }
        protected void OnDrawConfigureGUI() { this.OnConfigureGUI?.Invoke(this, new EventArgs()); }*/
        #endregion

        protected virtual void OnDrawGUI()
        {

        }

        protected ConfigNoneMono()
        {
            this.Initialize();
        }

        public virtual void Update()
        {
            if (Input.GetKeyDown(this.openKey))
            {
                this.Open = !this.Open;
            }
            if (Input.GetKeyDown(this.loadKey))
            {
                this.LoadAndNotify();
            }
            if (Input.GetKeyDown(this.saveKey))
            {
                this.Save();
            }
        }

        public virtual void Initialize()
        {
            if (this.inited == false)
            {
                this.inited = true;
                this.LoadAndNotify();
            }
        }
        protected virtual void OnGUI()
        {
            if (this.open)
            {
                this.windowRect =
                    GUILayout.Window(
                        //GUIUtil.ResizableWindow(
                        GetHashCode(), this.windowRect, (id) =>
                        {
                            this.OnDrawGUI();
                            GUI.DragWindow();
                        },
                "",
                GUILayout.MinWidth(MinWidth));
            }
        }
        public virtual void Save()
        {
            var path = this.GetFilePath();
            FileTool.Write(path, this.D, this.saveType);
        }
        protected virtual void Load()
        {
            var path = this.GetFilePath();
            var data = FileTool.Read<T>(path, this.saveType);
            if (data != null) this.D = data;
        }
        protected string GetFilePath()
        {
            var path = this.filePath;
            var ext = typeToExtension[this.saveType];
            if (path.Contains(ext) == false) path += ext;

            if(this.preset != ConfigurePreset.Default) path.Insert(path.LastIndexOf('.')-1, this.preset.ToString());
            return path;
        }
        public virtual void LoadAndNotify()
        {
            this.Load();
            this.NotifyChange();
        }

        public virtual void NotifyChange()
        {
            this.OnConfigureChange();
        }
    }

    public abstract class ConfigMonoSingleton<T> : SingletonMonoBehaviour<ConfigMonoSingleton<T>>, IConfigureSerialize where T : class, new()
    {
        private readonly Dictionary<FileTool.SerializerType, string> typeToExtension = new Dictionary<FileTool.SerializerType, string>()
        {
            {FileTool.SerializerType.XML, ".xml"},
            {FileTool.SerializerType.Json, ".json"},
            {FileTool.SerializerType.Binary, ".bin"},

        };
        [SerializeField] protected ConfigureSaveMode saveMode = ConfigureSaveMode.UseEditorValue;
        public abstract T D { get; set; }
        public bool Open { get { return this.open; } set { this.open = value; } }
        protected virtual string filePath { get { return System.IO.Path.Combine(Application.persistentDataPath, "config_" + SceneManager.GetActiveScene().name + ".xml"); } }

        protected bool inited = false;

        protected Rect windowRect = new Rect();
        protected bool open = false;
        protected virtual float MinWidth { get { return 500f; } }
        public FileTool.SerializerType SaveType => this.saveType;

        [SerializeField] protected FileTool.SerializerType saveType = FileTool.SerializerType.XML;
        [SerializeField] protected KeyCode saveKey = KeyCode.None;
        [SerializeField] protected KeyCode openKey = KeyCode.None;
        [SerializeField] protected KeyCode loadKey = KeyCode.None;

        [SerializeField] protected ConfigurePreset preset = ConfigurePreset.Default;

        #region IConfigure
        protected event EventHandler OnConfigureChanged;
        public void RegisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; this.OnConfigureChanged += handler; }
        public void UnregisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; }
        protected void OnConfigureChange() { this.OnConfigureChanged?.Invoke(this, new EventArgs()); }

        //this is not necessary, using OnDrawGUI should be enough
        /*protected event EventHandler OnConfigureGUI;
        public void RegisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; this.OnConfigureGUI += handler; }
        public void UnregisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; }
        protected void OnDrawConfigureGUI() { this.OnConfigureGUI?.Invoke(this, new EventArgs()); }*/
        #endregion

        protected virtual void OnDrawGUI()
        {

        }

        //handler will be registered on enabled
        //so at here call notify to call all handlers
        protected virtual void Start()
        {
            this.Initialize();
            this.NotifyChange();
        }
        protected virtual void OnDisable()
        {
            if (Application.isEditor && this.saveMode == ConfigureSaveMode.UseEditorValue)
            {
                this.Save();
                this.LoadAndNotify(); 
                //LogTool.Log("Configure " + this.name + " Saved", LogLevel.Verbose, LogChannel.IO);
            }
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(this.openKey))
            {
                this.Open = !this.Open;
            }
            if (Input.GetKeyDown(this.loadKey))
            {
                this.LoadAndNotify();
            }
            if (Input.GetKeyDown(this.saveKey))
            {
                this.Save();
            }
        }

        public virtual void Initialize()
        {
            //do not load for prefab object
            if (this.gameObject.scene.rootCount == 0) return;
            
            if (this.inited == false)
            {
                this.inited = true;
                this.Load();
            }
        }
        protected virtual void OnGUI()
        {
            if (this.open)
            {
                this.windowRect =
                    GUILayout.Window(
                        //GUIUtil.ResizableWindow(
                        GetHashCode(), this.windowRect, (id) =>
                        {
                            this.OnDrawGUI();
                            GUI.DragWindow();
                        },
                "",
                GUILayout.MinWidth(MinWidth));
            }
        }

        public virtual void Save()
        {
            var path = this.GetFilePath();
            FileTool.Write(path, this.D, this.saveType);
        }
        protected virtual void Load()
        {
            var path = this.GetFilePath();
            var data = FileTool.Read<T>(path, this.saveType);
            if (data != null) this.D = data;
        }
        protected string GetFilePath()
        {
            var path = this.filePath;
            var ext = typeToExtension[this.saveType];
            if (path.Contains(ext) == false) path += ext;

            if(this.preset != ConfigurePreset.Default) path.Insert(path.LastIndexOf('.')-1, this.preset.ToString());
            return path;
        }

        public virtual void LoadAndNotify()
        {
            this.Load();
            this.NotifyChange();
        }

        public virtual void NotifyChange()
        {
            this.OnConfigureChange();
        }
    }

    public abstract class Config<T> : MonoBehaviour, IConfigureSerialize where T : class, new()
    {
        private readonly Dictionary<FileTool.SerializerType, string> typeToExtension = new Dictionary<FileTool.SerializerType, string>()
        {
            {FileTool.SerializerType.XML, ".xml"},
            {FileTool.SerializerType.Json, ".json"},
            {FileTool.SerializerType.Binary, ".bin"},

        };
        public abstract T D { get; internal set; }
        public bool Open { get { return this.open; } set { this.open = value; } }
        protected virtual string filePath { get { return System.IO.Path.Combine(Application.persistentDataPath, "config_" + SceneManager.GetActiveScene().name + typeToExtension[this.saveType]); } }
        [SerializeField] protected ConfigureSaveMode saveMode = ConfigureSaveMode.UseEditorValue;

        protected bool inited = false;

        protected Rect windowRect = new Rect();
        protected bool open = false;
        protected virtual float MinWidth { get { return 500f; } }

        public FileTool.SerializerType SaveType => this.saveType;

        [SerializeField] protected FileTool.SerializerType saveType = FileTool.SerializerType.XML;
        [SerializeField] protected KeyCode saveKey = KeyCode.None;
        [SerializeField] protected KeyCode openKey = KeyCode.None;
        [SerializeField] protected KeyCode loadKey = KeyCode.None;
        [SerializeField] protected ConfigurePreset preset = ConfigurePreset.Default;

        #region IConfigure
        protected event EventHandler OnConfigureChanged;
        public void RegisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; this.OnConfigureChanged += handler; }
        public void UnregisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; }
        protected void OnConfigureChange() { this.OnConfigureChanged?.Invoke(this, new EventArgs()); }

        /*
        protected event EventHandler OnConfigureGUI;
        public void RegisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; this.OnConfigureGUI += handler; }
        public void UnregisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; }
        protected void OnDrawConfigureGUI() { this.OnConfigureGUI?.Invoke(this, new EventArgs()); }*/
        #endregion

        protected virtual void OnDrawGUI()
        {

        }

        protected virtual void OnSaveLoadGUI()
        {
            ConfigureGUI.OnGUIEnum(ref this.preset, "Preset");
            if(GUILayout.Button("Save"))
            {
                this.Save();
            }
            if(GUILayout.Button("Load"))
            {
                this.LoadAndNotify();
            }
        }

        //handler will be registered on enabled
        //so at here call notify to call all handlers
        protected virtual void Start()
        {
            this.Initialize();
            this.NotifyChange();
        }
        protected virtual void OnDisable()
        {
            if (Application.isEditor && this.saveMode == ConfigureSaveMode.UseEditorValue)
            {
                this.Save();
                this.LoadAndNotify();
                //LogTool.Log("Configure " + this.name + "Saved ", LogLevel.Verbose, LogChannel.IO);
            }

        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(this.openKey))
            {
                this.Open = !this.Open;
            }
            if (Input.GetKeyDown(this.loadKey))
            {
                this.LoadAndNotify();
            }
            if (Input.GetKeyDown(this.saveKey))
            {
                this.Save();
            }
        }

        public virtual void Initialize()
        {
            //do not load for prefab object
            if (this.gameObject.scene.rootCount == 0) return;

            if (this.inited == false)
            {
                this.inited = true;
                this.Load();
            }
        }
        protected virtual void OnGUI()
        {
            if (this.open)
            {
                this.windowRect =
                    GUILayout.Window(
                        //GUIUtil.ResizableWindow(
                        GetHashCode(), this.windowRect, (id) =>
                        {
                            this.OnDrawGUI();
                            GUI.DragWindow();
                        },
                "",
                GUILayout.MinWidth(MinWidth));
            }
        }


        public virtual void Save()
        {
            var path = this.GetFilePath();
            FileTool.Write(path, this.D, this.saveType);
        }
        protected virtual void Load()
        {
            var path = this.GetFilePath();
            var data = FileTool.Read<T>(path, this.saveType);
            if (data != null) this.D = data;
        }
        protected string GetFilePath()
        {
            var path = this.filePath;
            var ext = typeToExtension[this.saveType];
            if (path.Contains(ext) == false) path += ext;

            if(this.preset != ConfigurePreset.Default) path = path.Insert(path.LastIndexOf('.'), this.preset.ToString());
            return path;
        }
        public virtual void LoadAndNotify()
        {
            this.Load();
            this.NotifyChange();
        }

        public virtual void NotifyChange()
        {
            this.OnConfigureChange();
        }
    }

    public class ConfigureGUI
    {

        public class LastParseInfo
        {
            public object lastAvailableValue;
            public string lastString;
        }
        static Dictionary<object, LastParseInfo> lastAvailableValue = new Dictionary<object, LastParseInfo>();
        static Dictionary<int, string> stringTable = new Dictionary<int, string>();
        public static void OnGUISlider<T>(ref T value, float min = 0, float max = 1, string displayName = null)
        {
            OnGUI(ref value, displayName);

            var isSliderable = typeof(T) == typeof(float)
                            || typeof(T) == typeof(int)
                            || typeof(T) == typeof(uint);

            if (isSliderable == false) return;

            using (var h = new GUILayout.HorizontalScope())
            {
                var floatValue = (float)Convert.ChangeType(value, typeof(float));
                OnGUISliderFloat(ref floatValue, min, max);
                value = (T)Convert.ChangeType(floatValue, typeof(T));
            }
        }

        public static void OnGUISliderFloat(ref float value, float min = 0, float max = 1)
        {
            value = GUILayout.HorizontalSlider(value, min, max, GUILayout.MinWidth(70));
        }
        static public void OnGUIInternal<T>(ref T value, string displayName, string variableHash)
        {
            OnGUI(ref value, variableHash + displayName);
        }
        static public void OnGUI<T>(ref T value, [System.Runtime.CompilerServices.CallerMemberName] string displayName = null)
        {
            var dhash = displayName.GetHashCode();

            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName);
                var hash = (displayName + value.ToString()).GetHashCode();
                var target = value.ToString();
                var hasUnparsedStr = stringTable.ContainsKey(hash);
                if (hasUnparsedStr)
                {
                    target = stringTable[hash];
                }
                var color = hasUnparsedStr ? Color.red : GUI.color;

                using (var cs = new GUIUtil.ColorScope(color))
                {
                    var ret = GUILayout.TextField(target, op);
                    var newValue = default(T);
                    var canParse = false;
                    try
                    {
                        newValue = (T)Convert.ChangeType(ret, typeof(T));
                        canParse = newValue.ToString() == ret;
                    }
                    catch (Exception) { }

                    if (canParse)
                    {
                        value = newValue;
                        if (hasUnparsedStr) stringTable.Remove(hash);
                    }
                    else
                    {
                        stringTable[hash] = ret;
                    }
                }
            }
        }
        static public void OnGUI(ref bool toggle, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                toggle = GUILayout.Toggle(toggle, displayName == null ? "bool" : displayName);
            }
        }
        static public void OnGUI(ref Vector2 vector, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName == null ? nameof(Vector2) : displayName);
                OnGUI(ref vector.x, "x");
                OnGUI(ref vector.y, "y");
            }
        }
        static public void OnGUISlider(ref Vector2 vector, Vector2 min, Vector2 max, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName == null ? nameof(Vector2) : displayName);
                OnGUI(ref vector.x, "x"); OnGUISliderFloat(ref vector.x, min.x, max.x);
                OnGUI(ref vector.y, "y"); OnGUISliderFloat(ref vector.y, min.y, max.y);
            }
        }
        static public void OnGUI(ref Vector2Int vector, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName == null ? nameof(Vector2) : displayName);
                var x = vector.x;
                var y = vector.y;
                OnGUI(ref x, "x");
                OnGUI(ref y, "y");
                vector.x = x;
                vector.y = y;
            }
        }
        static public void OnGUI(ref Vector3 vector, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName == null ? typeof(Vector3).ToString() : displayName);
                OnGUI(ref vector.x, "x");
                OnGUI(ref vector.y, "y");
                OnGUI(ref vector.z, "z");
            }
        }

        static public void OnGUISlider(ref Vector3 vector, Vector3 min, Vector3 max, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName == null ? typeof(Vector3).ToString() : displayName);
                OnGUI(ref vector.x, "x"); OnGUISliderFloat(ref vector.x, min.x, max.x);
                OnGUI(ref vector.y, "y"); OnGUISliderFloat(ref vector.y, min.y, max.y);
                OnGUI(ref vector.z, "z"); OnGUISliderFloat(ref vector.z, min.z, max.z);
            }
        }
        static public void OnGUI(ref Vector4 vector, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName == null ? typeof(Vector4).ToString() : displayName);
                OnGUI(ref vector.x, "x");
                OnGUI(ref vector.y, "y");
                OnGUI(ref vector.z, "z");
                OnGUI(ref vector.w, "w");
            }
        }
        static public void OnGUI(ref Color color, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName == null ? typeof(Color).ToString() : displayName);
                OnGUIInternal(ref color.r, displayName, "r");
                OnGUIInternal(ref color.g, displayName, "g");
                OnGUIInternal(ref color.b, displayName, "b");
                OnGUIInternal(ref color.a, displayName, "a");
            }
        }
        static public void OnGUISlider(ref Vector4 vector, Vector4 min, Vector4 max, string displayName = null)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName == null ? typeof(Vector4).ToString() : displayName);
                OnGUI(ref vector.x, "x"); OnGUISliderFloat(ref vector.x, min.x, max.x);
                OnGUI(ref vector.y, "y"); OnGUISliderFloat(ref vector.y, min.y, max.y);
                OnGUI(ref vector.z, "z"); OnGUISliderFloat(ref vector.z, min.z, max.z);
                OnGUI(ref vector.w, "w"); OnGUISliderFloat(ref vector.w, min.w, max.w);
            }
        }

        static public void OnGUI(ref int value, string[] labels, string displayName = null, GUILayoutOption[] op = null)
        {
            using (var h = new GUILayout.VerticalScope())
            {
                if (labels.Length > 1)
                {
                    GUILayout.Label(displayName);
                    if (value >= 0 && value < labels.Length)
                    {
                        value = GUILayout.SelectionGrid(value, labels, 4, op);
                    }
                    else
                    {
                        GUILayout.Label(string.Format("value:{0} is not valid", value));
                    }
                }
                else
                {
                    GUILayout.Label(displayName + labels.FirstOrDefault());
                }
            }
        }
        static public void OnGUIEnum<T>(ref T value, string displayName = null, GUILayoutOption[] op = null) where T : struct
        {
            using (var h = new GUILayout.VerticalScope())
            {
                var labels = Enum.GetNames(typeof(T));
                var index = Array.IndexOf(labels, Enum.GetName(typeof(T), value));
                if (labels.Length > 1)
                {
                    GUILayout.Label(displayName);
                    if (index >= 0)
                    {
                        index = GUILayout.SelectionGrid(index, labels, 4, op);
                        Enum.TryParse(labels[index], out value);
                    }
                    else
                    {
                        GUILayout.Label(string.Format("value:{0} index {1} is not valid", value, index));
                    }
                }
                else
                {
                    GUILayout.Label(displayName + labels.FirstOrDefault());
                }
            }
        }
        public static class Style
        {
            // 参考 https://github.com/XJINE/XJUnity3D.GUI
            public static readonly GUIStyle FoldoutPanelStyle;

            static Style()
            {
                FoldoutPanelStyle = new GUIStyle(GUI.skin.label);
                FoldoutPanelStyle.normal.textColor = GUI.skin.toggle.normal.textColor;
                FoldoutPanelStyle.hover.textColor = GUI.skin.toggle.hover.textColor;

                var tex = new Texture2D(1, 1);
                tex.SetPixels(new[] { new Color(0.5f, 0.5f, 0.5f, 0.5f) });
                tex.Apply();
                FoldoutPanelStyle.hover.background = tex;
            }
        }
        static public void OnFolder(ref bool foldOpen, string name = null)
        {
            var foldStr = foldOpen ? "▼" : "▶";

            using (var h = new GUILayout.HorizontalScope())
            {
                foldOpen ^= GUILayout.Button(foldStr + name, Style.FoldoutPanelStyle);
            }
        }
    }
}