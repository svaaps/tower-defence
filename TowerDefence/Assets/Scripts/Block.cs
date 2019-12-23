using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public static Color Black = Color.black;
    public static Color Red = new Color(.75f, .0625f, .0625f, 1);
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
    private float life;

    [SerializeField]
    private bool isInvincible;

    private bool dead;

    private float despawnTimer;
    public Color Color => GetColor(color);
    public PathFinding.Path Path { get; set; }
    public float Life { get => life; set => life = value; }
    public Tile Target { get; private set; }
    public Tile Current { get; private set; }

    public int prevX;
    public int prevY;

    public bool updated;
    public bool moving;
    public bool moved;
    public bool waiting;
    public bool needsRepath;

    public void Awake()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        Color color = Color;
        //rend.material.SetColor("_EmissionColor", color * 2);
    }

    public void Init(int x, int y)
    {
        transform.position = new Vector3(x + 0.5f, 0, y + 0.5f);
        Current = Map.Instance.GetTile(x, y);
        prevX = x;
        prevY = y;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            Target = Map.Instance.MouseTile;

        if (Current == null || Current.block != this)
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

    public void GoToNext()
    {
        if (!moving)
            return;

        if (!Path.FoundPath)
            return;

        if (Path.Nodes.Count < 2)
            return;

        Tile next = Path.Nodes[1] as Tile;
        if (next == null)
            return;

        if (next.block != null)
            return;

        prevX = Current.pos.x;
        prevY = Current.pos.y;

        transform.position = new Vector3(prevX + 0.5f, 0, prevY + 0.5f);

        moved = true;
        Current.block = null;
        Current = next;
        Current.block = this;
        Path.Nodes.RemoveAt(0);
    }

    public virtual void RecalculatePath()
    {
        if (Map.Instance.NearestBlockGoal(Current, out BlockGoal goal, out PathFinding.Path path))
        {
            Path = path;
            Target = goal.tile;
        }
    }

    public void OnDrawGizmosSelected()
    {
        PathFinding.DrawPath(Path);
    }

    public virtual PathFinding.CostFunction CostFunction()
    {
        return PathFinding.StandardCostFunction;
    }

    public virtual void AddDamage(float amount)
    {
        if (isInvincible)
            return;

        if (dead)
            return;

        life -= amount;
        if (life <= 0)
        {
            OnDeath();
            Map.Instance.RemoveBlock(this);
        }
    }
    public virtual void OnDeath()
    {

    }
}
