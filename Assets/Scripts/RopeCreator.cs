using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class RopeSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject ropePartPrefab, parent;

    [SerializeField]
    [Range(1, 1000)]
    int length = 1;

    [SerializeField]
    float interval = 0.21f;

    [SerializeField]
    bool lockFirstPart, lockLastPart;

    [SerializeField]
    bool spawn, reset;

    void Update()
    {
        if(reset)
        {
            foreach(GameObject tmp in GameObject.FindGameObjectsWithTag("Rope"))
            {
                Destroy(tmp);
            }

            reset = false;
        
        }

        if(spawn)
        {
            SpawnRope();
            spawn = false;

        }
    }

    public void SpawnRope()
    {
        int count = (int)(length / interval);

        for(int x=0;x<count;x++)
        {
            GameObject tmp = Instantiate(ropePartPrefab, new Vector3(transform.position.x, transform.position.y + interval*(x+1),transform.position.z), Quaternion.identity,parent.transform);
            tmp.transform.eulerAngles = new Vector3(180,0,0);
        
            tmp.name=parent.transform.childCount.ToString();

            if(x==0)
            {
                Destroy(tmp.GetComponent<CharacterJoint>());
                if(lockFirstPart)
                {
                    tmp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                }
            }
            else
            {
                tmp.GetComponent<CharacterJoint>().connectedBody = parent.transform.Find((parent.transform.childCount - 1).ToString()).GetComponent<Rigidbody>();
            }
        }

        if(lockLastPart)
        {
            parent.transform.Find((parent.transform.childCount).ToString()).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }

    }
}
