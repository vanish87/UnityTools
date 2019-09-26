using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityTools.ComputeShaderTool
{
    public class ComputeShaderParameterManager : Common.SystemSingleton<ComputeShaderParameterManager>
    {
        //path after "Assets"
        public List<string> nameList = new List<string>()
        {
            "ShaderParameter/csVariable.txt"
        };

        protected List<ComputeShaderParameterParser> computeShaderList = new List<ComputeShaderParameterParser>();

        protected ComputeShaderParameterManager()
        {
            this.ParseAllCSFiles();
        }

        public void ParseAllCSFiles()
        {
            var shaderBasePath = Application.dataPath;

            foreach (var f in nameList)
            {
                var file = Path.Combine(shaderBasePath, f);
                computeShaderList.Add(new ComputeShaderParameterParser(file));
            }
        }
    }
    public class ComputeShaderParameterParser
    {
        public class Configure
        {
            //Folder name under Resource folder
            public const string ResourcePath = "ComputeShaderVaribles";

            public static readonly List<string> VarNameList = new List<string>()
            {
                "int","int1","int2","int3","int4",
                "float","float1","float2","float3","float4",
                "float3x3","float4x4",

                "Texture2D",
                "StructuredBuffer",
                "RWStructuredBuffer",
            };
        }

        public string Name { get { return this.computeShaderName; } }
        public Dictionary<string, List<string>> ComputeShaderVariables { get { return this.computerShaderVariables; } }

        protected string computeShaderName;
        protected Dictionary<string, List<string>> computerShaderVariables = new Dictionary<string, List<string>>();
        protected Dictionary<string, ComputeShaderParameterBase> computeShaderParameter = new Dictionary<string, ComputeShaderParameterBase>();

        public ComputeShaderParameterParser(string filePath)
        {
            this.computeShaderName = Path.GetFileNameWithoutExtension(filePath);

            this.OutputTextAsset(filePath);
            this.Generate();
        }

        protected void Generate()
        {
            foreach (var p in this.computerShaderVariables)
            {
                var varType = p.Key;
                var index = p.Key.IndexOf('<');
                if (index != -1)
                {
                    varType = varType.Substring(index);
                }

                switch(varType)
                {
                    case "StructuredBuffer":
                    case "RWStructuredBuffer":
                        {
                            foreach (var name in p.Value)
                            {
                                this.computeShaderParameter.Add(name, new ComputeShaderParameterBuffer(name));
                            }
                        }
                        break;
                    case "Texture2D":
                        {
                            foreach (var name in p.Value)
                            {
                                this.computeShaderParameter.Add(name, new ComputeShaderParameterTexture(name));
                            }
                        }
                        break;
                    case "float3x3":
                    case "float4x4":
                        {
                            foreach (var name in p.Value)
                            {
                                this.computeShaderParameter.Add(name, new ComputeShaderParameterMatrix(name));
                            }
                        }
                        break;
                    case "int":
                    case "int1":
                    case "float":
                    case "float1":
                        {
                            foreach (var name in p.Value)
                            {
                                this.computeShaderParameter.Add(name, new ComputeShaderParameterFloat(name));
                            }
                        }
                        break;
                    case "int2":
                    case "int3":
                    case "float2":
                    case "float3":
                        {
                            Debug.LogWarning("Vector should be vecotr4 only");
                            goto case "int4";
                        }
                    case "int4":
                    case "float4":
                        {
                            foreach (var name in p.Value)
                            {
                                this.computeShaderParameter.Add(name, new ComputeShaderParameterVector(name));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        

        protected void OutputTextAsset(string filePath)
        {
            var output = "";
            if(File.Exists(filePath))
            {
                functionStack.Clear();

                var all = File.ReadAllLines(filePath);
                var last = "";
                foreach (var s in all)
                {
                    //Debug.Log(s);
                    output += GetVariables(last + s, ref last);
                }
            }
            else
            {
                Debug.LogWarning("File not found" + filePath);
            }

            this.computerShaderVariables = HandleVariables(output);

            var name = Path.GetFileName(filePath);

            var shaderBasePath = Application.streamingAssetsPath;
            var outputPath = Path.Combine(shaderBasePath, Configure.ResourcePath);

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            var fi = new FileInfo(Path.Combine(outputPath, name));

            using (var binaryFile = fi.Create())
            {
                binaryFormatter.Serialize(binaryFile, this.computerShaderVariables);
                binaryFile.Flush();
            }

            Dictionary<string, List<string>> readBack;
            using (var binaryFile = fi.OpenRead())
            {
                readBack = (Dictionary<string, List<string>>)binaryFormatter.Deserialize(binaryFile);
            }

            foreach (var d in readBack)
            {
                Debug.Log("key " + d.Key);
                foreach (var v in d.Value)
                {
                    Debug.Log(" value " + v);
                }
            }

            //Assert.IsTrue(this.computerShaderVariables == readBack);
        }

        protected Stack<string> functionStack = new Stack<string>();
        protected string GetVariables(string text, ref string last)
        {
            last = "";
            if (text.Contains("//"))
            {
                var start = text.IndexOf("//");
                var end = text.Length;
                text = text.Remove(start, end - start);
            }

            var next = 0;
            if(text.Contains("{"))
            {
                functionStack.Push("{");
                next = text.IndexOf("{")+1;
            }

            if (functionStack.Count > 0)
            {
                while (next < text.Length)
                {
                    var end = text.IndexOf("}", next);
                    if (end != -1)
                    {
                        functionStack.Pop();
                        next = end + 1;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            else
            {
                if(this.IsFunction(text))
                {
                    return "";
                }
                if(text.Contains("#"))
                {
                    return "";
                }
                if (text.Contains(";"))
                {
                    return text + "\n";
                }
                else
                {
                    #if LIYUAN && DEBUG
                    Debug.LogWarning("skip" + text);
                    #endif
                }
            }

            return "";
        }
        
        protected bool IsFunction(string text)
        {
            if(text.Contains("(") || text.Contains(")"))
            {
                return true;
            }
            return false;
        }

        protected Dictionary<string, List<string>> HandleVariables(string varibleList)
        {
            var ret = varibleList.Split(new char[] { ' ', ',', ';', '\n', '\t', '='}, System.StringSplitOptions.RemoveEmptyEntries);

            var current = "";

            var dic = new Dictionary<string, List<string>>();
            foreach(var v in ret)
            {
                var typeName = v.Split(new char[] { '<', '>', ' ', '\n', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);
                var found = false;
                foreach (var name in Configure.VarNameList)
                {
                    if (typeName[0].Contains(name))
                    {
                        found = true;
                        break;
                    }
                }
                if (current != v)
                {
                    if (found)
                    {
                        current = v;
                        if (dic.ContainsKey(current) == false)
                        {
                            dic.Add(current, new List<string>());
                        }
                    }
                    else
                    {
                        var isNumeric = System.Text.RegularExpressions.Regex.IsMatch(v, @"^-?\d+(\.\d+)?f?$");
                        if (isNumeric == false)
                        {
                            dic[current].Add(v);
                        }
                        else
                        {
                            Debug.LogWarning(v + " is value");
                        }

                    }
                }

            }
            return dic;
        }


    }
}
