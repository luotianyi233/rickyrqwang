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
        // 检查是否有任何有效的 Joint 连接到其他 Node
        if (JointToPiece.Any(j => j.Key != null && j.Value != null))
            return false;

        // 检查是否有其他 Node 通过 Joint 连接到当前 Node
        else if (PieceToJoint.Count > 0)
            return false;

        else
            return true;
    }

    //初始化物体并添加节点
    public void Init()
    {
        rb = GetComponent<Rigidbody>();
        Freeze();
        JointToPiece.Clear();
        PieceToJoint.Clear();
        
        //遍历该对象上的所有joint，给他们添加node，并将joint与node用字典关联
        foreach (Joint joint in GetComponents<Joint>())
        {
            Node pieceNode = joint.connectedBody.GetComponent<Node>();
            if (pieceNode == null)
                pieceNode = joint.connectedBody.gameObject.AddComponent<Node>();

            JointToPiece[joint] = pieceNode;
            PieceToJoint[pieceNode] = joint;
        }

        //遍历PieceToJoint字典中的所有node，将他们添加至邻居集合内
        foreach(Node pieceNode in PieceToJoint.Keys)
        {
            Neighbours.Add(pieceNode);
            if(Neighbours.Contains(pieceNode))
                pieceNode.Neighbours.Add(this);
        }
        NeighboursArray = Neighbours.ToArray();
    }

    //有joint断裂才调用清理断裂节点的方法
    private void OnJointBreak(float breakForce)
    {
        HasBrokenLinks = true;
    }

    //清除断裂的链接
    public void CleanBrokenLinks()
    {
        //JointToPiece字典中Joint对象被标记为false的全部存入brokenLinks列表
        List<Joint> brokenLinks = JointToPiece.Keys.Where(j => j == false).ToList();

        //遍历列表的每个Joint
        foreach (Joint link in brokenLinks)
        {
            //通过JointToPiece字典获取Node对象body
            Node body = JointToPiece[link];

            //从JointToPiece字典移除当前joint的键值对，并从PieceToJoint字典中移除该node
            JointToPiece.Remove(link);
            PieceToJoint.Remove(body);

            //从邻居集合中移除与body有关的所有node
            Neighbours.Remove(body);
        }
        //邻居列表已更新，重新赋值
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
