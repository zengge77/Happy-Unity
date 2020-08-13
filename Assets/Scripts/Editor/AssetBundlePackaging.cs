using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundlePackaging 
{
    [MenuItem("Assets/Bulid AssetBundle")]
    static void BulidAssetBundle()
    {
        if (!Directory.Exists(Date.AssetBundlesPackagingPath))
        {
            Directory.CreateDirectory(Date.AssetBundlesPackagingPath);
        }

        BuildPipeline.BuildAssetBundles(Date.AssetBundlesPackagingPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
