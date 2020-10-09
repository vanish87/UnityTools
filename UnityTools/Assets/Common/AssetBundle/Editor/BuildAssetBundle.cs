using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Common
{
    public class BuildAssetBundle
    {
        [MenuItem("Assets/Build ExampleAsset to asset")]
        static public void BuildToAssets()
        {
            //this is in the project directory
            var basePath = "Assets/AssetBundleOutput/SingleAssets";
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var a = new ExternalStruct();
            a.name = "test";

            var data = ExampleAsset.CreateInstance<ExampleAsset>();
            data.UpdateFields(a);


            var outputPath = basePath + "/" + data.assetName + ".asset";

            Debug.LogFormat("outputPath: {0}", outputPath);

            // Create an asset.
            AssetDatabase.CreateAsset(data, outputPath);
            AssetDatabase.AddObjectToAsset(data.texture, data);

            // Save the generated mesh asset.
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/Pack AssetBundle")]
        static public void PackAssetBundle()
        {
            var fileList = new List<string>();
            var assetBasePath = Application.dataPath;
            var assetDirectory = System.IO.Path.Combine(assetBasePath, "AssetBundleOutput/SingleAssets");
            foreach (string file in System.IO.Directory.GetFiles(assetDirectory))
            {
                if (System.IO.Path.GetExtension(file) == ".asset")
                {
                    var relativepath = file;
                    if (file.StartsWith(assetBasePath))
                    {
                        relativepath = "Assets" + file.Substring(assetBasePath.Length);
                    }
                    fileList.Add(relativepath);
                }
            }

            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = "example_asset_bundle";
            buildMap[0].assetNames = fileList.ToArray();

            string assetBundleDirectory = "Assets/" + "AssetBundleOutput/PackedAssetBundle";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
    }
}