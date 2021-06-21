using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityTools.Common;
using UnityTools.GUITool;

namespace UnityTools
{
    public interface IConfigure
    {
        void OnConfigureChange(object sender, EventArgs args);
        bool IsOpen { get; }
        string FilePath { get; }
        ConfigurePreset Preset { get; }

        KeyCode OpenKey { get; }
        KeyCode SaveKey { get; }
        KeyCode LoadKey { get; }

        void Initialize();
        void Save();
        void Load();
        void NotifyChange();
        void OnGUIDraw();

    }
    public interface IConfigure<T> : IConfigure
    {
        T D { get; }
    }

    public interface IConfigureSerialize
    {
        FileTool.SerializerType SaveType { get; }
		bool IsSyncing { get; }

        byte[] OnSerialize();
        void OnDeserialize(byte[] data);
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
    public static class ConfigureTool
    {
        public static Dictionary<FileTool.SerializerType, string> TypeToExtension = new Dictionary<FileTool.SerializerType, string>()
        {
            {FileTool.SerializerType.XML, ".xml"},
            {FileTool.SerializerType.Json, ".json"},
            {FileTool.SerializerType.Binary, ".bin"},
        };

        public static string GetFilePath(string fileName, FileTool.SerializerType saveType, ConfigurePreset preset)
        {
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
            var ext = TypeToExtension[saveType];
            if (path.Contains(ext) == false) path += ext;

            if(preset != ConfigurePreset.Default) path = path.Insert(path.LastIndexOf('.'), preset.ToString());
            return path;
        }
    }

    public abstract class Configure<T> : MonoBehaviour, IConfigure<T>, IConfigureSerialize where T : new()
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

        [SerializeField] protected ConfigureSaveMode saveMode = ConfigureSaveMode.UseEditorValue;
        [SerializeField] protected FileTool.SerializerType saveType = FileTool.SerializerType.XML;
        [SerializeField] protected KeyCode openKey = KeyCode.None;
        [SerializeField] protected KeyCode saveKey = KeyCode.None;
        [SerializeField] protected KeyCode loadKey = KeyCode.None;
        [SerializeField] protected ConfigurePreset preset = ConfigurePreset.Default;

		[SerializeField] protected bool isSyncing = false;
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
                this.Load();
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

		public virtual byte[] OnSerialize()
		{
            return Serialization.ObjectToByteArray(JsonUtility.ToJson(this.D));
		}

		public virtual void OnDeserialize(byte[] data)
		{
            JsonUtility.FromJsonOverwrite(Serialization.ByteArrayToObject<string>(data), this.data);
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

                using (new GUITool.ColorScope(color))
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
        public static bool HasFlag<T>(Enum value, T flag)
        {
            Type underlyingType = Enum.GetUnderlyingType(value.GetType());

            // note: AsInt mean: math integer vs enum (not the c# int type)
            dynamic valueAsInt = Convert.ChangeType(value, underlyingType);
            dynamic flagAsInt = Convert.ChangeType(flag, underlyingType);

            if(flagAsInt == 0) return false;
            if(flagAsInt == ~0) return false;

            return (valueAsInt & flagAsInt) == flagAsInt;
        }
        public static T SetFlag<T>(Enum value, T flag, bool set)
        {
            Type underlyingType = Enum.GetUnderlyingType(value.GetType());

            // note: AsInt mean: math integer vs enum (not the c# int type)
            dynamic valueAsInt = Convert.ChangeType(value, underlyingType);
            dynamic flagAsInt = Convert.ChangeType(flag, underlyingType);
            if (set)
            {
                valueAsInt |= flagAsInt;
            }
            else
            {
                valueAsInt &= ~flagAsInt;
            }

            return (T)valueAsInt;
        }
        static public void OnGUIEnum<T>(ref T value, string displayName = null, GUILayoutOption[] op = null) where T : struct, System.Enum
        {
            // if(typeof(T).GetCustomAttributes<FlagsAttribute>().Any())
            // {
            //     using (var h = new GUILayout.HorizontalScope())
            //     {
            //         var labels = Enum.GetValues(typeof(T));
            //         foreach(T l in labels)
            //         {
            //             var v =  HasFlag(value, l);
            //             v = GUILayout.Toggle(v, l.ToString());
            //             value = SetFlag(value, l, v);
            //         }
            //     }
            // }
            // else
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