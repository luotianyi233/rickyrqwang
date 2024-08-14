using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : Switchable
{
    private JointMotor jointMotor;

    [SerializeField]
    private bool isLeftDoor;


    private void Start()
    {
        jointMotor.force = 100000;
        jointMotor.targetVelocity = isLeftDoor ? 100 : -100;
        gameObject.GetComponent<HingeJoint>().motor = jointMotor;
    }

    public override void Open()
    {
        if (state == SwitchState.OPEN)
            return;

        state = SwitchState.OPEN;
        jointMotor.targetVelocity = isLeftDoor ? -100 : 100;
        gameObject.GetComponent<HingeJoint>().motor = jointMotor;
    }

    public override void Close()
    {
        if (state == SwitchState.CLOSED)
            return;

        state = SwitchState.CLOSED;
        jointMotor.targetVelocity = isLeftDoor ? 100 : -100;
        gameObject.GetComponent<HingeJoint>().motor = jointMotor;
    }

    public override void Respawn()
    {
        Close();
        state = SwitchState.CLOSED;
    }
}
