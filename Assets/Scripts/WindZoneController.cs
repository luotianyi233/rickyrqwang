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
    private bool isClose;   //�糡�رձ�־
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

    //��һ򷽿����糡��ȡ�������������������������������ҳ��򸽼�xz���ٶ�
    private void OnTriggerStay(Collider other)
    {
        if ((other.tag == "Player" || other.tag == "Moveable" ) &&!isClose)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {

                //��ʱȡ���������������ӷ糡����
                rb.useGravity =  false ;
                if (other.tag == "Player")
                {
                    rb.gameObject.GetComponent<Animator>().ResetTrigger("leaveWind");
                    rb.gameObject.GetComponent<Animator>().SetTrigger("enterWind");
                }

                float heightDif = floatHeight - other.transform.position.y + gameObject.transform.position.y;
                if (heightDif > 0)
                {   
                    rb.AddForce(Vector3.up * floatPower * heightDif, ForceMode.Acceleration);   //�糡�ĳ�����������
                    tarDir = (rb.gameObject.transform.forward).normalized;
                    if(other.tag=="Player")
                        rb.velocity = new Vector3(tarSpeed*tarDir.x, rb.velocity.y, tarSpeed * tarDir.z);
                    //rb.velocity = new Vector3(tarSpeed.x, rb.velocity.y, tarSpeed.z);   //����糡���޷��ı䷽����˼̳���Ҹս���糡ʱ���ٶ�
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
    public override void Open() //�糡��
    {
        Debug.Log("�����糡");
        if (state == SwitchState.OPEN)
            return;
        state = SwitchState.OPEN;
        if(wind == null)
            wind = Instantiate(windZonePrefab, windZonePoint.position, windZonePoint.rotation);
        isClose = false;
    }

    public override void Close()    //�糡�ر�
    {
        Debug.Log("�رշ糡");
        if (state == SwitchState.CLOSED)
            return;
        state = SwitchState.CLOSED;
        if(wind != null)
            Destroy(wind);
        isClose = true;
    }
}
