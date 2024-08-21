using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class BreakableWallController : MonoBehaviour,IRespawnable
{

    [SerializeField] private GameObject cubeMeshObj;
    [SerializeField] private Material insideMaterial;
    [SerializeField] private Material outsideMaterial;
    [SerializeField] private float wallDensity = 50;
    [SerializeField] private float internalStrength = 100;
    [SerializeField] private float jointBreakForce = 1000;
    [SerializeField] private int totalPieces = 500;

    private GameObject fractureGameObject;  //

    private Node[] nodes;

    static List<GameObject> targets=new List<GameObject>();
    GameObject origin;

    public void Awake()
    {
        //Ԥ�ָ�
        PreBake();  
        //��ȡԭmesh��GameObject
        origin = gameObject.transform.GetChild(0).gameObject;

        RespawnController.Instance.RegisterRespawnable(this);
    }

    public void OnDestroy()
    {
        RespawnController.Instance.UnregisterRespawnable(this);
    }

    public void Respawn()
    {
        if (targets != null)
        {
            //��������Piece�ĸ�����Pieces�����Ѽ��˵�����ڵ����У�����ɾ��Pieces
            foreach (GameObject target in targets)  
            {
                if (target!=null&&!target.activeSelf)
                    continue;
                Destroy(target);
            }

            //��ȡ���ؽű��������壨Ҳ����orginWall�������Ϊδ����״̬��˵���ѱ��ڵ����У��������¼������һ��PreBake������ƬԤ����
            if (!origin.activeSelf) 
            {
                origin.SetActive(true);
                PreBake();
            }

        }
    }
    private void FixedUpdate()
    {
        //��������Node��joint����������
        bool runSearch = false;
        foreach (Node brokenNodes in nodes.Where(n => n.HasBrokenLinks))
        {
            brokenNodes.CleanBrokenLinks();
            runSearch = true;
        }

        //Unfreeze�ڵ�
        if (runSearch)
            SearchGraph(nodes);
    }

    //����ͼ�ṹ
    //�ҵ�����isKinematic��Node����Ϊ��㲢����dfs�������������ھӼ���search�У�����search�еĽڵ�Unfreeze
    private void SearchGraph(Node[] objects)
    {
        if (objects == null || objects.Length == 0)
            return;

        //��objects������isKinematic��Node����starts
        List<Node> starts = objects
            .Where(o => o != null && o.gameObject != null && o.GetComponent<Rigidbody>() != null && o.GetComponent<Rigidbody>().isKinematic)
            .ToList();

        //��objects��ʼ��������
        ISet<Node> search = new HashSet<Node>(objects.Where(o => o != null && o.gameObject != null));
        foreach (Node o in starts)
        {
            if (search.Contains(o))
            {
                HashSet<Node> subVisited = new HashSet<Node>();
                Traverse(o, search, subVisited);
                //search = new HashSet<Node>(search.Where(s => !subVisited.Contains(s)));
                search.ExceptWith(subVisited);
            }
        }
        foreach (Node sub in search)
        {
            if(sub!=null && sub.gameObject!=null)
            sub.Unfreeze();
        }
    }

    //dfs
    private void Traverse(Node o, ISet<Node> search, ISet<Node> visited)
    {
        //�ڵ���������search�����Ƿ�δ����
        if (o != null && o.gameObject != null&&search.Contains(o) && !visited.Contains(o))
        {
            //�ڵ㱻����
            visited.Add(o);

            //������ǰ�ڵ�������ھ�
            for (int i = 0; i < o.NeighboursArray.Length; i++)
            {
                Node neighbour = o.NeighboursArray[i];
                //�ݹ����
                if (neighbour != null && neighbour.gameObject != null)
                    Traverse(neighbour, search, visited);
            }
        }
    }

    //��������������������Ƭ���зָ�
    private void PreBake()
    {
        //���ǽ��obj��mesh
        Mesh cubeMesh = GetMesh(cubeMeshObj);

        //���������������nvidia��api���зָ�
        Random rng = new Random();
        int seed = rng.Next();

        NvBlastExtUnity.setSeed(seed);

        //����nvidia blast�ĸ�ʽ����
        NvMesh nvMesh = new NvMesh(
            cubeMesh.vertices,
            cubeMesh.normals,
            cubeMesh.uv,
            cubeMesh.vertexCount,
            cubeMesh.GetIndices(0),
            (int)cubeMesh.GetIndexCount(0)
        );

        List<Mesh> meshes = FractureMeshesInNvblast(totalPieces, nvMesh);
        float pieceWeight = Volume(cubeMesh) * wallDensity / totalPieces;
        List<GameObject> pieces = BuildPieces(insideMaterial, outsideMaterial, meshes, pieceWeight);

        foreach (GameObject piece in pieces)
        {
            ConnectTouchingPieces(piece, jointBreakForce);  //Ϊ��Ƭ��ӹؽ�
        }

        //Ϊ��Ƭ����������
        fractureGameObject = new GameObject("Pieces");  
        fractureGameObject.tag = "BreakableWallParent";
        foreach (GameObject piece in pieces)
        {
            piece.transform.SetParent(fractureGameObject.transform, false);
        }

        //��ʼ��ÿ����Ƭ
        Initial(fractureGameObject.GetComponentsInChildren<Rigidbody>());

        //�����fractureGameObject����targets�б�
        targets.Add(fractureGameObject);

        fractureGameObject.SetActive(false);
    }

    public void SetPiecesActive()
    {
        fractureGameObject.SetActive(true);
    }

    //Ϊÿ����Ƭ���Node�ű�
    private void Initial(Rigidbody[] bodies)
    {
        nodes = new Node[bodies.Length];
        for (int i = 0; i < bodies.Length; i++)
        {
            Node node = bodies[i].GetComponent<Node>();
            if (node == null)
                node = bodies[i].gameObject.AddComponent<Node>();
            node.Init();
            nodes[i] = node;
        }
    }

    //���obj��mesh
    private Mesh GetMesh(GameObject gameObject)
    {
        CombineInstance[] combineInstances = gameObject
            .GetComponentsInChildren<MeshFilter>()
            .Select(mf => new CombineInstance()
            {
                mesh = mf.mesh,
                transform = mf.transform.localToWorldMatrix
            }).ToArray();

        Mesh totalMesh = new Mesh();
        totalMesh.CombineMeshes(combineInstances, true);
        return totalMesh;
    }

    //�������зָ�õ�mesh��ʹ��BuildPiece����������Ƭ����������������tag
    private List<GameObject> BuildPieces(Material insideMaterial, Material outsideMaterial, List<Mesh> meshes, float pieceWeight)
    {
        return meshes.Select((pieceMesh, i) =>
        {
            GameObject piece = BuildPiece(insideMaterial, outsideMaterial, pieceMesh, pieceWeight);
            piece.name += $" [{i}]";
            piece.tag = "BreakableWall";
            piece.layer = LayerMask.NameToLayer("Pieces");
            return piece;
        }).ToList();
    }

    //ʹ��Nvidia blast��api���зָ�
    private List<Mesh> FractureMeshesInNvblast(int totalPieces, NvMesh nvMesh)
    {
        NvFractureTool fractureTool = new NvFractureTool();
        fractureTool.setRemoveIslands(false);
        fractureTool.setSourceMesh(nvMesh);
        NvVoronoiSitesGenerator sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(totalPieces);
        fractureTool.voronoiFracturing(0, sites);
        fractureTool.finalizeFracturing();

        // �����Ƭ��meshes
        int meshCount = fractureTool.getChunkCount();
        List<Mesh> meshes = new List<Mesh>(fractureTool.getChunkCount());
        for (int i = 1; i < meshCount; i++)
        {
            meshes.Add(ExportPiecesMesh(fractureTool, i));
        }

        return meshes;
    }

    //�����Ƭ��Mesh
    private Mesh ExportPiecesMesh(NvFractureTool fractureTool, int index)
    {
        NvMesh outside = fractureTool.getChunkMesh(index, false);
        NvMesh inside = fractureTool.getChunkMesh(index, true);
        Mesh pieceMesh = outside.toUnityMesh();
        pieceMesh.subMeshCount = 2;
        pieceMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
        return pieceMesh;
    }

    //������Ƭ�����Component
    private GameObject BuildPiece(Material insideMaterial, Material outsideMaterial, Mesh mesh, float mass)
    {
        GameObject pieces = new GameObject("Piece");

        MeshRenderer renderer = pieces.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new[]
        {
            outsideMaterial,
            insideMaterial
        };

        MeshFilter meshFilter = pieces.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        Rigidbody rigidbody = pieces.AddComponent<Rigidbody>();
        rigidbody.mass = mass;

        MeshCollider meshCollider = pieces.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;

        return pieces;
    }

    // ����ǰpiece����Χ��piece���<FixedJoint>������������ǣ��������Ӷ�����
    //touchRadius�ǽӴ��İ뾶
    private void ConnectTouchingPieces(GameObject piece, float jointBreakForce, float touchRadius = .01f)
    {
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        Mesh mesh = piece.GetComponent<MeshFilter>().mesh;

        // ��ȡ��piece�Ӵ�������collider��ʵ���Ͼ�������
        HashSet<Collider> overlaps = new HashSet<Collider>(
            mesh.vertices
            .Select(v => piece.transform.TransformPoint(v))
            .SelectMany(v => Physics.OverlapSphere(v, touchRadius))
            .Where(o => o.GetComponent<Rigidbody>())
            );

        foreach (Collider overlap in overlaps)
        {
            if (overlap.gameObject != piece.gameObject)
            {
                var joint = overlap.gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = rb;
                joint.breakForce = jointBreakForce;
            }
        }
    }

    //����һ��mesh�����
    private float Volume(Mesh mesh)
    {
        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += TriangleVolume(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }

    //һ�������ε����
    private float TriangleVolume(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }
}