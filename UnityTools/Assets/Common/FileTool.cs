using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    public class FileTool
    {
        public enum SerializerType
        {
            XML,
            Json,
            Binary,
        }
        /// <summary>
        /// if target is not exist, rename source to target,
        /// else replace target file with source file
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void ReplaceOrRename(string target, string source)
        {
            var file = File.GetAttributes(source);
            if (file.HasFlag(FileAttributes.Directory))
            {
                try
                {
                    Directory.Move(source, target);
                }
                catch (Exception e)
                {
                    LogTool.Log("Error on Replace Folder " + target, LogLevel.Error);
                    LogTool.Log(e.ToString(), LogLevel.Error);
                }
            }
            else
            {
                try
                {
                    if (File.Exists(target))
                    {
                        File.Delete(target);
                    }
                    File.Move(source, target);
                }
                catch (Exception e)
                {
                    LogTool.Log("Error on Replace file " + target, LogLevel.Error);
                    LogTool.Log(e.ToString(), LogLevel.Error);
                }
            }
        }
        public static string GetTempFileName(string filePath)
        {
            return filePath + ".temp";
        }

        static public void Write<T>(string filePath, T data, SerializerType type = SerializerType.Binary)
        {
            var temp = GetTempFileName(filePath);
            bool ret = false;

            try
            {
                switch (type)
                {
                    case SerializerType.XML:
                        {
                            using (var fs = new StreamWriter(temp, false, System.Text.Encoding.UTF8))
                            {
                                var serializer = new XmlSerializer(typeof(T));
                                serializer.Serialize(fs, data);
                                fs.Flush();
                                fs.Close();
                            }
                        }
                        break;
                    case SerializerType.Json:
                        {
                            using (var fs = new StreamWriter(temp, false, System.Text.Encoding.UTF8))
                            {
                                var json = JsonUtility.ToJson(data, true);
                                fs.Write(json);
                                fs.Flush();
                                fs.Close();
                            }
                        }
                        break;
                    case SerializerType.Binary:
                        {
                            using (var fs = new FileStream(temp, FileMode.Create))
                            {
                                var serializer = new BinaryFormatter();
                                serializer.Serialize(fs, data);
                                fs.Flush();
                                fs.Close();
                            }
                        }
                        break;
                }
                ret = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                ret = false;
            }

            try
            {
                if(ret)
                {
                    ReplaceOrRename(filePath, temp);
                }
                else
                {
                    File.Delete(temp);
                }
            }
            catch(Exception e)
            {
                File.Delete(temp);
                LogTool.Log(e.Message, LogLevel.Warning, LogChannel.IO);
            }
        }       

        
        static public T Read<T>(string filePath, SerializerType type = SerializerType.Binary)
        {
            var ret = default(T);
            #if UNITY_ANDROID && !UNITY_EDITOR
            if(true)
            #else
            if (File.Exists(filePath))
            #endif
            {
                try
                {

                    switch (type)
                    {
                        case SerializerType.XML:
                            {
                                #if UNITY_ANDROID && !UNITY_EDITOR
                                var web = UnityEngine.Networking.UnityWebRequest.Get(filePath);
                                web.SendWebRequest();
                                while(!web.isDone);
                                LogTool.AssertIsFalse(web.isNetworkError);
                                LogTool.AssertIsFalse(web.isHttpError);
                                var text = web.downloadHandler.data;
                                web.Dispose();
                                using(var fs = new MemoryStream(text))
                                #else
                                using (var fs = new StreamReader(filePath, System.Text.Encoding.UTF8))
                                #endif
                                {
                                    var serializer = new XmlSerializer(typeof(T));
                                    ret = (T)serializer.Deserialize(fs);
                                    fs.Close();
                                }
                            }
                            break;
                        case SerializerType.Json:
                            {
                                using (var fs = new StreamReader(filePath, System.Text.Encoding.UTF8))
                                {
                                    var json = fs.ReadToEnd();
                                    ret = JsonUtility.FromJson<T>(json);
                                    fs.Close();
                                }
                            }
                            break;
                        case SerializerType.Binary:
                            {
                                using (var fs = new FileStream(filePath, FileMode.Open))
                                {
                                    var serializer = new BinaryFormatter();
                                    ret = (T)serializer.Deserialize(fs);
                                    fs.Flush();
                                    fs.Close();
                                }
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    LogTool.Log(e.Message + " " + filePath, LogLevel.Warning, LogChannel.IO);
                }
            }
            else
            {
                LogTool.Log(filePath + " not found", LogLevel.Warning, LogChannel.IO);
            }

            return ret;
        }
    }
}