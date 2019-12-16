using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    private Vector2Int size;

    private Wall[,] verticalWalls;
    private Wall[,] horizontalWalls;
    private Node[,] nodes;

    [SerializeField]
    private Node emptyNodePrefab;

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
        nodes[x, y].structure.transform.localPosition = Vector3.zero;
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

    public bool DeleteStructure(int x, int y, int rotation, bool sudo = false)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
            return false;

        if (!nodes[x, y].structure)
            return false;

        if (!sudo && !nodes[x, y].structure.isRemovable)
            return false;

        Destroy(nodes[x, y].structure.gameObject);
        nodes[x, y] = null;
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

    public enum PathResult
    {
        Success,
        FailureNoPath,
        FailureTooManyTries,
        FailureTooFar
    }

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

    public Path PathFind(int startX, int startY, int endX, int endY, float maxDistance, int maxTries)
    {
        return PathFind(startX, startY, endX, endY, maxDistance, maxTries, StandardCostFunction);
    }

    public Path PathFind(int startX, int startY, int endX, int endY, float maxDistance, int maxTries, CostFunction costFunction)
    {
        float d = Distance(startX, startY, endX, endY);
        if (d > maxDistance) return new Path();

        List<Node> visited = new List<Node>();
        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        Node start = GetNode(startX, startY);
        Node end = GetNode(endX, endY);



        start.pathDistance = 0;
        start.pathCrowFliesDistance = d;

        open.Add(start);

        int tries = 0;
        while (true)
        {
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
            nodes.Add(current);
            //this is backwards.

            current = current.pathParent;
        }
        nodes.Add(start);
        //so is this.

        Path result = new Path
        {
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

        return GetNode(Mathf.FloorToInt(intersection.x), Mathf.FloorToInt(intersection.y));
    }

    public static Vector3 ScreenPointToRayPlaneIntersection(Vector3 screenPos, float y, Camera camera)
    {
        Vector3 hit = Vector3.zero;
        Ray ray = camera.ScreenPointToRay(screenPos);
        if (new Plane(Vector3.up, new Vector3(0, y, 0)).Raycast(ray, out float distance))
            hit = ray.GetPoint(distance);
        return hit;
    }

}
