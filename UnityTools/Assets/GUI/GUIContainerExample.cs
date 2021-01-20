
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
            [GUIDisplayName("Nice Name")]public Vector4 vector1 = new Vector4(4,3,2,1);


            [NoneGUI] protected string NoneGUIField = "NONEGUI";


        }

        [SerializeField] protected Data data = new Data();


        protected void Start()
        {
            this.data = new Data();
            this.data.intField = 100;
            // this.data.vector1 = new Vector4(5,6,7,8);


            var a1 = 10;
            var a2 = 10;

            var r1 = __makeref(a1);
            var r2 = __makeref(a2);

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