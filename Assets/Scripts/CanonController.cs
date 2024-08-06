using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CanonController : MonoBehaviour
{
    public GameObject bulletPrefab;
    private GameObject bullet;
    private Transform bossTransform;
    private Quaternion bossRot;
    private Vector3 bossPos;
    private int rotSpeed = 50;
    public Transform launchPos;

    public PlayerController[] players;
    private PlayerController nearestTarget;
    
    public float sightRadius;

    public float coolDownTime;
    private float lastAtkTime;
    private bool isTargetLocked;

    enum BossState
    {
        REST,
        ACTIVE,
    };
    BossState bossState;

    private void Awake()
    {
        bossTransform = this.transform;

        //players = GameObject.FindObjectsOfType<PlayerController>();       //编辑器直接拖了，players为private这边才要初始化

        lastAtkTime = coolDownTime;

        bossPos = bossTransform.position;
        bossRot = bossTransform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        SwitchStates();
        if(FindPlayer())
            lastAtkTime -= Time.deltaTime;
    }

    PlayerController GetNearestPlayer()
    {
        float minDistance = float.MaxValue; // 初始化为最大可能值

        foreach (PlayerController player in players)
        {
            float distance = Vector3.Distance(bossPos, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTarget = player;
            }
        }

        return nearestTarget;
    }

    void SwitchStates()
    {
        if (!FindPlayer())  //如果没有找到玩家或者TODO:炮塔已被关闭
        {
            bossState = BossState.REST;
            isTargetLocked = false;
        }
            
        else
        {
            bossState = BossState.ACTIVE;
            isTargetLocked = true;
        }

        switch(bossState)
        {
            case BossState.REST:
                //TODO:shutdown逻辑
                break;
            case BossState.ACTIVE:
                RotationToPlayer();
                if(lastAtkTime<0)
                {
                    lastAtkTime = coolDownTime;
                    Shoot();
                    isTargetLocked = false;
                }
                break;
        }
    }

    bool FindPlayer()
    {
        if(isTargetLocked) 
            return nearestTarget != null;

        var colliders = Physics.OverlapSphere(bossPos, sightRadius);
        foreach(var target in colliders)
        {
            if(target.CompareTag("Player"))
            {
                nearestTarget = GetNearestPlayer();
                return true;
            }
        }
        nearestTarget = null;
        return false;
    }

    void RotationToPlayer()
    {
        if (!isTargetLocked || nearestTarget == null ) return; 
        
        //计算从炮塔到目标的方向向量
        Vector3 targetDirection = nearestTarget.transform.position - bossTransform.position;    
        //根据方向向量计算旋转角
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);   
        float maxRotationSpeed = rotSpeed * Time.deltaTime;

        bossTransform.rotation = Quaternion.RotateTowards(bossTransform.rotation,targetRotation,maxRotationSpeed);
    }

    void Shoot()    //发射逻辑
    {      
        
        bullet=Instantiate(bulletPrefab,launchPos.position,Quaternion.identity);
        bullet.transform.position = launchPos.position;
        bullet.transform.rotation = launchPos.rotation;
        bullet.GetComponent<BulletController>().target = nearestTarget;
        bullet.GetComponent<BulletController>().MoveToTarget();
    }
}
