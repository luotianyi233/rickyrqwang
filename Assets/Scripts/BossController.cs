using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public CanonController[] canons;

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (CanonController canon in canons)
            {
                canon.Close();
            }
        }
    }
}
