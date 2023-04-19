using System.Collections.Generic;
using System.Linq;
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
        [SerializeField, DisableEdit] protected float maxFrameTime = 0;
        [SerializeField, DisableEdit] protected Vector3Int fps;
        protected List<float> fpsBuffer = new List<float>();
        protected float fpsB, timeCounter;
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

            this.fps = this.Counter(this.staticsTime);
        }
        protected Vector3Int Counter(float timeUpdate)
        {
            int fpsBCount = fpsBuffer.Count;

            if (timeCounter <= timeUpdate)
            {
                timeCounter += Time.deltaTime;
                fpsBuffer.Add(1.0f / Time.deltaTime);
            }
            else
            {
                fps.x = Mathf.RoundToInt(fpsBuffer.Min());
                fps.z = Mathf.RoundToInt(fpsBuffer.Max());
                for (int f = 0; f < fpsBCount; f++) fpsB += fpsBuffer[f];
                fpsBuffer = new List<float> { 1.0f / Time.deltaTime };
                fpsB = fpsB / fpsBCount;
                fps.y = Mathf.RoundToInt(fpsB);
                fpsB = timeCounter = 0;
            }

            if (Time.timeScale == 1 && fps.y > 0) return fps;
            else return Vector3Int.zero;
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
                GUILayout.Label("MIN " + this.fps.x.ToString() + " | AVG " + this.fps.y.ToString() + " | MAX " + this.fps.z.ToString());
            }
            GUILayout.EndArea();
        }
    }
    public static class StFPS
    {

    }
}