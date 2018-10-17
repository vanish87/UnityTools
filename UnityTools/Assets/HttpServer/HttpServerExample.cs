using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityTools.HttpServer
{
    public class HttpServerExample : MonoBehaviour
    {

        private HttpServer httpServer = null;
        // Use this for initialization
        void Start()
        {
            this.CreateAndSetup();
            this.httpServer.Run();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            this.httpServer.Stop();
        }

        private void CreateAndSetup()
        {
            if (this.httpServer == null)
            {
                string[] prefix = new string[] { "http://*:" + 20000 + "/" };
                this.httpServer = new HttpServer(prefix);

                var path = Path.Combine(Application.dataPath, "HttpServer/HttpData");
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                this.httpServer.DocumentRootPath = path;

                this.httpServer.RegisterHandler("GET", SendResponseFile);
            }
        }

        private void SendResponseFile(object sender, HttpServer.HttpEventArg args)
        {
            var request = args.context.Request;
            if (request == null) return;

            bool detailedLogging = true;

            string rawUrl = request.RawUrl;
            string path = args.docBasePath + rawUrl;

            if (detailedLogging)
            {
                Debug.LogFormat("IP {4} Requesting file: '{0}'. Relative url: {1} Full url: '{2} HttpDataDirectory: '{3}''",
                    path, request.RawUrl, request.Url, args.docBasePath, request.RemoteEndPoint.ToString());
            }
            else
            {
                Debug.LogFormat("Requesting file: '{0}' ... ", request.RawUrl);
            }

            var response = args.context.Response;
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    string mime;
                    string filename = Path.GetFileName(path);
                    //response is HttpListenerContext.Response...
                    response.ContentLength64 = fs.Length;
                    response.SendChunked = false;
                    response.ContentType = HttpServer.mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : System.Net.Mime.MediaTypeNames.Application.Octet;
                    response.ContentEncoding = System.Text.Encoding.Default;
                    response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";

                    byte[] buffer = new byte[64 * 1024];
                    int read = 0;
                    using (BinaryWriter bw = new BinaryWriter(response.OutputStream, System.Text.Encoding.Default, true))
                    {
                        //If the read operation is successful, 
                        //the current position of the stream is advanced by the number of bytes read.
                        //If an exception occurs, the current position of the stream is unchanged.
                        //so here always use 0 for offset
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, read);
                        }
                        bw.Flush(); //seems to have no effect

                        bw.Close();
                    }

                    response.OutputStream.Flush();
                    response.OutputStream.Close();

                    fs.Close();

                    Debug.LogFormat("Response as type {0}", response.ContentType);
                    Debug.Log(filename + " download completed.");
                }
            }
            catch (System.Exception exc)
            {
                Debug.Log("Error at " + DateTime.Now.ToShortTimeString());
                Debug.LogFormat("Requested file failed: '{0}'. Relative url: {1} Full url: '{2} AssetBundleDirectory: '{3}''", path, request.RawUrl, request.Url, args.docBasePath);
                Debug.LogFormat("Exception {0}: {1}'", exc.GetType(), exc.Message);
                response?.Abort();
            }
        }
    }
}