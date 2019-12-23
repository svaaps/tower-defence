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
    private Tile[,] tiles;

    [SerializeField]
    private Vector2Int size;

    [SerializeField]
    private Tile tilePrefab;

    [SerializeField]
    private Transform blockContainer;

    private Camera cam;
    public Vector3 MousePlane { get; private set; }
    public Tile MouseTile { get; private set; }
    public Vector2Int MouseCoord { get; private set; }
    public Vector2Int Size => size;
    public bool Changed { get; private set; }

    private bool changed;

    private void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    public void Update()
    {
        MousePlane = ScreenPointToRayPlaneIntersection(Input.mousePosition, 0, cam);
        MouseCoord = new Vector2Int(Mathf.FloorToInt(MousePlane.x), Mathf.FloorToInt(MousePlane.z));
        MouseTile = GetTile(MouseCoord.x, MouseCoord.y);
    }

    public void DeclareMapChanged()
    {
        changed = true;
    }

    public void Tick()
    {
        Changed = changed;
        changed = false;

        foreach (Tile tile in tiles)
        {
            if (tile.structure != null && tile.structure.placed)
                tile.structure.EarlyTick();
        }
        foreach (Tile tile in tiles)
        {
            if (tile.structure != null && tile.structure.placed)
                tile.structure.Tick();
        }

        List<Block> blocks = new List<Block>();

        foreach (Tile tile in tiles)
        {
            if (tile.block)
            {
                tile.block.waiting = false;
                tile.block.moved = false;
                tile.block.moving = false;
                tile.block.updated = false;
                blocks.Add(tile.block);
                if (Changed)
                    tile.block.RecalculatePath();
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
                    Tile tile = block.Path.Nodes[1] as Tile;

                    if (!tile.block)
                    {
                        block.moving = true;
                        block.GoToNext();
                        block.updated = true;
                    }
                    else if (!tile.block.moved)
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

            if (block.Path.FoundPath && block.Path.Nodes.Count > 1)
            {
                if (!(block.Path.Nodes[1] as Tile).block)
                {
                    block.moving = true;
                    block.GoToNext();
                    block.updated = true;
                }
            }
        }

        foreach (Tile tile in tiles)
        {
            if (tile.structure != null && tile.structure.placed)
                tile.structure.LateTick();
        }
    }

    public void InterTick(float t)
    {
        if (tiles != null)
        foreach (Tile tile in tiles)
            if (tile.structure != null && tile.structure.placed)
                tile.structure.InterTick(t);

        if (tiles != null)
        foreach (Tile tile in tiles)
        {
            if (!tile.block)
                continue;

            if (!tile.block.moving)
                continue;

            //node.block.angle = t * 90 - node.block.angle;
            Vector3 lerp = Vector3.Lerp(new Vector3(tile.block.prevX + 0.5f, 0, tile.block.prevY + 0.5f), new Vector3(tile.pos.x + 0.5f, 0, tile.pos.y + 0.5f), t);
            tile.block.transform.position = lerp;
        }
    }

    public void BuildModeUpdate()
    {
        foreach(Tile tile in tiles)
        {
            if (tile.structure)
            {
                tile.structure.BuildModeUpdate(changed);
            }
        }
        changed = false;
    }

    public bool NearestBlock(Vector3 position, float maxRange, out Block nearest)
    {
        nearest = null;
        float sqDistance = float.MaxValue;
        if (tiles != null)
        foreach(Tile tile in tiles)
        {
            if (!tile.block)
                continue;
            float d = SquareDistance(position, tile.block.transform.position);
            if (d > maxRange * maxRange)
                continue;
            if (nearest == null || sqDistance > d)
            {
                nearest = tile.block;
                sqDistance = d;
            }
        }
        return nearest;
    }

    public bool NearestStructure<T>(Vector3 position, out T nearest) where T : Structure
    {
        nearest = null;
        float sqDistance = float.MaxValue;
        foreach (Tile tile in tiles)
        {
            if (!tile.structure)
                continue;
            if (!(tile.structure is T))
                continue;
            float d = SquareDistance(position, new Vector3(tile.pos.x + 0.5f, 0, tile.pos.y + 0.5f));
            if (nearest == null || sqDistance > d)
            {
                nearest = tile.structure as T;
                sqDistance = d;
            }
        }
        return nearest;
    }

    public bool NearestBlockGoal(Vector3 position, out BlockGoal nearest)
    {
        nearest = null;
        float sqDistance = float.MaxValue;
        foreach(Tile tile in tiles)
        {
            if (!tile.structure)
                continue;
            if (!(tile.structure is BlockGoal))
                continue;
            float d = SquareDistance(position, new Vector3(tile.pos.x + 0.5f, 0, tile.pos.y + 0.5f));
            if (nearest == null || sqDistance > d)
            {
                nearest = tile.structure as BlockGoal;
                sqDistance = d;
            }
        }
        return nearest;
    }

    public bool NearestBlockGoal(Tile position, out BlockGoal nearest, out PathFinding.Path path)
    {
        path = new PathFinding.Path(PathFinding.PathResult.FailureNoPath);
        nearest = null;

        foreach(Tile tile in tiles)
        {
            if (!tile.structure)
                continue;
            if (!(tile.structure is BlockGoal))
                continue;

            PathFinding.Path thisPath = PathFinding.PathFind(position, tile, PATHFINDING_MAX_DISTANCE, PATHFINDING_MAX_TRIES, PathFinding.StandardCostFunction);

            if (thisPath.FoundPath)
            {
                if (nearest == null || thisPath.TotalCost(PathFinding.StandardCostFunction) < path.TotalCost(PathFinding.StandardCostFunction))
                {
                    nearest = tile.structure as BlockGoal;
                    path = thisPath;
                }
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

    public bool AddBlock(Block prefab, int x, int y, out Block block)
    {
        block = null;
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
            return false;

        if (tiles[x, y].block != null)
            return false;

        block = tiles[x, y].block = Instantiate(prefab, blockContainer);
        block.Init(x, y);
        
        return true;
    }

    public bool RemoveBlock(Block block)
    {
        if (block == null)
            return false;

        if (block.Current)
            block.Current.block = null;
        
        Destroy(block.gameObject);

        return true;
    }

    public Tile GetTile(int x, int y) => x < 0 || y < 0 || x >= size.x || y >= size.y || tiles == null ? null : tiles[x, y];
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
        tiles = new Tile[size.x, size.y];
    }

    [ContextMenu("Clear")]
    public void Clear()
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
        if (tiles != null)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    if (tiles[x, y] != null)
                        DestroyImmediate(tiles[x, y].gameObject);
                }
        }

        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        InitArrays();
    }

    public void ClearBlocks()
    {
        if (tiles != null)
        foreach(Tile tile in tiles)
        {
            if (tile.block)
            {
                Destroy(tile.block.gameObject);
                tile.block = null;
            }
        }
    }

    public void ClearStructures(bool sudo)
    {
        if (tiles != null)
            foreach(Tile tile in tiles)
            {
                if (tile.structure && (tile.structure.isRemovable || sudo))
                {
                    Destroy(tile.structure.gameObject);
                    tile.structure = null;
                    DeclareMapChanged();
                }
            }
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();

        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {

#if UNITY_EDITOR
                tiles[x, y] = PrefabUtility.InstantiatePrefab(tilePrefab, transform) as Tile;
#else
                nodes[x, y] = Object.Instantiate(emptyNodePrefab, transform);
#endif

                tiles[x, y].Init(x, y);
            }

        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                Tile t = GetTile(x, y);
                Tile n = GetTile(x, y + 1);
                Tile e = GetTile(x + 1, y);

                if (t) t.NorthNeighbour = n;
                if (n) n.SouthNeighbour = t;
                if (t) t.EastNeighbour = e;
                if (e) e.WestNeighbour = t;
            }

        BakeNavMesh();
        CenterCamera();
    }

    public bool PlaceStructure(Structure prefab, int x, int y, int rotation, out Structure structure, bool sudo = false)
    {
        structure = null;
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
            return false;

        if (tiles[x, y].structure)
        {
            if (!sudo && prefab != null && !tiles[x, y].structure.canBeBuiltOver)
                return false;

            if (!sudo && prefab == null && !tiles[x, y].structure.isRemovable)
                return false;

            Destroy(tiles[x, y].structure.gameObject);
            tiles[x, y] = null;
            DeclareMapChanged();
        }

        if (prefab == null)
            return true;

        structure = Instantiate(prefab, tiles[x, y].transform);
        structure.tile = tiles[x, y];
        structure.transform.localPosition = new Vector3(0.5f, 0, 0.5f);
        structure.Rotation = rotation;
        tiles[x, y].structure = structure;
        DeclareMapChanged();
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
            DeclareMapChanged();
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
            DeclareMapChanged();
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

        if (!tiles[x, y].structure)
            return false;

        if (!sudo && !tiles[x, y].structure.isRemovable)
            return false;

        Destroy(tiles[x, y].structure.gameObject);
        tiles[x, y].structure = null;
        DeclareMapChanged();
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
        DeclareMapChanged();

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

    public Tile ScreenPointToRayPlaneNode(Vector3 screenPos, float y, Camera camera)
    {
        Vector3 intersection = ScreenPointToRayPlaneIntersection(screenPos, y, camera);

        return GetTile(Mathf.FloorToInt(intersection.x), Mathf.FloorToInt(intersection.z));
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
