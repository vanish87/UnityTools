using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

namespace UnityTools.Common
{
    public class FileTool
    {
        static public void WriteXML<T>(string filePath, T data)
        {
            //System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
            using (var fs = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                //シリアル化し、XMLファイルに保存する
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(fs, data);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e.Message);
                }

                fs.Flush();
                fs.Close();
            };
        }

        static public void WriteBinary<T>(string filePath, T data)
        {
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                //シリアル化し、XMLファイルに保存する
                try
                {
                    var serializer = new BinaryFormatter();
                    serializer.Serialize(fs, data);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e.Message);
                }

                fs.Flush();
                fs.Close();
            };
        }
        static public T ReadXML<T>(string filePath) where T : new()
        {
            var ret = default(T);
            if (File.Exists(filePath))
            {
                //読み込むファイルを開く
                //System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
                using (var xs = new StreamReader(filePath, System.Text.Encoding.UTF8))
                {
                    //XmlSerializerオブジェクトを作成

                    //XMLファイルから読み込み、逆シリアル化する
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        ret = (T)serializer.Deserialize(xs);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message + " " + filePath);
                    }
                }
            }
            else
            {
                Debug.LogWarning(filePath + " not found, create new one");
                WriteXML(filePath, new T());
            }

            return ret;
        }

        static public T ReadBinary<T>(string filePath)
        {
            var ret = default(T);
            if (File.Exists(filePath) == false) return ret;

            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                //シリアル化し、XMLファイルに保存する
                try
                {
                    var serializer = new BinaryFormatter();
                    ret = (T)serializer.Deserialize(fs);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("File Name " + filePath);
                    Debug.LogWarning(e.Message);
                }

                fs.Flush();
                fs.Close();
            };

            return ret;
        }
    }
}