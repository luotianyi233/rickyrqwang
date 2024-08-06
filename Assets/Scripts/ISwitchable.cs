using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//����״̬ö��
public enum SwitchState
{
    OPEN,
    CLOSED,
}

public interface ISwitchable
{
    public SwitchState State { get; }
    void Switch();
    void Open();
    void Close();
}
