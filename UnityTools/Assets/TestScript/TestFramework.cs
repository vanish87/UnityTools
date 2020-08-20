using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Test
{
    public interface ITest
    {
        string Name { get; }
        void RunTest();
        void Report();
    }
    public class TestFramework
    {
        public void RunAll()
        {
            var types = ObjectTool.FindAllTypes<ITest>();

            foreach(var t in types)
            {
                var instance = System.Activator.CreateInstance(t) as ITest;

                LogTool.Log("Start Run" + instance.Name, LogLevel.Info);
                instance.RunTest();
                LogTool.Log("End Test Run" + instance.Name, LogLevel.Info);
                instance.Report();
            }
        }
    }
}
