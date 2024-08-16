using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressController : MonoBehaviour,IRespawnable
{
    public Transform[] respawnPoints;
    private int currentIndex = 0;

    void Start()
    {
        currentIndex = 0;
        gameObject.transform.position = respawnPoints[currentIndex].position;
        RespawnController.Instance.RegisterRespawnable(this);
    }

    void Destroy()
    {
        RespawnController.Instance.UnregisterRespawnable(this);
    }

    public void Respawn()
    {
        // ����gameObject��λ�ú���תΪ��һ���������λ�ú���ת
        gameObject.transform.position = respawnPoints[currentIndex].position;
    }
    
    public void Progress()
    {
        //��ʾ�浵�Ѹ���


        if (respawnPoints.Length == 0)
            return;

        // ���µ���һ��������
        currentIndex = (currentIndex + 1) % respawnPoints.Length;
    }
}
