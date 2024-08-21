using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour    //标题与主场景切换，鼠标隐藏
{
    private AsyncOperation asyncLoad;

    bool isVisible = false;

    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "MainGame")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            asyncLoad = SceneManager.LoadSceneAsync("MainGame");    //预加载主场景
            asyncLoad.allowSceneActivation = false;
        }
    }

    void Update()
    {
        Scene scene = SceneManager.GetActiveScene();
        if(scene.name == "MainGame")
        {
            CursorControl();
        }
    }

    void CursorControl()    //主场景中esc隐藏，esc显示
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            if (isVisible == false)
            {
                isVisible = !isVisible;
                Cursor.visible = isVisible;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                isVisible = !isVisible;
                Cursor.visible = !isVisible;
                Cursor.lockState = CursorLockMode.Locked;
            }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); //退出游戏
#endif
    }
    public void StartGame()
    {
        asyncLoad.allowSceneActivation = true;
    }
}
