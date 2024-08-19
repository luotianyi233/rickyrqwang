using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMController : MonoBehaviour
{
    public GameObject btn1;
    public GameObject btn2;
    public GameObject btn3;

    public Transform level1;
    public Transform level2;
    public Transform level3;
    public PlayerController P1;
    public PlayerController P2;

    // Update is called once per frame
    public void TransToLevel1()
    {
        P1.Transport(level1);
        P2.Transport(level1);
    }
    public void TransToLevel2()
    {
        P1.Transport(level2);
        P2.Transport(level2);
        if (btn1 != null) Destroy(btn1);
    }

    public void TransToLevel3()
    {
        P1.Transport(level3);
        P2.Transport(level3);
        if(btn1!= null) Destroy(btn1);
        if(btn2!= null) Destroy(btn2);
    }
}
