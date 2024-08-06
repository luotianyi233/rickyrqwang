using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{    
    public Camera Camera1;
    public Camera Camera2;
    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider playerCollider;

    //������Ծ������(�ѷ�����
    public KeyCode runJoystick = KeyCode.JoystickButton2;
    public KeyCode runKeyboard = KeyCode.LeftShift;
    public KeyCode jumpJoystick = KeyCode.JoystickButton3;
    public KeyCode jumpKeyboard = KeyCode.Space;

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
    private bool isFall; //�Ƿ����
    private bool isRightHoldButtonPressed;   //�Ƿ�������ץȡ����
    private bool isLeftHoldButtonPressed;   //�Ƿ�������ץȡ����
    private bool isRightHold; //�Ƿ�����������ץ������
    private bool isLeftHold; //�Ƿ�����������ץ������
    private float moveSpeed;    //�ƶ��ٶ�
    private float jumpForce;    //��Ծ��
    private bool readyToJump;   //��ȴ��ɱ�־
    private bool hasJumped; //��Ծ��ʼ��־
    private float jumpCD;   //��Ծ��ȴ
    public PhysicMaterial noFriction;
    public PhysicMaterial defaultFriction;

    private float horizontal;   //ˮƽ����
    private float vertical;     //��ֱ����
    private float groundCheckOffset = 0.05f;  //������ƫ����
    private int ropeLayerMask;

    void Start()
    {
        rb= GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        moveableLayer = LayerMask.GetMask("Moveable"); 
        ropeLayerMask = LayerMask.GetMask("Rope");  // �������Ӳ����ײ���
        ropeLayerMask =~ropeLayerMask;

        //�������Գ�ʼ��
        playerCollider= GetComponent<CapsuleCollider>();
        playerTransform = transform;
        turnSpeed = 10f;
        tarDir = Vector3.zero;
        isRun = false;
        moveSpeed = 0f;
        jumpForce = 600f;
        readyToJump = true;
        hasJumped = false;
        jumpCD = 1f;
    }

    void FixedUpdate()
    {
        PlayerMoving();
        PlayerJumping();
    }
    private void Update()
    {
        ActionDetection();
        GroundDetection();
        //CaculateCameraToPlayer();
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
            hasJumped = false;
            playerCollider.material=defaultFriction;
            Debug.DrawLine(castOrigin,castOrigin+Vector3.down*hit.distance,Color.red);
        }
        else
        {
            isGround = false;
            isFall = true;
            playerCollider.material = noFriction;
            Debug.DrawLine(castOrigin,castOrigin+Vector3.down*castDistance,Color.green);

        }
    }

    private void ActionDetection()
    {
        isRun = ((Input.GetButton("Player" + playerNO + "Run") || Input.GetButton("Player" + playerNO + "Run")) && (horizontal != 0 || vertical != 0) && isGround);
        animator.SetBool("isRunning", isRun);

        isJumping = ((Input.GetButton("Player" + playerNO + "Jump") || Input.GetButton("Player" + playerNO + "Jump")) && isGround && readyToJump && (!isRightHold || !isLeftHold));
        animator.SetBool("isJumping", isJumping);

        isRightHoldButtonPressed = ((Input.GetButton("Player" + playerNO + "RightHold") || Input.GetButton("Player" + playerNO + "RightHold")) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump")) && !isLeftHoldButtonPressed;
        isLeftHoldButtonPressed = ((Input.GetButton("Player" + playerNO + "LeftHold") || Input.GetButton("Player" + playerNO + "LeftHold")) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump")) && !isRightHoldButtonPressed;

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
            }
        }

        //�������ض����������ý�ɫ�����ƶ�����Ȼ�ͻ�����ض���ִ��ʱ���У������Ƕ��ݵĲ����ƶ������������Ծ���ٸ�
        //���Ŀǰ��animator����ʱ�����ɵ�ifFall��δ�����ܻ�����������
        isFall = (isGround && rb.velocity.y < 0);
        animator.SetBool("isFall", isFall);
    }

    private void HoldMoveableObject(bool isHoldButtonPressed,bool isLeft)
    {
        if (isHoldButtonPressed)
        {
            GameObject moveableObject = isLeft? MovebaleObject(playerTransform, -playerTransform.right) :  MovebaleObject(playerTransform, playerTransform.right);
            if (moveableObject != null)
            {
                moveableObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

                if (joint == null)
                {
                    joint = moveableObject.AddComponent<FixedJoint>();
                    moveableObject.transform.position = isLeft ?
                        new Vector3(leftHandAttachPoint.transform.position.x, moveableObject.transform.position.y, leftHandAttachPoint.transform.position.z) :
                        new Vector3(rightHandAttachPoint.transform.position.x, moveableObject.transform.position.y, rightHandAttachPoint.transform.position.z);
                        
                    joint.connectedBody = isLeft? leftHandAttachPoint:rightHandAttachPoint;
                }
                moveableObject.GetComponent<Collider>().isTrigger = true;

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

            //������巨����������/�Ҳ೯��нǴ���60��
            if (Vector3.Angle(-boxHitNormal, handDirection) > 60f)   
            {
                return null;
            }
            //���򷵻��������
            else
            {
                GameObject gameObject=hit.collider.gameObject;
                return gameObject;
            }

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
            if(isGround && !animator.GetCurrentAnimatorStateInfo(0).IsName("Fall")&&!hasJumped)
                rb.velocity = new Vector3((moveSpeed * tarDir).x,rb.velocity.y, (moveSpeed * tarDir).z);
        }
        else 
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void PlayerJumping()
    {
        if (isJumping && readyToJump)   
        {
            hasJumped = true;
            StartCoroutine(Jump());
            rb.velocity = new Vector3( 3f * rb.velocity.x, rb.velocity.y, 3f * rb.velocity.z); // FIXME:xz�������ʱ����Ч
            rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
            playerCollider.material = noFriction;
        }
    }

    IEnumerator Jump()
    {
        readyToJump = false;
        yield return new WaitForSeconds(jumpCD);
        readyToJump = true;
    }

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
}
