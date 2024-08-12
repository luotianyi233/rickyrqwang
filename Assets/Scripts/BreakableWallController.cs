using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class BreakableWallController : MonoBehaviour
{

    [SerializeField] private GameObject cubeMeshObj;
    [SerializeField] private Material insideMaterial;
    [SerializeField] private Material outsideMaterial;
    [SerializeField] private float wallDensity = 50;
    [SerializeField] private float internalStrength = 100;
    [SerializeField] private float jointBreakForce = 1000;
    [SerializeField] private int totalPieces = 500;

    private GameObject fractureGameObject;

    private Node[] nodes;

    public void Start()
    {
        PreBake();
        //transform.GetChild(0).gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        //遍历所有Node，joint断裂则清理
        bool runSearch = false;
        foreach (Node brokenNodes in nodes.Where(n => n.HasBrokenLinks))
        {
            brokenNodes.CleanBrokenLinks();
            runSearch = true;
        }

        if (runSearch)
            SearchGraph(nodes);
    }

    //遍历图结构
    //找到所有isKinematic的Node，作为起点并进行dfs，将它的所有邻居加入search中，最后对search中的节点Unfreeze
    private void SearchGraph(Node[] objects)
    {
        if (objects == null || objects.Length == 0)
            return;

        //将objects中所有isKinematic的Node加入starts
        List<Node> starts = objects
            .Where(o => o != null && o.gameObject != null && o.GetComponent<Rigidbody>() != null && o.GetComponent<Rigidbody>().isKinematic)
            .ToList();

        //用objects初始化搜索表
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
        //节点在搜索集search中且是否未访问
        if (o != null && o.gameObject != null&&search.Contains(o) && !visited.Contains(o))
        {
            //节点被访问
            visited.Add(o);

            //遍历当前节点的所有邻居
            for (int i = 0; i < o.NeighboursArray.Length; i++)
            {
                Node neighbour = o.NeighboursArray[i];
                //递归调用
                if (neighbour != null && neighbour.gameObject != null)
                    Traverse(neighbour, search, visited);
            }
        }
    }

    //核心主方法，将所有碎片进行分割
    private void PreBake()
    {
        //获得墙面obj的mesh
        Mesh cubeMesh = GetMesh(cubeMeshObj);

        //随机生成种子用于nvidia的api进行分割
        Random rng = new Random();
        int seed = rng.Next();

        NvBlastExtUnity.setSeed(seed);

        //按照nvidia blast的格式保存
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
            ConnectTouchingPieces(piece, jointBreakForce);
        }

        fractureGameObject = new GameObject("Pieces");

        foreach (GameObject piece in pieces)
        {
            piece.transform.SetParent(fractureGameObject.transform, false);
        }

        Initial(fractureGameObject.GetComponentsInChildren<Rigidbody>());

        fractureGameObject.SetActive(false);
    }

    public void SetPiecesActive()
    {
        fractureGameObject.SetActive(true);
    }

    //为每个碎片添加Node脚本
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

    //获得obj的mesh
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

    //遍历所有分割好的mesh，使用BuildPiece创建单个碎片，并对他们命名加tag
    private List<GameObject> BuildPieces(Material insideMaterial, Material outsideMaterial, List<Mesh> meshes, float pieceWeight)
    {
        return meshes.Select((pieceMesh, i) =>
        {
            GameObject piece = BuildPiece(insideMaterial, outsideMaterial, pieceMesh, pieceWeight);
            piece.name += $" [{i}]";
            piece.tag = "Wall";
            return piece;
        }).ToList();
    }

    //使用Nvidia blast的api进行分割
    private List<Mesh> FractureMeshesInNvblast(int totalPieces, NvMesh nvMesh)
    {
        NvFractureTool fractureTool = new NvFractureTool();
        fractureTool.setRemoveIslands(false);
        fractureTool.setSourceMesh(nvMesh);
        NvVoronoiSitesGenerator sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(totalPieces);
        fractureTool.voronoiFracturing(0, sites);
        fractureTool.finalizeFracturing();

        // 输出碎片的meshes
        int meshCount = fractureTool.getChunkCount();
        List<Mesh> meshes = new List<Mesh>(fractureTool.getChunkCount());
        for (int i = 1; i < meshCount; i++)
        {
            meshes.Add(ExportPiecesMesh(fractureTool, i));
        }

        return meshes;
    }

    //输出碎片的Mesh
    private Mesh ExportPiecesMesh(NvFractureTool fractureTool, int index)
    {
        NvMesh outside = fractureTool.getChunkMesh(index, false);
        NvMesh inside = fractureTool.getChunkMesh(index, true);
        Mesh pieceMesh = outside.toUnityMesh();
        pieceMesh.subMeshCount = 2;
        pieceMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
        return pieceMesh;
    }

    //构建碎片
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

    // 给当前chunk的周围的chunk添加<FixedJoint>组件并连接他们，设置连接断裂力
    //touchRadius是接触的半径
    private void ConnectTouchingPieces(GameObject piece, float jointBreakForce, float touchRadius = .01f)
    {
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        Mesh mesh = piece.GetComponent<MeshFilter>().mesh;

        // 获取与piece接触的所有collider，实际上就是相邻
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

    //计算一个mesh的体积
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

    //一个三角形的体积
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