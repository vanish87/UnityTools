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
			KeyCode OpenKey { get; }
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
                            GUILayout.Label("Window On/Off Key: " + this.key.ToString());
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
		protected List<IConfigure> configures = new List<IConfigure>();


		protected void Start()
		{
			foreach (var gui in ObjectTool.FindAllObject<IGUIHandler>())
			{
				this.windowData.Add(new WindowData() { title = gui.Title, key = gui.OpenKey });
			}

			this.configures = ObjectTool.FindAllObject<IConfigure>();

			foreach (var w in this.windowData)
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
		public KeyCode OpenKey => KeyCode.I;
		public void OnDrawGUI()
		{
			foreach (var w in this.windowData)
			{
				var label = "Use " + w.key.ToString() + " key " + w.title;
				w.open = GUILayout.Toggle(w.open, label);
			}

			foreach (var c in this.configures)
			{
				var label = "Use " + c.OpenKey.ToString() + " key " + c.ToString();
				c.IsOpen = GUILayout.Toggle(c.IsOpen, label);
			}

		}

	}
}
