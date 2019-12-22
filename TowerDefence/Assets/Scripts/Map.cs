using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Map : MonoBehaviour
{
    private static Map instance;
    public static Map Instance => instance;

    public static float PATHFINDING_MAX_DISTANCE = 100;
    public static int PATHFINDING_MAX_TRIES = 1000;

    private Wall[,] verticalWalls;
    private Wall[,] horizontalWalls;
    private Node[,] nodes;

    [SerializeField]
    private Vector2Int size;

    [SerializeField]
    private Node emptyNodePrefab;

    private Camera cam;
    public Vector3 MousePlane { get; private set; }
    public Node MouseNode { get; private set; }
    public Vector2Int MouseTile { get; private set; }
    public Vector2Int Size => size;
    public bool Changed { get; private set; }

    private bool changed;

    public void Awake()
    {
        instance = this;
        cam = Camera.main;
    }
    public void Start()
    {
        Generate();
        BakeNavMesh();
        CenterCamera();
    }

    public void Update()
    {
        MousePlane = ScreenPointToRayPlaneIntersection(Input.mousePosition, 0, cam);
        MouseTile = new Vector2Int(Mathf.FloorToInt(MousePlane.x), Mathf.FloorToInt(MousePlane.z));
        MouseNode = GetNode(MouseTile.x, MouseTile.y);
    }

    public void DeclareMapChanged()
    {
        changed = true;
    }

    public void Tick()
    {
        Changed = changed;

        foreach (Node node in nodes)
        {
            if (node.structure != null && node.structure.placed)
                node.structure.EarlyTick();
        }

        foreach (Node node in nodes)
        {
            if (node.structure != null && node.structure.placed)
                node.structure.Tick();
        }

        List<Block> blocks = new List<Block>();

        foreach(Node node in nodes)
        {
            if (node.block)
            {
                node.block.waiting = false;
                node.block.moved = false;
                node.block.moving = false;
                node.block.updated = false;
                blocks.Add(node.block);
                if (Changed)
                    node.block.RecalculatePath();
            }
        }

        for (int t = 0; t < 100; t++)
        {
            foreach(Block block in blocks)
            {
                if (block.updated || block.moved)
                    continue;

                if (block.Path.FoundPath && block.Path.Nodes.Count > 1)
                {
                    if (!block.Path.Nodes[1].block)
                    {
                        block.moving = true;
                        block.GoToNext();
                        block.updated = true;
                    }
                    else if (!block.Path.Nodes[1].block.moved)
                    {
                        block.waiting = true;
                    }
                    else
                    {
                        block.updated = true;
                        block.moving = false;
                    }
                }
            }

            for (int i = blocks.Count - 1; i >= 0; i--)
            {
                if (blocks[i].updated)
                    blocks.RemoveAt(i);
            }
        }

        foreach (Block block in blocks)
        {
            if (block.updated || block.moved)
                continue;

            if (block.Path.FoundPath)
            {
                if (!block.Path.Nodes[1].block)
                {
                    block.moving = true;
                    block.GoToNext();
                    block.updated = true;
                }
            }
        }

        foreach (Node node in nodes)
        {
            if (node.structure != null && node.structure.placed)
                node.structure.LateTick();
        }
    }

    public void InterTick(float t)
    {
        foreach (Node node in nodes)
            if (node.structure != null && node.structure.placed)
                node.structure.InterTick(t);

        foreach (Node node in nodes)
        {
            if (!node.block)
                continue;

            if (!node.block.moving)
                continue;

            //node.block.angle = t * 90 - node.block.angle;
            Vector3 lerp = Vector3.Lerp(new Vector3(node.block.prevX + 0.5f, 0, node.block.prevY + 0.5f), new Vector3(node.pos.x + 0.5f, 0, node.pos.y + 0.5f), t);
            node.block.transform.position = lerp;
        }
    }

    public bool NearestBlock(Vector3 position, out Block nearest)
    {
        nearest = null;
        float sqDistance = float.MaxValue;
        foreach(Node node in nodes)
        {
            if (!node.block)
                continue;
            float d = SquareDistance(position, node.block.transform.position);
            if (nearest == null || sqDistance > d)
            {
                nearest = node.block;
                sqDistance = d;
            }
        }
        return nearest;
    }

    public bool NearestBlockGoal(Vector3 position, out BlockGoal nearest)
    {
        nearest = null;
        float sqDistance = float.MaxValue;
        foreach(Node node in nodes)
        {
            if (!node.structure)
                continue;
            if (!(node.structure is BlockGoal))
                continue;
            float d = SquareDistance(position, new Vector3(node.pos.x + 0.5f, 0, node.pos.y + 0.5f));
            if (nearest == null || sqDistance > d)
            {
                nearest = node.structure as BlockGoal;
                sqDistance = d;
            }
        }
        return nearest;
    }

    [ContextMenu("Center Camera")]
    public void CenterCamera()
    {
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(size.x / 2f, 0, size.y / 2f);
        cam.transform.position -= cam.transform.forward * 20;
    }

    public bool AddBlock(Block prefab, int x, int y)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
            return false;

        if (nodes[x, y].block != null)
            return false;

        nodes[x, y].block = Instantiate(prefab);
        nodes[x, y].block.Init(x, y);
        
        return true;
    }

    public bool AddBlock(Block prefab, int x, int y, PathFinding.Path path)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
            return false;

        if (nodes[x, y].block != null)
            return false;

        nodes[x, y].block = Instantiate(prefab);
        nodes[x, y].block.Init(x, y);
        nodes[x, y].block.Path = new PathFinding.Path(path);

        return true;
    }

    public Node GetNode(int x, int y) => x < 0 || y < 0 || x >= size.x || y >= size.y ? null : nodes[x, y];
    public Wall GetWall(int x, int y, bool vertical) => x < 0 || y < 0 || x >= size.x + 1 || y >= size.y + 1 ? null : vertical ? verticalWalls[x, y] : horizontalWalls[x, y];

    public Wall GetNorthWall(int x, int y)
    {
        return GetWall(x, y + 1, false);
    }

    public Wall GetEastWall(int x, int y)
    {
        return GetWall(x + 1, y, true);
    }

    public Wall GetSouthWall(int x, int y)
    {
        return GetWall(x, y, false);
    }

    public Wall GetWestWall(int x, int y)
    {
        return GetWall(x, y, true);
    }

    public Wall GetWall(int x, int y, int direction)
    {
        if (direction == 0)
            return GetNorthWall(x, y);
        if (direction == 1)
            return GetEastWall(x, y);
        if (direction == 2)
            return GetSouthWall(x, y);
        if (direction == 3)
            return GetWestWall(x, y);
        return null;
    }

    public float GetWallCost(int x, int y, int direction)
    {
        Wall get = GetWall(x, y, direction);
        return get == null ? 0 : get.cost;
    }

    private void InitArrays()
    {
        verticalWalls = new Wall[size.x + 1, size.y];
        horizontalWalls= new Wall[size.x, size.y + 1];
        nodes = new Node[size.x, size.y];
    }
    [ContextMenu("Clear")]
    private void Clear()
    {
        if (verticalWalls != null)
        {
            for (int y = 0; y < verticalWalls.GetLength(1); y++)
                for (int x = 0; x < verticalWalls.GetLength(0); x++)
                {
                    if (verticalWalls[x, y] != null)
                        DestroyImmediate(verticalWalls[x, y].gameObject);
                }
        }
        if (horizontalWalls != null)
        {
            for (int y = 0; y < horizontalWalls.GetLength(1); y++)
                for (int x = 0; x < horizontalWalls.GetLength(0); x++)
                {
                    if (horizontalWalls[x, y] != null)
                    DestroyImmediate(horizontalWalls[x, y].gameObject);
                }
        }
        if (nodes != null)
        {
            for (int y = 0; y < nodes.GetLength(1); y++)
                for (int x = 0; x < nodes.GetLength(0); x++)
                {
                    if (nodes[x, y] != null)
                        DestroyImmediate(nodes[x, y].gameObject);
                }
        }

        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        InitArrays();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();

        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {

#if UNITY_EDITOR
                nodes[x, y] = PrefabUtility.InstantiatePrefab(emptyNodePrefab, transform) as Node;
#else
                nodes[x, y] = Object.Instantiate(emptyNodePrefab, transform);
#endif

                nodes[x, y].Init(x, y);
            }

        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                Node t = GetNode(x, y);
                Node n = GetNode(x, y + 1);
                Node e = GetNode(x + 1, y);

                if (t) t.NorthNeighbour = n;
                if (n) n.SouthNeighbour = t;
                if (t) t.EastNeighbour = e;
                if (e) e.WestNeighbour = t;
            }
    }

    private void IdentifyNeighbours(int x, int y)
    {
        Node t = GetNode(x, y);
        Node n = GetNode(x, y + 1);
        Node e = GetNode(x + 1, y);
        Node s = GetNode(x, y - 1);
        Node w = GetNode(x - 1, y);

        if (t) t.NorthNeighbour = n;
        if (n) n.SouthNeighbour = t;

        if (t) t.EastNeighbour = e;
        if (e) e.WestNeighbour = t;

        if (t) t.SouthNeighbour = s;
        if (s) s.NorthNeighbour = t;

        if (t) t.WestNeighbour = w;
        if (w) w.EastNeighbour = t;
    }

    public bool PlaceStructure(Structure prefab, int x, int y, int rotation, out Structure structure, bool sudo = false)
    {
        structure = null;
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
            return false;

        if (nodes[x, y].structure)
        {
            if (!sudo && prefab != null && !nodes[x, y].structure.canBeBuiltOver)
                return false;

            if (!sudo && prefab == null && !nodes[x, y].structure.isRemovable)
                return false;

            Destroy(nodes[x, y].structure.gameObject);
            nodes[x, y] = null;
            changed = true;
        }

        if (prefab == null)
            return true;

        structure = Instantiate(prefab, nodes[x, y].transform);
        structure.node = nodes[x, y];
        structure.transform.localPosition = new Vector3(0.5f, 0, 0.5f);
        structure.Rotation = rotation;
        nodes[x, y].structure = structure;
        changed = true;
        //BakeNavMesh();
        return true;
    }

    public bool PlaceWall(Wall prefab, int x, int y, bool vertical, bool facesOutward, bool sudo = false)
    {
        if (vertical)
        {
            if (x < 0 || y < 0 || x >= size.x + 1 || y >= size.y)
                return false;

        }
        else
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y + 1)
                return false;
        }

        Wall existing = vertical ? verticalWalls[x, y] : horizontalWalls[x, y];

        if (existing)
        {
            if (!sudo && prefab != null && !existing.canBeBuiltOver)
                return false;

            if (!sudo && prefab == null && !existing.isRemovable)
                return false;

            Destroy(existing.gameObject);
            if (vertical)
                verticalWalls[x, y] = null;
            else
                horizontalWalls[x, y] = null;
            changed = true;
        }

        if (prefab == null)
            return true;

        Wall wall = Instantiate(prefab, transform);
        wall.pos = new Vector2Int(x, y);
        wall.vertical = vertical;
        wall.facesOutward = facesOutward;

        if (vertical)
        {
            verticalWalls[x, y] = wall;
            Changed = true;
            if (wall)
            {
                wall.transform.localPosition = new Vector3(x, 0, y + 0.5f);
                wall.transform.localRotation = Quaternion.Euler(facesOutward ? 90 : 270, 0, 0);
            }
        }
        else
        {
            horizontalWalls[x, y] = wall;
            if (wall)
            {
                wall.transform.localPosition = new Vector3(x + 0.5f, 0, y);
                wall.transform.localRotation = Quaternion.Euler(facesOutward ? 0 : 180, 0, 0);
            }
        }

        return true;
    }

    public bool DeleteStructure(int x, int y, bool sudo = false)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
            return false;

        if (!nodes[x, y].structure)
            return false;

        if (!sudo && !nodes[x, y].structure.isRemovable)
            return false;

        Destroy(nodes[x, y].structure.gameObject);
        nodes[x, y].structure = null;
        changed = true;
        return true;
    }

    public bool DeleteWall(int x, int y, bool vertical, bool sudo = false)
    {
        if (vertical)
        {
            if (x < 0 || y < 0 || x >= size.x + 1 || y >= size.y)
                return false;
        }
        else
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y + 1)
                return false;
        }

        Wall existing = vertical ? verticalWalls[x, y] : horizontalWalls[x, y];

        if (!existing)
            return false;

        if (!sudo && !existing.isRemovable)
            return false;

        Destroy(existing.gameObject);
        if (vertical)
            verticalWalls[x, y] = null;
        else
            horizontalWalls[x, y] = null;
        changed = true;

        return true;
    }

    public static float SquareDistance(float x1, float y1, float x2, float y2)
    {
        return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
    }

    public static float SquareDistance(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1);
    }

    public static float SquareDistance(Vector3 v1, Vector3 v2)
    {
        return SquareDistance(v1.x, v1.y, v1.z, v2.x, v2.y, v2.z);
    }

    public static float SquareDistance(Vector2 v1, Vector2 v2)
    {
        return SquareDistance(v1.x, v1.y, v2.x, v2.y);
    }

    public Node ScreenPointToRayPlaneNode(Vector3 screenPos, float y, Camera camera)
    {
        Vector3 intersection = ScreenPointToRayPlaneIntersection(screenPos, y, camera);

        return GetNode(Mathf.FloorToInt(intersection.x), Mathf.FloorToInt(intersection.z));
    }

    public static Vector3 ScreenPointToRayPlaneIntersection(Vector3 screenPos, float y, Camera camera)
    {
        Vector3 hit = Vector3.zero;
        Ray ray = camera.ScreenPointToRay(screenPos);
        if (new Plane(Vector3.up, new Vector3(0, y, 0)).Raycast(ray, out float distance))
            hit = ray.GetPoint(distance);
        return hit;
    }


    public void OnDrawGizmos()
    {
        Vector3 mouse = ScreenPointToRayPlaneIntersection(Input.mousePosition, 0, Camera.main);

        int tileX = Mathf.FloorToInt(mouse.x);
        int tileZ = Mathf.FloorToInt(mouse.z);
        Gizmos.color = Color.red;
        DrawTile(tileX, tileZ);
    }
    

    public static void DrawTile(int x, int z)
    {
        Gizmos.DrawLine(new Vector3(x, 0, z), new Vector3(x + 1, 0, z));
        Gizmos.DrawLine(new Vector3(x + 1, 0, z), new Vector3(x + 1, 0, z + 1));
        Gizmos.DrawLine(new Vector3(x + 1, 0, z + 1), new Vector3(x, 0, z + 1));
        Gizmos.DrawLine(new Vector3(x, 0, z + 1), new Vector3(x, 0, z));
    }

    [System.Serializable]
    public struct NavMeshBuildSettingsSerialized
    {
        public float agentRadius;
        public float agentHeight;
        public float agentSlope;
        public float agentClimb;
        public float minRegionArea;
        public int tileSize;
    }

    [SerializeField]
    private NavMeshBuildSettingsSerialized navMeshBuildSettings;

    [ContextMenu("Bake Nav Mesh")]
    private void BakeNavMesh()
    {
        NavMeshBuildSettings buildSettings = new NavMeshBuildSettings
        {
            agentRadius = navMeshBuildSettings.agentRadius,
            agentHeight = navMeshBuildSettings.agentHeight,
            agentSlope = navMeshBuildSettings.agentSlope,
            agentClimb = navMeshBuildSettings.agentClimb,
            minRegionArea = navMeshBuildSettings.minRegionArea,
            tileSize = navMeshBuildSettings.tileSize,
            overrideTileSize = true
        };

        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

        foreach (NavMeshObject nmo in GetComponentsInChildren<NavMeshObject>())
            sources.Add(nmo.NavMeshBuildSource());


        Vector3 size = Vector3.Scale(transform.lossyScale, new Vector3(this.size.x, 1, this.size.y) * 2);

        NavMeshData data = NavMeshBuilder.BuildNavMeshData(buildSettings, sources, new Bounds(Vector3.zero, size), Vector3.zero, Quaternion.identity);


        NavMesh.RemoveAllNavMeshData();
        NavMesh.AddNavMeshData(data);
    }
}
