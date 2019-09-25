using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityTools.Common
{
    public class AssetBundleDownload : MonoBehaviour
    {
        public enum LastErrorCode
        {
            OK,
            ERROR,
            WARNING,
            UNKNOWN,
            BUILDING,
            COPYING,
        }

        private AssetBundleManifest currentManifest = null;
        private string syncServer = "http://127.0.0.1:20001";

        //current downloaded asset bundles
        private List<AssetBundle> currentAssetBundle = new List<AssetBundle>();
        public List<AssetBundle> CurrentAssetBundles { get { return this.currentAssetBundle; } }

        private List<Coroutine> currentDownloads = new List<Coroutine>();

        #region event
        protected event System.EventHandler<AssetBundleEventArg> OnFinishDownload;
        public class AssetBundleEventArg : System.EventArgs { public string message; }
        public void RegisterOnFinishDownload(System.EventHandler<AssetBundleEventArg> handler) { this.OnFinishDownload -= handler; this.OnFinishDownload += handler; }
        public void UnregisterOnFinishDownload(System.EventHandler<AssetBundleEventArg> handler) { this.OnFinishDownload -= handler; }

        protected event System.EventHandler<AssetBundleEventArg> OnDownloadError;
        public void RegisterOnDownloadError(System.EventHandler<AssetBundleEventArg> handler) { this.OnDownloadError -= handler; this.OnDownloadError += handler; }
        public void UnregisterOnDownloadError(System.EventHandler<AssetBundleEventArg> handler) { this.OnDownloadError -= handler; }
        #endregion      

        int startCount = 0;
        bool updatePending = false;

        private LastErrorCode lastError = LastErrorCode.OK;
        private string lastErrorMessage;

        // Use this for initialization
        void Start()
        {
            this.SetupCache();
        }

        void StartDownload()
        {
            this.startCount = 0;

            this.lastError = LastErrorCode.OK;
            this.lastErrorMessage = "OK";

            if (this.currentDownloads.Count > 0)
            {
                Debug.LogWarning("downloading is in progress, stop all");
                foreach (var c in this.currentDownloads)
                {
                    StopCoroutine(c);
                }

                this.currentDownloads.Clear();
            }

            //first we should unload all assets and all loaded objects,
            //we need refresh objects in asset bundles
            foreach (var b in this.CurrentAssetBundles)
            {
                if (b == null)
                {
                    Debug.LogWarning("CurrentAssetBundles has null asset");
                }
                b?.Unload(true);
            }
            this.CurrentAssetBundles.Clear();


            this.currentDownloads.Add(StartCoroutine(this.GetAssetBundleManifest()));
        }

        // Update is called once per frame
        void Update()
        {
            if (updatePending)
            {
                updatePending = false;
                this.StartDownload();
            }
        }

        public List<Hash128> GetCurrentBundleHash()
        {
            var ret = new List<Hash128>();
            foreach (var name in this.currentManifest.GetAllAssetBundles())
            {
                ret.Add(this.currentManifest.GetAssetBundleHash(name));
            }
            return ret;
        }

        private IEnumerator GetAssetBundleManifest()
        {
            startCount++;
            //download special AssetsBundles first to get AssetBundleManifest
            var www = UnityWebRequestAssetBundle.GetAssetBundle(this.syncServer + "/PackedAssetBundle");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                this.lastError = LastErrorCode.ERROR;
                this.lastErrorMessage = www.error;
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                if (bundle != null)
                {
                    if (this.currentManifest != null)
                    {
                        DestroyImmediate(this.currentManifest, true);
                        this.currentManifest = null;
                    }
                    this.currentManifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    Debug.LogFormat("Manifest {0}", this.currentManifest?.name);

                    //we need manifest for later
                    bundle.Unload(false);

                    var co = StartCoroutine(this.GetAssetBundles());
                    this.currentDownloads.Add(co);
                    yield return co;
                }
            }
            //Debug.LogFormat("Manifest done");
            startCount--;

            //it is not necessary that using startCount to trace this
            //because  yield return Coroutine will do same thing
            //just for double safety.
            if (startCount == 0)
            {
                if (this.lastError == LastErrorCode.OK)
                {
                    Debug.LogFormat("download done");
                    this.OnFinishDownload?.Invoke(this, new AssetBundleEventArg() { message = this.lastErrorMessage });
                }
                else
                {
                    Debug.LogFormat("download Error {0}", this.lastErrorMessage);
                    this.OnDownloadError?.Invoke(this, new AssetBundleEventArg() { message = this.lastErrorMessage });
                }
            }
            else
            {
                Debug.Log("startCount Error");
            }
            yield return 0;
        }

        private IEnumerator GetBundle(string name, Hash128 hash)
        {
            startCount++;
            //download one AssetBundle and add to current bundle list
            var www = UnityWebRequestAssetBundle.GetAssetBundle(this.syncServer + "/" + name, hash, 0);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                this.lastError = LastErrorCode.ERROR;
                this.lastErrorMessage = www.error;
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                if (!this.CurrentAssetBundles.Contains(bundle))
                {
                    this.CurrentAssetBundles.Add(bundle);
                    this.lastErrorMessage += " " + hash;
                    Debug.LogFormat("Bundle added {0}", bundle.name);
                }
            }


            //Debug.LogFormat("GetBundle done");
            startCount--;
            yield return 0;
        }

        private IEnumerator GetAssetBundles()
        {
            if (this.currentManifest == null) yield break;
            startCount++;

            var list = this.currentManifest.GetAllAssetBundles();

            foreach (var asset in list)
            {
                //to compare them with cached version
                List<Hash128> listOfCachedVersions = new List<Hash128>();
                Caching.GetCachedVersions(asset, listOfCachedVersions);

                var currentHash = this.currentManifest.GetAssetBundleHash(asset);

                var co = StartCoroutine(this.GetBundle(asset, currentHash));
                this.currentDownloads.Add(co);
                yield return co;
            }

            //Debug.LogFormat("GetAssetBundles done");
            startCount--;
            yield return 0;
        }

        public void UpdateAsset()
        {
            this.updatePending = true;
        }

        private void SetupCache()
        {
            string folderName = "SnycedAssetBundle";
            var path = Path.Combine(Application.persistentDataPath, folderName);
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            Cache newCache = Caching.AddCache(path);

            if (newCache.valid)
            {
                Caching.currentCacheForWriting = newCache;
            }

            //delete oldest caching
            while (Caching.cacheCount > 5)
            {
                Caching.RemoveCache(Caching.GetCacheAt(1));
            }
        }
    }
}