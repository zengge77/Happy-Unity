using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Date
{
    public static string AssetBundlesPackagingPathWithWindows = "Assets/AssetBundles/Windows";
    public static string AssetBundlesPackagingPathWithAndroid = "Assets/AssetBundles/Android";
    public static string AssetBundlesPackagingPathWithOriginal = "Assets/AssetBundles/Original";

    public static string AssetBundlesPath
    {
        get
        {
            return Application.persistentDataPath;
            //if (Application.platform == RuntimePlatform.WindowsEditor)
            //{
            //    return Application.persistentDataPath;
            //}
            //else if (Application.platform == RuntimePlatform.Android)
            //{
            //    return Application.persistentDataPath;
            //}
        }
    }

    public static AssetBundle CSVAssetBundle { get; set; }
    public static TextAsset CSV { get; set; }
}
