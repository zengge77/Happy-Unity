using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameLogic gameLogic;

    public Button RestartButton;
    public Button BackStartSceneButton;

    private void OnEnable()
    {
        RestartButton.onClick.AddListener(Reset);
        BackStartSceneButton.onClick.AddListener(LoadStartScene);
    }

    private void OnDisable()
    {
        RestartButton.onClick.RemoveListener(Reset);
        BackStartSceneButton.onClick.RemoveListener(LoadStartScene);
    }

    void Reset()
    {
        gameLogic.CreatrCandyArea();
    }

    void LoadStartScene()
    {
        SceneManager.LoadScene(0);
    }
}
