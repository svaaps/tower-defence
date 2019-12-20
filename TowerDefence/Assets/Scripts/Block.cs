using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public static Color Black = Color.black;
    public static Color Red = Color.red;
    public static Color Green = new Color(.125f, .75f, 0, 1);
    public static Color Blue = new Color(0, .125f, .75f, 1);

    private static Color[] Colors =
    {
        Black,
        Red,
        Green,
        Blue
    };

    public static Color GetColor(int index) => index < 0 || index >= Colors.Length ? Colors[0] : Colors[index];

    [SerializeField]
    private int color;

    [SerializeField]
    private int life;
    
    [SerializeField]
    private int pathMaxTries;

    [SerializeField]
    private float pathMaxDistance;

    private float despawnTimer;

    public Color Color => GetColor(color);
    public PathFinding.Path Path { get; set; }
    public int Life { get => life; set => life = value; }
    public Node Target { get; private set; }
    public Node Current { get; private set; }

    public Vector3 axis { get; set; }
    public Vector3 pivot { get; set; }
    public int direction { get; set; }
    public int nextX { get; set; }
    public int nextY { get; set; }
    public int prevX { get; set; }
    public int prevY { get; set; }
    public bool moving { get; set; }
    public float angle { get; set; }
    public bool moved { get; set; }

    public void Awake()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        Color color = Color;
        rend.material.SetColor("_EmissionColor", color * 2);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            Target = Map.Instance.MouseNode;

        if (Current == null)
        {
            despawnTimer += Time.deltaTime;
            if (despawnTimer > 5)
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            despawnTimer = 0;
        }
    }

    private static int DirectionFrom(Node n0, Node n1)
    {
        if (n0 == null || n1 == null)
            return 0;

        if (n0.pos.x == n1.pos.x && n0.pos.y + 1 == n1.pos.y)
            return 0;

        if (n0.pos.x + 1 == n1.pos.x && n0.pos.y == n1.pos.y)
            return 1;

        if (n0.pos.x == n1.pos.x && n0.pos.y - 1 == n1.pos.y)
            return 2;

        if (n0.pos.x - 1 == n1.pos.x && n0.pos.y == n1.pos.y)
            return 3;

        return 0;
    }

    public static float SnapAngle(float euler)
    {
        return Mathf.Round(euler / 90) * 90;
    }

    public void SetPosition(Node node)
    {
        Current = node;
        angle = 0;
    }

    public void GoToNext()
    {
        Node next = Map.Instance.GetNode(nextX, nextY);
        if (next.block != null)
            return;

        prevX = Current.pos.x;
        prevY = Current.pos.y;

        transform.position = new Vector3(prevX + 0.5f, 0.5f, prevY + 0.5f);

        transform.localRotation = Quaternion.Euler
        (
            SnapAngle(transform.localRotation.eulerAngles.x),
            SnapAngle(transform.localRotation.eulerAngles.y),
            SnapAngle(transform.localRotation.eulerAngles.z)
        );

        moved = true;
        Current.block = null;
        next.block = this;
        SetPosition(next);
    }

    public virtual void RecalculatePath()
    {
        //Target = Map.Instance.GetNode(7, 15);//TODO
        if (Map.Instance.NearestBlockGoal(transform.position, out BlockGoal goal))
        {
            Target = goal.node;
        }

        int x = Current.pos.x;
        int y = Current.pos.y;

        Path = PathFinding.PathFind(Current, Target, pathMaxDistance, pathMaxTries, CostFunction());

        if (!Path.FoundPath || Path.Nodes.Count <= 1)
        {
            moving = false;
            return;
        }

        Node n1 = Path.Nodes[1];
        
        if (n1.block)
        {
            moving = false;
            return;
        }

        moving = true;

        direction = DirectionFrom(Current, n1);
        nextX = x;
        nextY = y;

        if (direction == 0)
        {
            nextY++;
            pivot = new Vector3(x + 0.5f, 0, y + 1f);
            axis = new Vector3(1, 0, 0);
        }
        else if (direction == 1)
        {
            nextX++;
            pivot = new Vector3(x + 1, 0, y + 0.5f);
            axis = new Vector3(0, 0, 1);
        }
        else if (direction == 2)
        {
            nextY--;
            pivot = new Vector3(x + 0.5f, 0, y);
            axis = new Vector3(1, 0, 0);
        }
        else if (direction == 3)
        {
            nextX--;
            pivot = new Vector3(x, 0, y + 0.5f);
            axis = new Vector3(0, 0, 1);
        }
    }

    public void OnDrawGizmos()
    {
        PathFinding.DrawPath(Path);
    }

    public virtual PathFinding.CostFunction CostFunction()
    {
        return PathFinding.StandardCostFunction;
    }

    public void Init(int x, int y)
    {
        transform.position = new Vector3(x + 0.5f, 0, y + 0.5f);
        Current = Map.Instance.GetNode(x, y);
        prevX = x;
        prevY = y;
    }

    public void AnimateTo(int x, int y, int direction)
    {
        int targetX = x;
        int targetY = y;
        Vector3 pivot = new Vector3();
        Vector3 axis = new Vector3();

        if (direction == 0)
        {
            targetY++;
            pivot = new Vector3(x + 0.5f, 0, y + 1f);
            axis = new Vector3(1, 0, 0);
        }
        else if (direction == 1)
        {
            targetX++;
            pivot = new Vector3(x + 1, 0, y + 0.5f);
            axis = new Vector3(0, 0, 1);
        }
        else if (direction == 2)
        {
            targetY--;
            pivot = new Vector3(x + 0.5f, 0, y);
            axis = new Vector3(1, 0, 0);
        }
        else if (direction == 3)
        {
            targetX--;
            pivot = new Vector3(x, 0, y + 0.5f);
            axis = new Vector3(0, 0, 1);
        }


        StartCoroutine(AnimateTo(pivot, axis, 90, 1));

    }

    public IEnumerator AnimateTo(Vector3 pivot, Vector3 axis, float angle, float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            
            transform.RotateAround(pivot, axis, angle * t / duration);
            yield return null;
        }
    }
}
