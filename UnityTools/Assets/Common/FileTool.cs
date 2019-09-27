using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

namespace UnityTools.Common
{
    public class FileTool
    {
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

        static public void WriteXML<T>(string filePath, T data)
        {
            var temp = GetTempFileName(filePath);
            bool ret = false;

            try
            {
                using (var fs = new StreamWriter(temp, false, System.Text.Encoding.UTF8))
                {   
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(fs, data);
                    fs.Flush();
                    fs.Close();
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

        static public void WriteBinary<T>(string filePath, T data)
        {
            var tempFile = GetTempFileName(filePath);
            bool ret = false;
            try
            {
                using (var fs = new FileStream(tempFile, FileMode.Create))
                {            
                    var serializer = new BinaryFormatter();
                    serializer.Serialize(fs, data);
                    fs.Flush();
                    fs.Close();
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
                if (ret)
                {
                    ReplaceOrRename(filePath, tempFile);
                }
                else
                {
                    File.Delete(tempFile);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        static public T ReadXML<T>(string filePath) where T : new()
        {
            var ret = default(T);
            if (File.Exists(filePath))
            {
                try
                {
                    using (var xs = new StreamReader(filePath, System.Text.Encoding.UTF8))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        ret = (T)serializer.Deserialize(xs);
                        xs.Close();                    }
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
                WriteXML(filePath, ret);
            }

            return ret;
        }

        static public T ReadBinary<T>(string filePath) where T : new()
        {
            var ret = default(T);
            if (File.Exists(filePath) == false) return new T();

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var serializer = new BinaryFormatter();
                    ret = (T)serializer.Deserialize(fs);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("File Name " + filePath);
                Debug.LogWarning(e.Message);
            }

            return ret;
        }
    }
}