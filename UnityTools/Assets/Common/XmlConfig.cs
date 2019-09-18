using UnityEngine;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public interface IConfigure<T> where T : class, new()
{
    //not used
    void OnConfigureChange(XmlConfig<T> sender, EventArgs args);
}

//same as XmlConfig, but is not subclass of mono
//Note it could be multiple instance but they will save/load to same file
public abstract class XmlConfigNoneMono<T> where T : class, new()
{
    public abstract T Data { get; set; }
    public bool Open { get { return this.open; } set { this.open = value; } }
    protected virtual string filePath { get { return Path.Combine(Application.persistentDataPath, "config_" + SceneManager.GetActiveScene().name + ".xml"); } }

    protected bool inited = false;

    protected Rect windowRect = new Rect();
    protected bool open = false;
    protected virtual float MinWidth { get { return 500f; } }

    #region IConfigure
    protected event EventHandler OnConfigureChanged;
    public void RegisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; this.OnConfigureChanged += handler; }
    public void UnregisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; }
    protected void OnConfigureChange() { this.OnConfigureChanged?.Invoke(this, new EventArgs()); }

    protected event EventHandler OnConfigureGUI;
    public void RegisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; this.OnConfigureGUI += handler; }
    public void UnregisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; }
    protected void OnDrawConfigureGUI() { this.OnConfigureGUI?.Invoke(this, new EventArgs()); }
    #endregion

    protected virtual void OnDrawGUI()
    {

    }

    protected XmlConfigNoneMono()
    {
        this.Initialize();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            this.Open = !this.Open;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            this.LoadAndNotify();
        }
        if (Input.GetKeyDown(KeyCode.S))
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
                        this.OnDrawConfigureGUI();
                        GUI.DragWindow();
                    },
            "",
            GUILayout.MinWidth(MinWidth));
        }
    }
    public virtual void Save()
    {
        ConfigureTool.Write(this.filePath, this.Data);
    }
    protected virtual void Load()
    {
        this.Data = ConfigureTool.Read<T>(this.filePath);
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

public abstract class XmlConfigMonoSingleton<T> : SingletonMonoBehaviour<XmlConfigMonoSingleton<T>> where T : class, new()
{
    public abstract T Data { get; set; }
    public bool Open { get { return this.open; } set { this.open = value; } }
    protected virtual string filePath { get { return Path.Combine(Application.persistentDataPath, "config_" + SceneManager.GetActiveScene().name + ".xml"); } }

    protected bool inited = false;

    protected Rect windowRect = new Rect();
    protected bool open = false;
    protected virtual float MinWidth { get { return 500f; } }

    #region IConfigure
    protected event EventHandler OnConfigureChanged;
    public void RegisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; this.OnConfigureChanged += handler; }
    public void UnregisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; }
    protected void OnConfigureChange() { this.OnConfigureChanged?.Invoke(this, new EventArgs()); }

    protected event EventHandler OnConfigureGUI;
    public void RegisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; this.OnConfigureGUI += handler; }
    public void UnregisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; }
    protected void OnDrawConfigureGUI() { this.OnConfigureGUI?.Invoke(this, new EventArgs()); }
    #endregion

    protected virtual void OnDrawGUI()
    {

    }

    //awake will load configure form file
    protected virtual void Awake()
    {
        this.Initialize();
    }

    //halder will be registered on enabled
    //so at here call notify to call all handlers
    protected virtual void Start()
    {
        this.NotifyChange();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            this.Open = !this.Open;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            this.LoadAndNotify();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.Save();
        }
    }

    protected virtual void Initialize()
    {
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
                        this.OnDrawConfigureGUI();
                        GUI.DragWindow();
                    },
            "",
            GUILayout.MinWidth(MinWidth));
        }
    }


    public virtual void Save()
    {
        ConfigureTool.Write(this.filePath, this.Data);
    }
    protected virtual void Load()
    {
        this.Data = ConfigureTool.Read<T>(this.filePath);
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

public abstract class XmlConfig<T> : MonoBehaviour where T : class, new()
{
    public abstract T Data { get; set; }
    public bool Open { get { return this.open; } set { this.open = value; } }
    protected virtual string filePath { get { return Path.Combine(Application.persistentDataPath, "config_" + SceneManager.GetActiveScene().name + ".xml"); } }

    protected bool inited = false;

    protected Rect windowRect = new Rect();
    protected bool open = false;
    protected virtual float MinWidth { get { return 500f; } }

    #region IConfigure
    protected event EventHandler OnConfigureChanged;
    public void RegisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; this.OnConfigureChanged += handler; }
    public void UnregisterOnConfigureChanged(EventHandler handler) { this.OnConfigureChanged -= handler; }
    protected void OnConfigureChange() { this.OnConfigureChanged?.Invoke(this, new EventArgs()); }

    protected event EventHandler OnConfigureGUI;
    public void RegisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; this.OnConfigureGUI += handler; }
    public void UnregisterOnDrawConfigureGUI(EventHandler handler) { this.OnConfigureGUI -= handler; }
    protected void OnDrawConfigureGUI() { this.OnConfigureGUI?.Invoke(this, new EventArgs()); }
    #endregion

    protected virtual void OnDrawGUI()
    {

    }

    //awake will load configure form file
    protected virtual void Awake()
    {
        this.Initialize();
    }

    //handler will be registered on enabled
    //so at here call notify to call all handlers
    protected virtual void Start()
    {
        this.NotifyChange();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            this.Open = !this.Open;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            this.LoadAndNotify();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.Save();
        }
    }

    protected virtual void Initialize()
    {
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
                        this.OnDrawConfigureGUI();
                        GUI.DragWindow();
                    },
            "",
            GUILayout.MinWidth(MinWidth));
        }
    }


    public virtual void Save()
    {
        ConfigureTool.Write(this.filePath, this.Data);
    }
    protected virtual void Load()
    {
        this.Data = ConfigureTool.Read<T>(this.filePath);
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
public class ConfigureTool
{
    static public void Write<T>(string filePath, T data)
    {
        //System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
        using (var fs = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
        {
            //シリアル化し、XMLファイルに保存する
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(fs, data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }

            fs.Flush();
            fs.Close();
        };
    }

    static public void WriteBinary<T>(string filePath, T data)
    {
        using (var fs = new FileStream(filePath, FileMode.Create))
        {
            //シリアル化し、XMLファイルに保存する
            try
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(fs, data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }

            fs.Flush();
            fs.Close();
        };
    }
    static public T Read<T>(string filePath) where T : new()
    {
        var ret = default(T);
        if (File.Exists(filePath))
        {
            //読み込むファイルを開く
            //System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
            using (var xs = new StreamReader(filePath, System.Text.Encoding.UTF8))
            {
                //XmlSerializerオブジェクトを作成

                //XMLファイルから読み込み、逆シリアル化する
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    ret = (T)serializer.Deserialize(xs);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message + " " + filePath);
                }
            }
        }
        else
        {
            Debug.LogWarning(filePath + " not found, create new one");
            Write(filePath, new T());
        }

        return ret;
    }

    static public T ReadBinary<T>(string filePath)
    {
        var ret = default(T);
        if (File.Exists(filePath) == false) return ret;

        using (var fs = new FileStream(filePath, FileMode.Open))
        {
            //シリアル化し、XMLファイルに保存する
            try
            {
                var serializer = new BinaryFormatter();
                ret = (T)serializer.Deserialize(fs);
            }
            catch (Exception e)
            {
                Debug.LogWarning("File Name " + filePath);
                Debug.LogWarning(e.Message);
            }

            fs.Flush();
            fs.Close();
        };

        return ret;
    }
}

public class ConfigureGUI
{
    static Dictionary<int, string> stringTable = new Dictionary<int, string>();
    public class ColorScope : IDisposable
    {
        Color _color;
        public ColorScope(Color color)
        {
            _color = GUI.color;
            GUI.color = color;
        }

        public void Dispose()
        {
            GUI.color = _color;
        }
    }

    static public void OnGUI<T>(ref T value, string displayName = null)
    {
        var op = new[] { GUILayout.MinWidth(70f) };
        using (var h = new GUILayout.HorizontalScope())
        {
            GUILayout.Label(displayName == null ? nameof(value) : displayName);
            var hash = value.GetHashCode();
            var target = value.ToString();
            var hasUnparsedStr = stringTable.ContainsKey(hash);
            if (hasUnparsedStr)
            {
                target = stringTable[hash];
            }
            var color = hasUnparsedStr ? Color.red : GUI.color;

            using (var cs = new ColorScope(color))
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
