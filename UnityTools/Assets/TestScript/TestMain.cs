using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Test
{
    public class TestMain : MonoBehaviour
    {
        protected TestFramework testFramework;
        protected void Start()
        {
            this.testFramework = new TestFramework();
            this.testFramework.RunAll();
        }
    }
}