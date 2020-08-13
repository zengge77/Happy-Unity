using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectLevelButton : MonoBehaviour
{
    private TextAsset csv;

    public void LoadGameScene()
    {
        Date.CSV = csv;
        SceneManager.LoadScene(1);
    }

    public void BindingCSV(TextAsset csv)
    {
        this.csv = csv;
    }

}
