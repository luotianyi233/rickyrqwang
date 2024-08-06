using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindZoneController : Switchable
{
    public float floatPower = 10f;
    public float floatHeight = 5f;
    public Vector3 tarSpeed;
    private bool isClose;   //风场关闭标志
    private Collider other;
    public GameObject windZonePrefab;
    public Transform windZonePoint;
    private GameObject wind;

    private void Start()
    {
        isClose = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.tag == "Player" || other.tag == "Moveable")&&!isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                tarSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if ((other.tag == "Player" || other.tag == "Moveable")&&!isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                tarSpeed = new Vector3(rb.velocity.x, tarSpeed.y, rb.velocity.z);

                //暂时取消人物重力，增加风场表现
                rb.useGravity =  false ;  

                float heightDif = floatHeight - other.transform.position.y;
                if (heightDif > 0)
                {   
                    rb.AddForce(Vector3.up * floatPower * heightDif, ForceMode.Acceleration);   //风场的持续上升推力
                    rb.velocity = new Vector3(tarSpeed.x, rb.velocity.y, tarSpeed.z);   //进入风场后无法改变方向，因此继承玩家刚进入风场时的速度
                }
            }
        }
        else if((other.tag == "Player" || other.tag == "Moveable") && isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.useGravity = true;
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if ((other.tag == "Player"||other.tag == "Moveable")&&!isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
            }
        }
    }
    public override void Open() //风场打开
    {
        Debug.Log("开启风场");
        if (state == SwitchState.OPEN)
            return;
        state = SwitchState.OPEN;
        if(wind == null)
            wind = Instantiate(windZonePrefab, windZonePoint.position, windZonePoint.rotation);
        isClose = false;
    }

    public override void Close()    //风场关闭
    {
        Debug.Log("关闭风场");
        if (state == SwitchState.CLOSED)
            return;
        state = SwitchState.CLOSED;
        if(wind != null)
            Destroy(wind);
        isClose = true;
    }
}
