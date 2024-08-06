using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchesController : Switchable
{
    [SerializeField] private Switchable[] switchesArray;

    public override void Open()
    {
        if(state==SwitchState.OPEN)
            return;

        state = SwitchState.OPEN;

        for (int i = 0; i < switchesArray.Length; i++)
        {
            switchesArray[i].Open();
        }
    }

    public override void Close()
    {
        if (state == SwitchState.CLOSED) 
            return;

        state = SwitchState.CLOSED;

        for (int i = 0; i < switchesArray.Length; i++)
        {
            switchesArray[i].Close();
        }
    }
}
