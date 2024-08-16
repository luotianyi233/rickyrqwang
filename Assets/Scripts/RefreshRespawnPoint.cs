using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshRespawnPoint : MonoBehaviour
{
    private BoxCollider boxCollider;
    public LevelProgressController LP;
    private bool haveEntered;

    void Start()
    {
       haveEntered = false;
       boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (haveEntered)
        {
            LP.Progress();
            haveEntered = true;
        }
    }
}
