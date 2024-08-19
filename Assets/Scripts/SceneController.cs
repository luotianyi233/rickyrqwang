using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private AsyncOperation asyncLoad;

    bool isVisible = false;

    // Start is called before the first frame update
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

            asyncLoad = SceneManager.LoadSceneAsync("MainGame");
            asyncLoad.allowSceneActivation = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Scene scene = SceneManager.GetActiveScene();
        if(scene.name == "MainGame")
        {
            CursorControl();
        }
    }

    void CursorControl()
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
        Application.Quit(); //ÍË³öÓÎÏ·
#endif
    }
    public void StartGame()
    {
        //SceneManager.LoadScene("MainGame");
        asyncLoad.allowSceneActivation = true;
    }
}
