
using UnityEngine;

namespace UnityTools.GUITool
{
    public class GUIContainerExample: MonoBehaviour
    {
        [System.Serializable]
        public class Data: GUIContainer
        {
            public bool boolField = true;
            public int intField = 10;
            public int intField1 = 10;
            protected string stringField = "TestString";
            private Vector4 vec = new Vector4(1,2,3,4);
            [GUIMenu(DisplayName="Nice Name")]public Vector4 vector1 = new Vector4(4,3,2,1);


            [NoneVariable] protected string NoneGUIField = "NONEGUI";

            [Shader(Name="_Test")] protected float csFoloat = 10;
            [Shader(Name="_Test")] public GPUBufferVariable<int> csBuffer;
            [Shader(Name="_Test")] public Texture texture;
            [Shader(Name="_Test")] public Texture2D texture1;
            [Shader(Name="_Test")] public RenderTexture texture2;

        }

        [SerializeField] protected Data data = new Data();


        protected void Start()
        {
            // this.data = new Data();
            this.data.intField = 100;
            this.data.csBuffer.InitBuffer(10, true);
            // this.data.vector1 = new Vector4(5,6,7,8);


            var a1 = 10;
            var a2 = 10;

            var r1 = __makeref(a1);
            var r2 = __makeref(a2);


            // this.data.csBuffer.SetToGPU(this.data, null);

            foreach(var v in this.data.VariableList)
            {
                // (v as VariableContainer.GPUVariable)?.SetToGPU(this.data,null);
            }
        }

        protected void OnDestroy()
        {
            this.data.csBuffer.Release();
        }


        protected void OnGUI()
        {
            // ConfigureGUI.OnGUISlider(ref this.data.intField,0, 100, "My int");
            // ConfigureGUI.OnGUI(ref this.data.intField, "My int");
            this.data.OnGUI();
            if(GUILayout.Button("Default"))
            {
                this.data.ResetToDefault();
            }
        }

    }
}