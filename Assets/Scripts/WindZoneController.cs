using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindZoneController : Switchable
{
    public float floatPower = 15f;
    public float floatHeight = 8f;
    public float tarSpeed = 2f;
    public Vector3 tarDir;
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
        if((other.tag == "Player"|| other.tag == "Moveable") &&!isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                tarDir = (rb.gameObject.transform.forward).normalized;
                if (other.tag == "Player")
                    rb.velocity = new Vector3(tarSpeed * tarDir.x, 0f, tarSpeed * tarDir.z);
            }
        }
    }

    //玩家或方块进入风场则取消重力，给予持续上升推力，并对玩家朝向附加xz向速度
    private void OnTriggerStay(Collider other)
    {
        if ((other.tag == "Player" || other.tag == "Moveable" ) &&!isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {

                //暂时取消人物重力，增加风场表现
                rb.useGravity =  false ;
                if (other.tag == "Player")
                {
                    rb.gameObject.GetComponent<Animator>().ResetTrigger("leaveWind");
                    rb.gameObject.GetComponent<Animator>().SetTrigger("enterWind");
                }

                float heightDif = floatHeight - other.transform.position.y + gameObject.transform.position.y;
                if (heightDif > 0)
                {   
                    rb.AddForce(Vector3.up * floatPower * heightDif, ForceMode.Acceleration);   //风场的持续上升推力
                    tarDir = (rb.gameObject.transform.forward).normalized;
                    if(other.tag=="Player")
                        rb.velocity = new Vector3(tarSpeed*tarDir.x, rb.velocity.y, tarSpeed * tarDir.z);
                    //rb.velocity = new Vector3(tarSpeed.x, rb.velocity.y, tarSpeed.z);   //进入风场后无法改变方向，因此继承玩家刚进入风场时的速度
                }
            }
        }
        else if((other.tag == "Player" || other.tag == "Moveable" ) && isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.useGravity = true;
            if (other.tag == "Player")
            {
                rb.gameObject.GetComponent<Animator>().ResetTrigger("enterWind");
                rb.gameObject.GetComponent<Animator>().SetTrigger("leaveWind");
            }
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if ((other.tag == "Player"||other.tag == "Moveable" ) &&!isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                if (other.tag == "Player")
                {
                    rb.gameObject.GetComponent<Animator>().ResetTrigger("enterWind");
                    rb.gameObject.GetComponent<Animator>().SetTrigger("leaveWind");
                }
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
