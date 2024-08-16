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
        // 设置gameObject的位置和旋转为下一个重生点的位置和旋转
        gameObject.transform.position = respawnPoints[currentIndex].position;
    }
    
    public void Progress()
    {
        //显示存档已更新


        if (respawnPoints.Length == 0)
            return;

        // 更新到下一个重生点
        currentIndex = (currentIndex + 1) % respawnPoints.Length;
    }
}
