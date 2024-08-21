using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallOutDetector : MonoBehaviour    //µô³öÆ½Ì¨¼ì²â
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            RespawnController.Instance.RespawnAll();
        }
        else if (other.gameObject.tag == "Moveable")
        {
            IRespawnable respawnable = other.gameObject.GetComponent<IRespawnable>();
            if(respawnable != null) 
                RespawnController.Instance.RespawnSelf(respawnable);
        }
    }
}
