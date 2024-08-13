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

    //�����ת����
    private Quaternion freeRot; //������ת

    //��ҿ�ץȡ����������
    private float minMoveableObjectHeight = 0.5f;  //��ץȡ��Ʒ����С�߶�
    private float maxDistance = 1.0f;   //��Ҿ���ץȡ��Ʒ��������
    private LayerMask moveableLayer;    //���ƶ�����Ĳ�
    private Vector3 boxHitNormal;   //���巨��
    private FixedJoint joint;

    //��������
    public Rigidbody rightHandAttachPoint;  //ץȡ�������Ӵ�-����
    public Rigidbody leftHandAttachPoint;  //ץȡ�������Ӵ�-����
    private Transform playerTransform;  //��ҵ�transform����
    private Vector3 playerMovementWorldSpace;   //�������������ϵ�µ��ƶ���
    public int playerNO;
    private float turnSpeed;  //����ת���ٶ�
    private Vector3 tarDir;  //���ﳯ��
    private bool isRun;     //�Ƿ���
    private bool isJumping;     //�Ƿ�����Ծ
    private bool isGround;    //�Ƿ��ڵ���
    private bool isFall; //�Ƿ���أ��ѷ�����
    private bool isRightHoldButtonPressed;   //�Ƿ�������ץȡ����
    private bool isLeftHoldButtonPressed;   //�Ƿ�������ץȡ����
    private bool isRightHold; //�Ƿ�����������ץ������
    private bool isLeftHold; //�Ƿ�����������ץ������
    private float moveSpeed;    //�ƶ��ٶ�
    private float jumpForce;    //��Ծ��
    private bool isHolding;   //�����ճ�
    private float jumpCD;   //��Ծ��ȴ���ѷ�����
    public PhysicMaterial noFriction;
    public PhysicMaterial defaultFriction;

    private float horizontal;   //ˮƽ����
    private float vertical;     //��ֱ����
    private float groundCheckOffset = 0.5f;  //������ƫ����
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
        ropeLayerMask = LayerMask.GetMask("Rope");  // �������Ӳ����ײ���
        ropeLayerMask = ~ropeLayerMask;

        //�������Գ�ʼ��
        playerCollider= GetComponent<CapsuleCollider>();
        playerTransform = transform;
        turnSpeed = 10f;
        tarDir = Vector3.zero;
        isRun = false;
        moveSpeed = 0f;
        jumpForce = 700f;
        isHolding = false;
        jumpCD = 1f;

        //��ȡ����
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

        /*�ѷ�����ض������������ض����������ý�ɫ�����ƶ�����Ȼ�ͻ�����ض���ִ��ʱ���У������Ƕ��ݵĲ����ƶ������������Ծ���ٸ�
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

    /*private void CaculateCameraToPlayer()   //������������ҵ�������
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

        //�������/�ҷ��������ߣ�����Ƿ��п�ץȡ������
        if (Physics.Raycast(playerTransform.position + Vector3.up * minMoveableObjectHeight , handDirection , out RaycastHit hit , maxDistance ,moveableLayer))   
        {
            Debug.Log("��⵽��ץȡ����");
            boxHitNormal = hit.normal;

            GameObject gameObject = hit.collider.gameObject;
            return gameObject;

            /*������������巨����������/�Ҳ೯��нǴ���45��
            if (Vector3.Angle(-boxHitNormal, handDirection) > 45f)   
            {
                return null;
            }
            //���򷵻��������
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
        horizontal = Input.GetAxis("Player" + playerNO + "Horizontal");    //��ȡˮƽ����(AD)
        vertical = Input.GetAxis("Player" + playerNO + "Vertical");        //��ȡ��ֱ����(WS)

        moveSpeed = (isRun == (isRightHold||isLeftHold)) ? 2.5f : (isRun ? 5 : 1.5f); //�ճ�״̬����1��2.5����·״̬2.5���ܲ�״̬5

        tarDir = new Vector3(horizontal, 0, vertical);

        //����Ŀ�귽�򣬽�֮������������ӽ�
        UpdatePlayerDirection();

        if ((horizontal != 0 || vertical != 0) && tarDir.magnitude > 0.1f )  //������wsad�����뷽��Ϊ��
        {
            if (isGround) 
                animator.SetBool("isWalking", true);
            else
                animator.SetBool("isWalking", false);

            Vector3 lkDir = tarDir.normalized;
            freeRot = Quaternion.LookRotation(lkDir, transform.up); //������ת�Ƕ�
            var diferenceRot = freeRot.eulerAngles.y - transform.eulerAngles.y; //������ת�ǶȲ�ֵ
            var eularY = transform.eulerAngles.y;           //��ȡ��ǰ��ת��

            if (diferenceRot < 0 || diferenceRot > 0)
                eularY = freeRot.eulerAngles.y;

            var eular = new Vector3(0, eularY, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(eular), turnSpeed * Time.deltaTime);       //������ת

            //�����ƶ�����
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

    /*IEnumerator Jump()//��Ծcd�ϳ�
    {
        readyToJump = false;
        yield return new WaitForSeconds(jumpCD);
        readyToJump = true;
    }*/

    private void UpdatePlayerDirection()
    {
        if (playerNO == 1)
        {
            //������������ת�Ƕ�
            var forward = Camera1.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            var right = Camera1.transform.TransformDirection(Vector3.right);
            
            //�������﷽��
            tarDir = horizontal * right + vertical * forward;
        }
        else
        {
            //������������ת�Ƕ�
            var forward = Camera2.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            var right = Camera2.transform.TransformDirection(Vector3.right);

            //�������﷽��
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
