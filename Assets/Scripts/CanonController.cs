using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CanonController : Switchable
{
    public GameObject canon;
    public GameObject bulletPrefab;
    private GameObject bullet;
    public GameObject brokenEffect;
    private GameObject brokenVFX;
    private Transform bossTransform;
    public GameObject visualization;
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

    public enum BossState
    {
        REST,
        ACTIVE,
    };
    public BossState bossState;

    private void Awake()
    {
        bossTransform = canon.transform;

        //players = GameObject.FindObjectsOfType<PlayerController>();       //编辑器直接拖了，players为private这边才要初始化

        lastAtkTime = coolDownTime;

        bossPos = bossTransform.position;
        bossRot = bossTransform.rotation;
    }

    void Update()
    {
        if (brokenVFX != null)
        {
            brokenVFX.transform.position = launchPos.position;
            brokenVFX.transform.rotation = launchPos.rotation;
        }
        SwitchStates();
        if(FindPlayer()&&bossState == BossState.ACTIVE)
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
        if (!FindPlayer() || state==SwitchState.CLOSED)  //如果没有找到玩家或者炮塔已被关闭
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
                visualization.SetActive(false);
                Broken();
                break;
            case BossState.ACTIVE:
                visualization.SetActive(true);
                if (brokenVFX!=null)
                    Destroy(brokenVFX);
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
        //targetDirection.y = 0;  //保持炮塔不会上下旋转
        if (targetDirection == Vector3.zero) return;  //如果方向向量为0，不旋转
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

    public override void Open()
    {
        if (state == SwitchState.OPEN)
            return;
        state = SwitchState.OPEN;

    }

    public override void Close()
    {
        if (state == SwitchState.CLOSED)
            return;
        state = SwitchState.CLOSED;

    }

    public override void Respawn()
    {
        if(brokenVFX!=null)
            Destroy(brokenVFX);
        if (state == SwitchState.CLOSED) return;
        Close();
        state = SwitchState.CLOSED;
    }

    void Broken()
    {
        //炮管冒烟特效
        if(brokenVFX==null)
            brokenVFX=Instantiate(brokenEffect, launchPos.position, Quaternion.identity);
    }
}
