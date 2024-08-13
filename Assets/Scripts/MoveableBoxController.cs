using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableBoxController : MonoBehaviour,IRespawnable
{
    private Vector3 respawnPos;
    private Quaternion respawnRot;
    private Rigidbody rb;

    void Start()
    {
        respawnPos = transform.position;
        respawnRot = transform.rotation;

        rb = GetComponent<Rigidbody>();
        RespawnController.Instance.RegisterRespawnable(this);
    }

    public void Respawn()
    {
        rb.velocity = Vector3.zero;
        gameObject.transform.position = respawnPos;
        gameObject.transform.rotation = respawnRot;
    }

    void OnDestroy()
    {
        RespawnController.Instance.UnregisterRespawnable(this);
    }
}
