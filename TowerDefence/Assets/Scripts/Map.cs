using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Map : MonoBehaviour
{

    private static Map instance;
    public static Map Instance => instance;

    [SerializeField]
    private Vector2Int size;

    private Wall[,] verticalWalls;
    private Wall[,] horizontalWalls;
    private Node[,] nodes;

    [SerializeField]
    private Node emptyNodePrefab;

    [SerializeField]
    private Mob mobPrefab;

    public Structure placeStructurePrefab;

    private Node mouseNode;

    [SerializeField]
    private Path path;
    private Node start, end;

    private Camera cam;

    [SerializeField]
    public float pathMaxDistance;

    [SerializeField]
    public int pathMaxTries;

    public void Awake()
    {
        instance = this;
        cam = Camera.main;

        
    }
    public void Start()
    {
        Generate();
        CenterCamera();

        nodes[7, 0].mob = Instantiate(mobPrefab);
        nodes[7, 0].mob.Init(7, 0);
    }

    public void Update()
    {
        mouseNode = ScreenPointToRayPlaneNode(Input.mousePosition, 0, cam);


        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0) && mouseNode != null)
        {
            PlaceStructure(placeStructurePrefab, mouseNode.pos.x, mouseNode.pos.y, 0);
        }


        if (Input.GetMouseButtonDown(1) && mouseNode != null)
        {
            DeleteStructure(mouseNode.pos.x, mouseNode.pos.y);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            start = mouseNode;
            path = PathFind(start, end, 1000, 1000, StandardCostFunction);
            Debug.Log(path.result);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            end = mouseNode;
            path = PathFind(start, end, 1000, 1000, StandardCostFunction);
            Debug.Log(path.result);
        }

        
    }
    

    public void Tick()
    {
        

        foreach (Node node in nodes)
        {
            if (node.mob && node.mob.Path.foundPath && node.mob.moving && !node.mob.moved)
            {
                node.mob.GoToNext();
            }
        }
        foreach (Node node in nodes)
        {
            if (node.mob)
            {
                node.mob.moved = false;
                //node.mob.SetPosition(node);
                node.mob.DetermineTarget();
            }
        }
    }

    public void InterTick(float t)
    {
        foreach (Node node in nodes)
        {
            if (!node.mob)
                continue;

            if (!node.mob.moving)
                continue;

            node.mob.angle = t * 90 - node.mob.angle;
            Vector3 lerp = Vector3.Lerp(new Vector3(node.pos.x + 0.5f, 0.5f, node.pos.y + 0.5f), new Vector3(node.mob.nextX + 0.5f, 0.5f, node.mob.nextY + 0.5f), t);
            Debug.Log(lerp);
            node.mob.transform.position = lerp;
        }
    }

    [ContextMenu("Center Camera")]
    public void CenterCamera()
    {
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(size.x / 2f, 0, size.y / 2f);
        cam.transform.position -= cam.transform.forward * 80;
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
                nodes[x, y] = Instantiate(emptyNodePrefab, transform);
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

    public bool PlaceStructure(Structure prefab, int x, int y, int rotation, bool sudo = false)
    {
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
        }

        if (prefab == null)
            return true;

        nodes[x, y].structure = Instantiate(prefab, nodes[x, y].transform);
        nodes[x, y].structure.rotation = rotation;
        nodes[x, y].structure.transform.localPosition = new Vector3(0.5f, 0, 0.5f);
        nodes[x, y].structure.transform.localRotation = Quaternion.Euler(rotation * 90, 0, 0);

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

        return true;
    }

    public static float Distance(Node n1, Node n2)
    {
        return Distance(n1.pos.x, n1.pos.y, n2.pos.x, n2.pos.y);
    }

    public static float Distance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(SquareDistance(x1, y1, x2, y2));
    }

    public static float SquareDistance(float x1, float y1, float x2, float y2)
    {
        return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
    }

    public delegate float CostFunction(float distance, float cost, float crowFliesDistance, int steps);

    public static float StandardCostFunction(float distance, float cost, float crowFliesDistance, int steps)
    {
        return distance + cost + crowFliesDistance;
    }

    public static float NoAdditionalsCostFunction(float distance, float cost, float crowFliesDistance, int steps)
    {
        return distance + crowFliesDistance;
    }

    public enum PathResult
    {
        Success,
        FailureNoPath,
        FailureTooManyTries,
        FailureTooFar,
    }

    [System.Serializable]
    public struct Path
    {
        public bool foundPath;
        public PathResult result;
        public List<Node> path;
        public float pathDistance, pathCost, pathCrowFliesDistance;
        public int pathSteps;

        public Path(PathResult result)
        {
            foundPath = false;
            this.result = result;
            path = null;
            pathDistance = 0;
            pathCost = 0;
            pathCrowFliesDistance = 0;
            pathSteps = 0;

            Debug.LogWarning("Path Result: " + result);
        }
    }

    public Path PathFind(Node start, Node end, float maxDistance, int maxTries, CostFunction costFunction)
    {
        if (start == null || end == null)
            return new Path(PathResult.FailureNoPath);

        return PathFind(start.pos.x, start.pos.y, end.pos.x, end.pos.y, maxDistance, maxTries, costFunction);
    }

    public Path PathFind(int startX, int startY, int endX, int endY, float maxDistance, int maxTries)
    {
        return PathFind(startX, startY, endX, endY, maxDistance, maxTries, StandardCostFunction);
    }

    public Path PathFind(int startX, int startY, int endX, int endY, float maxDistance, int maxTries, CostFunction costFunction)
    {
        float d = Distance(startX, startY, endX, endY);

        if (d > maxDistance) 
            return new Path(PathResult.FailureTooFar);

        Node start = GetNode(startX, startY);
        Node end = GetNode(endX, endY);

        if (start == null || end == null)
            return new Path(PathResult.FailureNoPath);

        if (start.IsImpassable || end.IsImpassable)
            return new Path(PathResult.FailureNoPath);

        List<Node> visited = new List<Node>();
        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        start.pathDistance = 0;
        start.pathCrowFliesDistance = d;

        open.Add(start);

        int tries = 0;
        while (true)
        {
            //Debug.Log("Try #" + tries);
            tries++;
            if (tries > maxTries)
            {
                foreach (Node p in visited)
                    p.ClearPathFindingData();

                return new Path(PathResult.FailureTooManyTries);
            }

            Node currentNode = null;

            if (open.Count == 0)
            {
                foreach (Node p in visited)
                    p.ClearPathFindingData();

                return new Path(PathResult.FailureNoPath);
            }

            float currentCost = 0;

            foreach (Node node in open)
            {
                if (currentNode == null)
                {
                    currentNode = node;
                    currentCost = costFunction(currentNode.pathDistance, currentNode.pathCost, currentNode.pathCrowFliesDistance, currentNode.pathSteps);
                }
                else
                {
                    float nodeCost = costFunction(node.pathDistance, node.pathCost, node.pathCrowFliesDistance, node.pathSteps);
                    if (nodeCost < currentCost)
                    {
                        currentCost = nodeCost;
                        currentNode = node;
                    }
                }
            }

            if (currentNode == end)
            {
                break;
            }

            if (currentNode.pathDistance > maxDistance)
            {
                foreach (Node p in visited)
                    p.ClearPathFindingData();

                return new Path(PathResult.FailureTooFar);
            }

            open.Remove(currentNode);
            closed.Add(currentNode);


            for (int i = 0; i < currentNode.neighbours.Length; i++)
            {
                Node neighbour = currentNode.neighbours[i];

                if (neighbour == null) continue;
                if (neighbour.IsImpassable) continue;

                float distance = 1;//i % 2 == 0 ? 1 : ROOT_2;//currentNode.distances[i];

                float nextG = currentNode.pathDistance + distance;

                if (nextG < neighbour.pathDistance)
                {
                    open.Remove(neighbour);
                    closed.Remove(neighbour);
                }

                if (!open.Contains(neighbour) && !closed.Contains(neighbour))
                {
                    neighbour.pathDistance = nextG;
                    neighbour.pathCrowFliesDistance = Distance(neighbour, end);
                    neighbour.pathCost = currentNode.pathCost + neighbour.Cost + GetWallCost(currentNode.pos.x, currentNode.pos.y, i);
                    neighbour.pathSteps = currentNode.pathSteps + 1;
                    neighbour.pathParent = currentNode;
                    open.Add(neighbour);
                    visited.Add(neighbour);
                }
            }
        }

        List<Node> nodes = new List<Node>();
        Node current = end;
        while (current.pathParent != null)
        {
            nodes.Insert(0, current);
 //           nodes.Add(current);
            //this is backwards.

            current = current.pathParent;
        }
        nodes.Insert(0, start);
        //nodes.Add(start);
        //so is this.

        Path result = new Path
        {
            result = PathResult.Success,
            foundPath = true,
            path = nodes,
            pathDistance = end.pathDistance,
            pathCrowFliesDistance = end.pathCrowFliesDistance,
            pathCost = end.pathCost,
            pathSteps = end.pathSteps,
        };

        foreach (Node p in visited)
            p.ClearPathFindingData();

        return result;
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

        if (start != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(start.pos.x + 0.5f, 0, start.pos.y + 0.5f), 0.25f);
        }
        if (end != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(end.pos.x + 0.5f, 0, end.pos.y + 0.5f), 0.25f);
        }

        if (path.foundPath)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < path.path.Count - 1; i++)
            {
                Node n0 = path.path[i];
                Node n1 = path.path[i + 1];
                Gizmos.DrawLine(new Vector3(n0.pos.x + 0.5f, 0, n0.pos.y + 0.5f), new Vector3(n1.pos.x + 0.5f, 0, n1.pos.y + 0.5f));
            }
        }
    }

    public static void DrawTile(int x, int z)
    {
        Gizmos.DrawLine(new Vector3(x, 0, z), new Vector3(x + 1, 0, z));
        Gizmos.DrawLine(new Vector3(x + 1, 0, z), new Vector3(x + 1, 0, z + 1));
        Gizmos.DrawLine(new Vector3(x + 1, 0, z + 1), new Vector3(x, 0, z + 1));
        Gizmos.DrawLine(new Vector3(x, 0, z + 1), new Vector3(x, 0, z));
    }
}
