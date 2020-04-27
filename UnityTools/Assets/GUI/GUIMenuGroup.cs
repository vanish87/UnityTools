using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTools.GUITool
{
    public class GUIMenuGroup : MonoBehaviour, GUIMenuGroup.IGUIHandler
    {
        public interface IGUIHandler
        {
            void OnDrawGUI();
            string Title { get; }
        }

        [Serializable]
        public class WindowData
        {
            public string title;
            public bool open = false;
            public KeyCode key = KeyCode.None;

            protected Rect windowRect = new Rect();
            protected List<IGUIHandler> guiHandlers = new List<IGUIHandler>();
            protected virtual float MinWidth { get { return 500f; } }

            public void InitHandlers()
            {
                this.guiHandlers.Clear();
                var data = ObjectTool.FindAllObject<IGUIHandler>().Where(h => h.Title == this.title);
                this.guiHandlers.AddRange(data);
            }

            public void Update()
            {
                if (Input.GetKeyDown(this.key))
                {
                    if (this.open == false)
                    {
                        this.InitHandlers();
                    }
                    this.open = !this.open;
                }
            }
            public void OnGUI()
            {
                if (this.open)
                {
                    this.windowRect =
                    GUILayout.Window(
                    //GUIUtil.ResizableWindow(
                        GetHashCode(), this.windowRect, (id) =>
                        {
                            foreach (var g in this.guiHandlers) g.OnDrawGUI();
                            GUI.DragWindow();
                        },
                "",
                new[] { GUILayout.MinWidth(MinWidth), GUILayout.MaxWidth(MinWidth + 100) });
                    ;
                }
            }
        }

        public enum WindowType
        {
            Info,
            Debug,
            ConfigureAdjust,
            Log,
        }

        [SerializeField] protected List<WindowData> windowData = new List<WindowData>();


        protected void Start()
        {
            if (this.windowData.Count == 0)
            {
                this.windowData.Add(new WindowData() { title = WindowType.Debug.ToString(), key = KeyCode.D });
            }

            this.windowData.Add(new WindowData() { title = WindowType.Info.ToString(), key = KeyCode.I });

            foreach(var w in this.windowData)
            {
                w.InitHandlers();
            }
        }

        protected void Update()
        {
            foreach (var w in this.windowData)
            {
                w.Update();
            }
        }

        protected void OnGUI()
        {
            foreach (var w in this.windowData)
            {
                w.OnGUI();
            }
        }

        public string Title => WindowType.Info.ToString();
        public void OnDrawGUI()
        {
            foreach(var w in this.windowData)
            {
                GUILayout.Label(w.title + " window uses " + w.key.ToString() + " Key");
            }
        }
    }
}
