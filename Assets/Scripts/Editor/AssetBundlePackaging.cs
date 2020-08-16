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
        //if (!Directory.Exists(Date.AssetBundlesPackagingPathWithWindows))
        //{
        //    Directory.CreateDirectory(Date.AssetBundlesPackagingPathWithWindows);
        //}
        //BuildPipeline.BuildAssetBundles(Date.AssetBundlesPackagingPathWithWindows, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        //if (!Directory.Exists(Date.AssetBundlesPackagingPathWithAndroid))
        //{
        //    Directory.CreateDirectory(Date.AssetBundlesPackagingPathWithAndroid);
        //}
        //BuildPipeline.BuildAssetBundles(Date.AssetBundlesPackagingPathWithAndroid, BuildAssetBundleOptions.None, BuildTarget.Android);

        if (!Directory.Exists(Date.AssetBundlesPackagingPathWithOriginal))
        {
            Directory.CreateDirectory(Date.AssetBundlesPackagingPathWithOriginal);
        }
        BuildPipeline.BuildAssetBundles(Date.AssetBundlesPackagingPathWithOriginal, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
