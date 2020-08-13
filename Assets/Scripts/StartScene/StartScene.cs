using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Networking;
//using System.Net;

public class StartScene : MonoBehaviour
{
    public GameObject mainPlane;
    public Button selectButton;
    public Button updateButton;
    public Text log;

    public GameObject selectPlane;
    public GameObject node;
    public Button backButton;
    public Button buttonPrefab;

    private void OnEnable()
    {
        selectButton.onClick.AddListener(LoadSelectPlane);
        backButton.onClick.AddListener(BackSelectPlane);
        updateButton.onClick.AddListener(UpdateAssetBundle);
    }

    private void OnDisable()
    {
        selectButton.onClick.RemoveListener(LoadSelectPlane);
        backButton.onClick.RemoveListener(BackSelectPlane);
        updateButton.onClick.RemoveListener(UpdateAssetBundle);
    }

    void LoadSelectPlane()
    {
        mainPlane.GetComponent<Animator>().SetTrigger("Esc");
        selectPlane.GetComponent<Animator>().SetTrigger("Enter");
        StartCoroutine(UpdateSelectPlane());
    }

    void BackSelectPlane()
    {
        mainPlane.GetComponent<Animator>().SetTrigger("Enter");
        selectPlane.GetComponent<Animator>().SetTrigger("Esc");
    }

    void UpdateAssetBundle()
    {
        StartCoroutine(UpdateAssetBundleFromNetWork());
    }

    void UpdateLog(string value)
    {
        log.text += "\n";
        log.text += value;
    }

    /// <summary>
    /// 给ab包赋值
    /// </summary>
    void LoadAssetBundle()
    {
        if (Date.CSVAssetBundle != null)
        {
            Date.CSVAssetBundle.Unload(true);
        }
        Date.CSVAssetBundle = AssetBundle.LoadFromFile(Date.AssetBundlesPath + "/csv");
    }

    /// <summary>
    /// 将ab包写入到持久化文件夹下
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    IEnumerator WriteAssetBundle(string path)
    {
        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();
        byte[] dates = request.downloadHandler.data;
        File.WriteAllBytes(Date.AssetBundlesPath + "/csv", dates);

        UpdateLog("ab包更新完毕");
    }

    /// <summary>
    /// 将txt版本信息写入到持久化文件夹下
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    IEnumerator WriteVersionTXT(string value)
    {
        string path = Date.AssetBundlesPath + "/Version.txt";
        StreamWriter write = new StreamWriter(path, false);
        write.WriteLine(value);
        write.Close();

        UpdateLog("版本文件更新完毕");
        yield return null;
    }

    /// <summary>
    /// 更新关卡选择面板
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateSelectPlane()
    {
        //如果持久化文件夹下没有ab文件，将本地的ab包和版本文件复制到持久化文件夹下
        if (!File.Exists(Date.AssetBundlesPath + "/csv"))
        {
            UpdateLog(Date.AssetBundlesPath + "  持久化文件夹下未找到ab包文件");
            yield return StartCoroutine(CreatAssetBundleFromFile());
        }

        LoadAssetBundle();
        CreatButtons();
    }

    /// <summary>
    /// 从streamingAssetsPath下复制ab包到persistentDataPath中
    /// </summary>
    /// <returns></returns>
    IEnumerator CreatAssetBundleFromFile()
    {
        UpdateLog("开始从本地更新ab包");

        //写入ab包
        yield return StartCoroutine(WriteAssetBundle(Application.streamingAssetsPath + "/csv"));

        //写入版本文件
        StreamReader read = new StreamReader(Application.streamingAssetsPath + "/Version.txt");
        string version = read.ReadLine();
        read.Close();
        yield return StartCoroutine(WriteVersionTXT(version));
    }

    /// <summary>
    /// 创建选择面板的按钮，绑定从ab包中加载的csv文件
    /// </summary>
    void CreatButtons()
    {
        TextAsset[] csvs = Date.CSVAssetBundle.LoadAllAssets<TextAsset>();
        int num = csvs.Length;
        int oldNum = node.transform.childCount;
        for (int i = 0; i < node.transform.childCount; i++)
        {
            Destroy(node.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < num; i++)
        {
            Button newButton = Instantiate(buttonPrefab, node.transform);
            newButton.GetComponentInChildren<Text>().text = (i + 1).ToString();
            //绑定
            newButton.GetComponent<SelectLevelButton>().BindingCSV(csvs[i]);
        }
    }

    /// <summary>
    /// 从网络更新ab包
    /// </summary>
    IEnumerator UpdateAssetBundleFromNetWork()
    {
        UpdateLog("开始从网络更新ab包");

        //检查网络，连接百度
        string baiduUrl = "https://www.baidu.com/";
        UnityWebRequest baiduRequest = UnityWebRequest.Get(baiduUrl);
        yield return baiduRequest.SendWebRequest();
        if (baiduRequest.isHttpError || baiduRequest.isNetworkError)
        {
            UpdateLog(baiduRequest.error);
            baiduRequest.Dispose();
        }
        else
        {
            UpdateLog("网络连接至百度正常");
        }

        //获取现在的版本
        StreamReader read = new StreamReader(Application.persistentDataPath + "/Version.txt");
        string oldVersion = read.ReadLine();
        read.Close();
        UpdateLog("现在版本为： " + oldVersion);

        //获取网络上的版本
        string newVersion = string.Empty;
        string versionUrl = "https://zengge77.github.io/resources/Happy/Version.txt";
        UnityWebRequest versionRequest = UnityWebRequest.Get(versionUrl);
        yield return versionRequest.SendWebRequest();
        if (versionRequest.isHttpError || versionRequest.isNetworkError)
        {
            UpdateLog(versionRequest.error);
        }
        else
        {
            //获取txt并读取内容
            newVersion = versionRequest.downloadHandler.text;
            UpdateLog("网络版本为： " + newVersion);
        }

        //版本对比，更新ab包和版本文件
        if (int.Parse(oldVersion) < int.Parse(newVersion))
        {
            UpdateLog("发现新版本");

            //更新ab包
            string assetBundleUrl = "https://zengge77.github.io/resources/Happy/csv";
            yield return StartCoroutine(WriteAssetBundle(assetBundleUrl));

            //更新版本文件
            yield return StartCoroutine(WriteVersionTXT(newVersion));
        }
        else
        {
            UpdateLog("已经是最新版本");
        }
    }
}
