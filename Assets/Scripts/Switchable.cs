using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Switchable : MonoBehaviour,ISwitchable,IRespawnable
{
    [SerializeField] protected SwitchState state = SwitchState.CLOSED;

    public SwitchState State { get { return state; } }

    public void Switch()
    {
        switch (state)
        {
            case SwitchState.CLOSED:
                Open();
                break;
            case SwitchState.OPEN:
                Close();
                break;
        }
    }

    public virtual void Respawn()
    {
        if (state == SwitchState.CLOSED) return;
        Close();
        state = SwitchState.CLOSED;
    }
    public abstract void Close();
    public abstract void Open();
}
