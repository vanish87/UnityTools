using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Attributes;
using UnityTools.GUITool;

namespace UnityTools.Common
{
	[ExecuteAlways]
	public class AverageFPS : MonoBehaviour, GUIMenuGroup.IGUIHandler
	{
		[SerializeField, DisableEdit] protected float fpsAverage = 0;
		[SerializeField, DisableEdit] protected double frameRate = 0;
		[SerializeField, DisableEdit] protected int frameCount = 0;
		[SerializeField, DisableEdit] protected float elapsedTime = 0;
		public float staticsTime = 1f;
        public bool displayGUI = true;
		public string Title => this.ToString();
		public KeyCode OpenKey => KeyCode.None;
		protected void Update()
		{
			this.fpsAverage = Time.frameCount / Time.time;

			this.frameCount++;
			this.elapsedTime += Time.deltaTime;
			if (this.elapsedTime > this.staticsTime)
			{
				this.frameRate = System.Math.Round(this.frameCount / this.elapsedTime, 2, System.MidpointRounding.AwayFromZero);
				this.frameCount = 0;
				this.elapsedTime = 0;
			}
		}
		protected void OnGUI()
		{
            this.OnDrawGUI();
		}

		public void OnDrawGUI()
		{
			GUILayout.BeginArea(new Rect(Screen.width - 200, 0, 200, 100));
			ConfigureGUI.OnGUI(ref this.displayGUI, "On/Off FPS");
			if (this.displayGUI)
			{
				GUILayout.Label("Average FPS:" + this.fpsAverage);
				GUILayout.Label("Last " + this.staticsTime + " second(s) FPS:" + this.frameRate);
			}
			GUILayout.EndArea();
		}
	}
}