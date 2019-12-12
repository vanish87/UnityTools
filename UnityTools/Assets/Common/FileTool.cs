using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

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
        /// if target is not exsit, rename source to target,
        /// else replace target file with source file
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void ReplaceOrRename(string target, string source)
        {
            if (File.Exists(target))
            {
                File.Delete(target);
            }
            File.Move(source, target);
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
                                var json = JsonUtility.ToJson(data);
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
                Debug.Log(e.Message);
            }
        }       

        
        static public T Read<T>(string filePath, SerializerType type = SerializerType.Binary) where T : new()
        {
            var ret = default(T);
            if (File.Exists(filePath))
            {
                try
                {

                    switch (type)
                    {
                        case SerializerType.XML:
                            {
                                using (var fs = new StreamReader(filePath, System.Text.Encoding.UTF8))
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
                    Debug.LogWarning(e.Message + " " + filePath);
                }
            }
            else
            {
                Debug.LogWarning(filePath + " not found, create new one");
                ret = new T();
                Write(filePath, ret, type);
            }

            return ret;
        }
    }
}