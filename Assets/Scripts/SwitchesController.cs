using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class SwitchesController : Switchable    //控制多个Switchable实例的开关
{
    [SerializeField] private Switchable[] switchesArray;

    public int unlockCount;
    private int unlockSwitch;

    private bool openLocked;

    private void Awake()
    {
        openLocked = false;
        unlockSwitch = 0;
        RespawnController.Instance.RegisterRespawnable(this);
    }

    public override void Open()
    {
        if(state==SwitchState.OPEN)
            return;

        state = SwitchState.OPEN;

        for (int i = 0; i < switchesArray.Length; i++)
        {
            switchesArray[i].Open();
        }

        if(unlockCount>1)   //场景中多个复用机关控制的机关在开启后会锁定开启状态/单个复用机关不锁定
            openLocked = true;
    }

    public override void Respawn()
    {
        openLocked = false;
        unlockSwitch = 0;
        Close();
        state = SwitchState.CLOSED;
    }

    public override void Close()
    {
        if (state == SwitchState.CLOSED || openLocked) 
            return;

        state = SwitchState.CLOSED;

        for (int i = 0; i < switchesArray.Length; i++)
        {
            switchesArray[i].Close();
        }
    }

    public void Activate()
    {
        if (unlockSwitch < unlockCount)
            unlockSwitch++;

        if(unlockSwitch == unlockCount)
            Open();
    }

    public void DeActivate()
    {
        if (unlockSwitch > 0 && unlockSwitch < unlockCount)
        {
            unlockSwitch--;
            Close();
        }
    }

}
