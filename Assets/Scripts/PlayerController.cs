using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour,IRespawnable
{    
    public Camera Camera1;
    public Camera Camera2;
    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider playerCollider;

    private GameObject[] ropes;
    private GameObject ropeParent;

    //相机旋转设置
    private Quaternion freeRot; //自由旋转

    //玩家可抓取的物体属性
    private float minMoveableObjectHeight = 0.5f;  //可抓取物品的最小高度
    private float maxDistance = 1.0f;   //玩家距离抓取物品的最大距离
    private LayerMask moveableLayer;    //可移动物体的层
    private Vector3 boxHitNormal;   //物体法线
    private FixedJoint joint;

    //人物属性
    public Rigidbody rightHandAttachPoint;  //抓取物体连接处-右手
    public Rigidbody leftHandAttachPoint;  //抓取物体连接处-左手
    private Transform playerTransform;  //玩家的transform属性
    private Vector3 playerMovementWorldSpace;   //玩家在世界坐标系下的移动量
    public int playerNO;
    private float turnSpeed;  //人物转身速度
    private Vector3 tarDir;  //人物朝向
    private bool isRun;     //是否奔跑
    private bool isJumping;     //是否在跳跃
    private bool isGround;    //是否在地上
    private bool isFall; //是否落地（已废弃）
    private bool isRightHoldButtonPressed;   //是否按下右手抓取按键
    private bool isLeftHoldButtonPressed;   //是否按下左手抓取按键
    private bool isRightHold; //是否正在用右手抓持物体
    private bool isLeftHold; //是否正在用左手抓持物体
    private float moveSpeed;    //移动速度
    private float jumpForce;    //跳跃力
    private bool isHolding;   //正在握持
    private float jumpCD;   //跳跃冷却（已废弃）
    public PhysicMaterial noFriction;
    public PhysicMaterial defaultFriction;

    private float horizontal;   //水平方向
    private float vertical;     //垂直方向
    private float groundCheckOffset = 0.5f;  //地面检测偏移量
    private int ropeLayerMask;
    private int playerLayerMask;

    private Vector3 respawnPosition;
    private Quaternion respawnRotation;
    private List<Vector3> respawnRopePos = new List<Vector3>();
    private List<Quaternion> respawnRopeRot = new List<Quaternion>();

    void Start()
    {
        rb= GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        moveableLayer = LayerMask.GetMask("Moveable"); 
        ropeLayerMask = LayerMask.GetMask("Rope");  // 忽略绳子层的碰撞检测
        ropeLayerMask = ~ropeLayerMask;

        //人物属性初始化
        playerCollider= GetComponent<CapsuleCollider>();
        playerTransform = transform;
        turnSpeed = 10f;
        tarDir = Vector3.zero;
        isRun = false;
        moveSpeed = 0f;
        jumpForce = 700f;
        isHolding = false;
        jumpCD = 1f;

        //获取绳子
        ropes = GameObject.FindGameObjectsWithTag("Rope");
        ropeParent = ropes[0].transform.parent.gameObject;

        respawnPosition = transform.position;
        respawnRotation = transform.rotation;
        foreach (GameObject rope in ropes)
        {
            respawnRopePos.Add(rope.transform.position);
            respawnRopeRot.Add(rope.transform.rotation);
        }

        RespawnController.Instance.RegisterRespawnable(this);
    }

    void FixedUpdate()
    {
        PlayerMoving();
        PlayerJumping();
        GroundDetection();
        ActionDetection();
    }

    private void GroundDetection()
    {       
        float radius=playerCollider.radius;
        float halfHeight = playerCollider.height/2.0f;

        Vector3 castOrigin = playerTransform.position+Vector3.up*(halfHeight-radius+groundCheckOffset);
        float castDistance = groundCheckOffset + 2 * radius;
        if(Physics.SphereCast(castOrigin, radius, Vector3.down, out RaycastHit hit, castDistance,ropeLayerMask))
        {
            isFall = false;
            isGround = true;
            animator.SetBool("isGround", true);
            playerCollider.material=defaultFriction;

            foreach (GameObject rope in ropes)
            {
                rope.GetComponent<CapsuleCollider>().material = defaultFriction;
            }

            Debug.DrawLine(castOrigin,castOrigin+Vector3.down*hit.distance,Color.red);
        }
        else
        {
            isGround = false;
            animator.SetBool("isGround", false);
            isFall = true;
            playerCollider.material = noFriction;

            foreach (GameObject rope in ropes)
            {
                rope.GetComponent<CapsuleCollider>().material = noFriction;
            }

            Debug.DrawLine(castOrigin,castOrigin+Vector3.down*castDistance,Color.green);

        }
    }

    private void ActionDetection()
    {
        isRun = ((Input.GetButton("Player" + playerNO + "Run") || Input.GetButton("Player" + playerNO + "Run")) && (horizontal != 0 || vertical != 0) && isGround);
        animator.SetBool("isRunning", isRun);

        isJumping = ((Input.GetButton("Player" + playerNO + "Jump") || Input.GetButton("Player" + playerNO + "Jump"))) && isGround  && !isRightHold && !isLeftHold && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump")&& !animator.GetCurrentAnimatorStateInfo(0).IsName("Float");
        animator.SetBool("isJumping", isJumping);

        isRightHoldButtonPressed = ((Input.GetButton("Player" + playerNO + "RightHold") || Input.GetButton("Player" + playerNO + "RightHold"))) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump") && !isLeftHoldButtonPressed;
        isLeftHoldButtonPressed = ((Input.GetButton("Player" + playerNO + "LeftHold") || Input.GetButton("Player" + playerNO + "LeftHold"))) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump") && !isRightHoldButtonPressed;

        if (isRightHoldButtonPressed || isLeftHoldButtonPressed)
        {
            HoldMoveableObject(isRightHoldButtonPressed, false);
            HoldMoveableObject(isLeftHoldButtonPressed, true);
        }
        else
        {
            isLeftHoldButtonPressed = false;
            isRightHoldButtonPressed = false;
            isLeftHold = isLeftHoldButtonPressed;
            animator.SetBool("isLeftHold", isLeftHold);
            isRightHold = isRightHoldButtonPressed;
            animator.SetBool("isRightHold", isRightHold);
            if (joint != null)
            {
                GameObject moveableObject = joint.gameObject;
                Destroy(joint);
                moveableObject.GetComponent<Collider>().isTrigger = false;
                moveableObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                isHolding = false;
            }
        }

        /*已废弃落地动作：如果有落地动作，必须让角色不能移动（不然就会在落地动作执行时滑行），但是短暂的不可移动会带给连续跳跃卡顿感
        isFall = (isGround && rb.velocity.y < 0);
        animator.SetBool("isFall", isFall);
        */
    }

    private void HoldMoveableObject(bool isHoldButtonPressed,bool isLeft)
    {
        if (isHoldButtonPressed && !isHolding)
        {
            GameObject moveableObject = isLeft ? MovebaleObject(playerTransform, -playerTransform.right) : MovebaleObject(playerTransform, playerTransform.right);
            if (moveableObject != null)
            {
                //moveableObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                moveableObject.GetComponent<Collider>().isTrigger = true;
                if (joint == null)
                {
                    moveableObject.transform.position = isLeft ?
                        new Vector3(leftHandAttachPoint.transform.position.x, moveableObject.transform.position.y, leftHandAttachPoint.transform.position.z) :
                        new Vector3(rightHandAttachPoint.transform.position.x, moveableObject.transform.position.y, rightHandAttachPoint.transform.position.z);

                    moveableObject.transform.rotation = isLeft ?
                        leftHandAttachPoint.rotation :
                        rightHandAttachPoint.rotation;

                    joint = moveableObject.AddComponent<FixedJoint>();

                    joint.connectedBody = isLeft ? leftHandAttachPoint : rightHandAttachPoint;

                    joint.connectedMassScale = 0.0045f;

                    joint.autoConfigureConnectedAnchor = false ;
                    joint.anchor = new Vector3(0, -0.5f, 0);
                    joint.connectedAnchor = Vector3.zero;

                    isHolding = true;
            }
                

                if (isLeft)
                {
                    isLeftHold = isHoldButtonPressed;
                    animator.SetBool("isLeftHold", isLeftHold);
                }
                else
                {
                    isRightHold = isHoldButtonPressed;
                    animator.SetBool("isRightHold", isRightHold);
                }
            }
        }
    }

    /*private void CaculateCameraToPlayer()   //世界坐标下玩家的输入量
    {
        if (playerNO == 1)
        {
            Vector3 cameraFwdProjection = new Vector3(Camera1.transform.forward.x, 0, Camera1.transform.forward.z).normalized;
            playerMovementWorldSpace = cameraFwdProjection * Input.GetAxis("Player" + playerNO + "Vertical") + Camera1.transform.right*Input.GetAxis("Player" + playerNO + "Horizontal");
        }
        else
        {
            Vector3 cameraFwdProjection = new Vector3(Camera2.transform.forward.x, 0, Camera2.transform.forward.z).normalized;
            playerMovementWorldSpace = cameraFwdProjection * Input.GetAxis("Player" + playerNO + "Vertical") + Camera1.transform.right * Input.GetAxis("Player" + playerNO + "Horizontal");
        }
    }*/

    private GameObject MovebaleObject(Transform playerTransfrom,Vector3 handDirection)
    {

        Vector3 rayStart = playerTransform.position + Vector3.up * minMoveableObjectHeight;
        Vector3 rayEnd = rayStart + playerTransfrom.right * maxDistance;
        Debug.DrawLine(rayStart, rayEnd, Color.red);

        //从玩家左方/右方发射射线，检测是否有可抓取的物体
        if (Physics.Raycast(playerTransform.position + Vector3.up * minMoveableObjectHeight , handDirection , out RaycastHit hit , maxDistance ,moveableLayer))   
        {
            Debug.Log("检测到可抓取物体");
            boxHitNormal = hit.normal;

            GameObject gameObject = hit.collider.gameObject;
            return gameObject;

            /*废弃：如果物体法线与玩家左侧/右侧朝向夹角大于45度
            if (Vector3.Angle(-boxHitNormal, handDirection) > 45f)   
            {
                return null;
            }
            //否则返回这个物体
            else
            {
                GameObject gameObject=hit.collider.gameObject;
                return gameObject;
            }*/

        }
        return null;
    }

    private void PlayerMoving()
    {
        horizontal = Input.GetAxis("Player" + playerNO + "Horizontal");    //获取水平方向(AD)
        vertical = Input.GetAxis("Player" + playerNO + "Vertical");        //获取垂直方向(WS)

        moveSpeed = (isRun == (isRightHold||isLeftHold)) ? 2.5f : (isRun ? 5 : 1.5f); //握持状态正常1跑2.5，走路状态2.5，跑步状态5

        tarDir = new Vector3(horizontal, 0, vertical);

        //更新目标方向，将之关联至摄像机视角
        UpdatePlayerDirection();

        if ((horizontal != 0 || vertical != 0) && tarDir.magnitude > 0.1f )  //当键盘wsad有输入方向不为空
        {
            if (isGround) 
                animator.SetBool("isWalking", true);
            else
                animator.SetBool("isWalking", false);

            Vector3 lkDir = tarDir.normalized;
            freeRot = Quaternion.LookRotation(lkDir, transform.up); //更新旋转角度
            var diferenceRot = freeRot.eulerAngles.y - transform.eulerAngles.y; //计算旋转角度差值
            var eularY = transform.eulerAngles.y;           //获取当前旋转角

            if (diferenceRot < 0 || diferenceRot > 0)
                eularY = freeRot.eulerAngles.y;

            var eular = new Vector3(0, eularY, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(eular), turnSpeed * Time.deltaTime);       //缓动旋转

            //人物移动更新
            if(isGround && !animator.GetCurrentAnimatorStateInfo(0).IsName("Float"))
                rb.velocity = new Vector3((moveSpeed * tarDir).x,rb.velocity.y, (moveSpeed * tarDir).z);
        }
        else 
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void PlayerJumping()
    {
        if (isJumping && isGround)   
        {
            //StartCoroutine(Jump());
            rb.velocity = new Vector3( 1.5f * rb.velocity.x, 0, 1.5f * rb.velocity.z);
            rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
            playerCollider.material = noFriction;
        }
    }

    /*IEnumerator Jump()//跳跃cd废除
    {
        readyToJump = false;
        yield return new WaitForSeconds(jumpCD);
        readyToJump = true;
    }*/

    private void UpdatePlayerDirection()
    {
        if (playerNO == 1)
        {
            //获得摄像机的旋转角度
            var forward = Camera1.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            var right = Camera1.transform.TransformDirection(Vector3.right);
            
            //更新人物方向
            tarDir = horizontal * right + vertical * forward;
        }
        else
        {
            //获得摄像机的旋转角度
            var forward = Camera2.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            var right = Camera2.transform.TransformDirection(Vector3.right);

            //更新人物方向
            tarDir = horizontal * right + vertical * forward;
        }
    }

    public void Respawn()
    {
        rb.velocity = Vector3.zero;
        rb.transform.position = respawnPosition;
        rb.transform.rotation = respawnRotation;

        for(int i =0;i<ropes.Length;i++)
        {
                ropes[i].transform.position = respawnRopePos[i];
                ropes[i].transform.rotation = respawnRopeRot[i];
        }
    }

    private void OnDestroy()
    {
        RespawnController.Instance.UnregisterRespawnable(this);
    }
}
