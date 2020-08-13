using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Date
{
    public static string AssetBundlesPackagingPath = "Assets/AssetBundles";

    public static string AssetBundlesPath
    {
        get
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return Application.persistentDataPath;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                //return "jar:file://" + Application.persistentDataPath + "!/assets/AssetBundles/";
                return Application.persistentDataPath;
            }
            else
            {
                return null;
            }
        }
    }

    private static TextAsset csv;
    public static TextAsset CSV
    {
        get { return csv; }
        set { csv = value; }
    }

    public static AssetBundle CSVAssetBundle { get; set; }
}
