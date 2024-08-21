using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour    //�������������л����������
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

            asyncLoad = SceneManager.LoadSceneAsync("MainGame");    //Ԥ����������
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

    void CursorControl()    //��������esc���أ�esc��ʾ
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
        Application.Quit(); //�˳���Ϸ
#endif
    }
    public void StartGame()
    {
        asyncLoad.allowSceneActivation = true;
    }
}
