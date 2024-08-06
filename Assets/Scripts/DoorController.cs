using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : Switchable
{
    private JointMotor jointMotor;

    private void Start()
    {
        jointMotor.force = 100000;
    }
    public override void Open()
    {
        if (state == SwitchState.OPEN)
            return;
        state = SwitchState.OPEN;
        jointMotor.targetVelocity = -50;
        gameObject.GetComponent<HingeJoint>().motor = jointMotor;
    }

    public override void Close()
    {
        if (state == SwitchState.CLOSED)
            return;
        state = SwitchState.CLOSED;
        jointMotor.targetVelocity = 50;
        gameObject.GetComponent<HingeJoint>().motor = jointMotor;
    }
}
