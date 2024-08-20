using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.BoolParameter;

public class RefreshRespawnPoint : MonoBehaviour
{
    private BoxCollider boxCollider;
    public PlayerController P1;
    public PlayerController P2;
    private bool haveEntered;
    public float displayTime = 3.0f;
    public Text text;
    public string[] texts;

    void Start()
    {
       haveEntered = false;
       boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!haveEntered && other.tag == "Player")
        {
            StartCoroutine(DisplaySubtitles());
            P1.Save();
            P2.Save();
            haveEntered = true;
            
        }
    }

    private IEnumerator DisplaySubtitles()
    {
        foreach (string subtitle in texts)
        {
            text.text = subtitle;
            yield return new WaitForSeconds(displayTime);
            text.text = "";
        }
    }
}
