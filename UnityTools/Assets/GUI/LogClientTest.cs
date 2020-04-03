using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.GUITool.Test
{
    public class LogClientTest : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            this.StartCoroutine(Log());
        }

        IEnumerator Log()
        {
            yield return 0;
            while(true)
            {
                LogTool.Log("Test " + Time.time);
                yield return new WaitForSeconds(1);
            }
        }
    }
}
