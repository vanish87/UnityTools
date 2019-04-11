using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

namespace Test
{
    public class UserTexture : RenderTexture
    {
        RenderTexture dataRef = null;
        public UserTexture()
            :base(128, 128, 24)
        {
            Debug.Log("UserTexture constructor");
            this.dataRef = this;
        }

        ~UserTexture()
        {
            Debug.Log("UserTexture destructor");
            this.dataRef.Release();
        }
    }
    public class TextureLifeTimeTest : MonoBehaviour
    {
        [SerializeField] protected List<RenderTexture> list = new List<RenderTexture>();
        [SerializeField] protected RenderTexture t1copy;
        // Start is called before the first frame update
        void Start()
        {
            var t = new UserTexture();
            list.Add(t);
            list[0].IsCreated();
            list.Clear();

            list.Add(new RenderTexture(128, 128, 24));
            var t1 = list[0];
            list.Clear();

            RenderTexUtil.Clear(t1, Color.red);

            t1.Release();

            //GameObject.Destroy(t1);

            t1copy = new RenderTexture(t1);
            Graphics.CopyTexture(t1, t1copy);

            list.Add(t1);
        }

        private void OnApplicationQuit()
        {
            #if DEBUG
            RenderTexUtil.tracking.ReportTextures();
            #endif
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}