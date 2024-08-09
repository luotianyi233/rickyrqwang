using UnityEngine;

public class FootIKController : MonoBehaviour
{
    //IK����Ļ�������
    private Animator animator;
    private Vector3 rFootIK, lFootIK;
    private Vector3 rFootPos, lFootPos;
    private Quaternion rFootRot, lFootRot;

    [SerializeField]
    private LayerMask IKLayer;

    [SerializeField]
    [Range (0,0.2f)] 
    private float raycastOffset;    //���߼��ƫ�����������㲿��ֹ��ģ

    [SerializeField]
    private float raycastDist;  //��ؾ���

    [SerializeField]
    private bool IKEnabled;

    [SerializeField]
    private float IKSphereRadius = 0.05f;

    [SerializeField]
    private float posSpehereRadius = 0.05f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rFootIK = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        lFootIK = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
    }

    private void FixedUpdate()
    {
        RayCastDetection(); //���߼��
    }

    private void OnAnimatorIK(int layerIndex)
    {
        lFootIK = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        rFootIK = animator.GetIKPosition(AvatarIKGoal.RightFoot);

        if (!IKEnabled)
            return;
        
        //����IKȨ��
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("leftIK"));
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("leftIK"));

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, animator.GetFloat("rightIK"));
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat("rightIK"));

        //����IKλ�ú���ת
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, lFootPos);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, lFootRot);

        animator.SetIKPosition(AvatarIKGoal.RightFoot, rFootPos);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, rFootRot);
    }

    private void RayCastDetection()
    {
        Debug.DrawLine(lFootIK + (Vector3.up * 0.5f), lFootIK + Vector3.down * raycastDist, Color.blue, Time.deltaTime);
        Debug.DrawLine(rFootIK + (Vector3.up * 0.5f), rFootIK + Vector3.down * raycastDist, Color.blue, Time.deltaTime);

        if (Physics.Raycast(lFootIK + (Vector3.up * 0.5f), Vector3.down, out RaycastHit lhit, raycastDist + 5f, IKLayer))
        {
            Debug.DrawRay(lhit.point, lhit.normal, Color.red, Time.deltaTime);
            lFootPos = lhit.point + Vector3.up * raycastOffset; //��Ҫoffset����ΪҪ�ýŵ�λ�ý���һ����̧�����Է�ֹ��ģ
            lFootRot = Quaternion.FromToRotation(Vector3.up, lhit.normal) * transform.rotation; //  �Ų�����תֵ�����߼�ⷵ�صķ�����Ϣ�й�
        }

        if(Physics.Raycast(rFootIK + (Vector3.up * 0.5f), Vector3.down, out RaycastHit rhit, raycastDist + 5f, IKLayer))
        {
            Debug.DrawRay(rhit.point, rhit.normal, Color.red, Time.deltaTime);
            rFootPos = rhit.point + Vector3.up * raycastOffset;
            rFootRot = Quaternion.FromToRotation(Vector3.up, rhit.normal) * transform.rotation;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;   //IKλ��
        Gizmos.DrawWireSphere(lFootIK, IKSphereRadius);
        Gizmos.DrawWireSphere(rFootIK, IKSphereRadius);

        Gizmos.color = Color.cyan; //�ŵ�λ��
        Gizmos.DrawWireSphere(lFootPos, posSpehereRadius);
        Gizmos.DrawWireSphere(rFootPos, posSpehereRadius);

    }
}

