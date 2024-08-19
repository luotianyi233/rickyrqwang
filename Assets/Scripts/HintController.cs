using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintController : MonoBehaviour
{
    public Text subtitleText;
    public string[] Hints;
    public float displayTime = 3.0f;
    public float delayBetweenSubtitles = 1.0f;
    private bool isRead = false;
    public bool isVictory = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isRead)
        {
            StartCoroutine(DisplaySubtitles());
            isRead = true;
        }
    }

    // Update is called once per frame
    private IEnumerator DisplaySubtitles()
    {
        foreach (string subtitle in Hints)
        {
            subtitleText.text = subtitle;
            yield return new WaitForSeconds(displayTime);
            subtitleText.text = "";
            yield return new WaitForSeconds(delayBetweenSubtitles);
            if (isVictory)
            {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit(); //ÍË³öÓÎÏ·
#endif
            }
        }
    }
}
