using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampController:Switchable
{
    private Light spotLight;

    public Light[] lights;

    private bool openLocked;

    private void Start()
    {
        openLocked = false;
        spotLight = gameObject.GetComponent<Light>();
        spotLight.enabled = false;
        RespawnController.Instance.RegisterRespawnable(this);
    }

    private void OnDestroy()
    {
        RespawnController.Instance.UnregisterRespawnable(this);
    }

    private void Update()
    {
        if (openLocked)
            return;

        bool allLightsOn = true;
        foreach (Light light in lights)
        {
            if (!light.enabled)
            {
                allLightsOn = false;
                break;
            }
        }

        if (allLightsOn)
        {
            openLocked = true;
        }
    }

    public override void Open()
    {
        if (state == SwitchState.OPEN)
            return;

        state = SwitchState.OPEN;
        spotLight.enabled = true;
    }

    public override void Close()
    {
        if (state == SwitchState.CLOSED || openLocked)
            return;

        state = SwitchState.CLOSED;
        spotLight.enabled = false;
    }

    public override void Respawn()
    {
        openLocked = false;
        Close();
        state = SwitchState.CLOSED;
    }
}