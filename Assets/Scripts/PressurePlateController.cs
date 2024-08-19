using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlateController : Switchable
{
    [SerializeField]
    private Animator animator;

    public SwitchesController switchableObject;

    private int count;//����̤���GameObject����

    public UnityEvent onPlatePressed;
    public UnityEvent onPlateReleased;

    private enum DoorClass
    {
        ONETIME,    //һ���Կ���
        REPEAT, //���ÿ���
    }

    [SerializeField]
    private DoorClass doorClass;

    private void Awake()
    {
        RespawnController.Instance.RegisterRespawnable(this);
    }

    private void OnDestroy()
    {
        RespawnController.Instance.UnregisterRespawnable(this);
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.collider.tag=="Player"||other.collider.tag=="Moveable")
        {
            if(doorClass==DoorClass.REPEAT) 
                count++;
            Debug.Log("���룡Ŀǰ����"+count);
            if(count==1||doorClass == DoorClass.ONETIME)
                Open();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if ((other.collider.tag=="Player" || other.collider.tag == "Moveable") && doorClass == DoorClass.REPEAT)
        {
            count--;
            Debug.Log("�˳���Ŀǰ����" + count);
            if (count==0)
                Close();
        }
    }

    public override void Open() //̤������
    {
        if (state == SwitchState.OPEN)
            return;
        state = SwitchState.OPEN;
        if(animator!=null)
            animator.SetBool("Down", true);
        if(doorClass==DoorClass.ONETIME||switchableObject.unlockCount <= 1)
            switchableObject.Open();
        onPlatePressed?.Invoke();
    }

    public override void Close()    //̤������
    {
        if (state == SwitchState.CLOSED)
            return;
        state = SwitchState.CLOSED;
        if (animator != null)
            animator.SetBool("Down", false);
        switchableObject.Close();
        onPlateReleased?.Invoke();
    }
}
