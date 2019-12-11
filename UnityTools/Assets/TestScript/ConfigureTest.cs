using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityTools.Common;

public class ConfigureTest : MonoBehaviour
{
    public class TestConfiure
    {
        public Vector2 vector;
        public string str = "12345";
        public string str1 = "67890";
    }
    // Start is called before the first frame update
    void Start()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "test.xml");
        var data = new TestConfiure();

        //ConfigureTool.Write(path, data);

        var loadData = FileTool.Read<TestConfiure>(path);

        Debug.Log(loadData.vector);
        Debug.Log(loadData.str);
        Debug.Log(loadData.str1);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
