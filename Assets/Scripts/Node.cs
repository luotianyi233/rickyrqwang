using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node : MonoBehaviour
{
    public HashSet<Node> Neighbours = new HashSet<Node>();
    public Node[] NeighboursArray = new Node[0];

    private Dictionary<Joint, Node> JointToPiece = new Dictionary<Joint, Node>();
    private Dictionary<Node, Joint> PieceToJoint = new Dictionary<Node, Joint>();

    private Vector3 frozenPos;
    private Quaternion forzenRot;
    private bool frozen; 
    private Rigidbody rb;

    public bool HasBrokenLinks { get; private set; }

    void FixedUpdate()
    {
        if(frozen)
        {
            transform.position = frozenPos;
            transform.rotation = forzenRot;
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") && isIsolated())
            Destroy(gameObject, 3f);
    }

    private bool isIsolated()
    {
        // ����Ƿ����κ���Ч�� Joint ���ӵ����� Node
        if (JointToPiece.Any(j => j.Key != null && j.Value != null))
            return false;

        // ����Ƿ������� Node ͨ�� Joint ���ӵ���ǰ Node
        else if (PieceToJoint.Count > 0)
            return false;

        else
            return true;
    }

    //��ʼ�����岢��ӽڵ�
    public void Init()
    {
        rb = GetComponent<Rigidbody>();
        Freeze();
        JointToPiece.Clear();
        PieceToJoint.Clear();
        
        //�����ö����ϵ�����joint�����������node������joint��node���ֵ����
        foreach (Joint joint in GetComponents<Joint>())
        {
            Node pieceNode = joint.connectedBody.GetComponent<Node>();
            if (pieceNode == null)
                pieceNode = joint.connectedBody.gameObject.AddComponent<Node>();

            JointToPiece[joint] = pieceNode;
            PieceToJoint[pieceNode] = joint;
        }

        //����PieceToJoint�ֵ��е�����node��������������ھӼ�����
        foreach(Node pieceNode in PieceToJoint.Keys)
        {
            Neighbours.Add(pieceNode);
            if(Neighbours.Contains(pieceNode))
                pieceNode.Neighbours.Add(this);
        }
        NeighboursArray = Neighbours.ToArray();
    }

    //��joint���Ѳŵ���������ѽڵ�ķ���
    private void OnJointBreak(float breakForce)
    {
        HasBrokenLinks = true;
    }

    //������ѵ�����
    public void CleanBrokenLinks()
    {
        //JointToPiece�ֵ���Joint���󱻱��Ϊfalse��ȫ������brokenLinks�б�
        List<Joint> brokenLinks = JointToPiece.Keys.Where(j => j == false).ToList();

        //�����б��ÿ��Joint
        foreach (Joint link in brokenLinks)
        {
            //ͨ��JointToPiece�ֵ��ȡNode����body
            Node body = JointToPiece[link];

            //��JointToPiece�ֵ��Ƴ���ǰjoint�ļ�ֵ�ԣ�����PieceToJoint�ֵ����Ƴ���node
            JointToPiece.Remove(link);
            PieceToJoint.Remove(body);

            //���ھӼ������Ƴ���body�йص�����node
            Neighbours.Remove(body);
        }
        //�ھ��б��Ѹ��£����¸�ֵ
        NeighboursArray = Neighbours.ToArray();
        HasBrokenLinks = false;
    }

    void Freeze()
    {
        frozen = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        frozenPos = rb.transform.position;
        forzenRot = rb.transform.rotation;
    }

    public void Unfreeze()
    {
        frozen = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
    }
}
