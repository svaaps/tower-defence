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
        //The Block automatically despawns if either it is not on a Tile, or the Block the Tile thinks it ought to have on it is not this...
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
        //This method is called in the tick method and moves the block along its path by removing references to the current tile, and assigning it a new tile which is the next one along the path.
        //It doesn't actually control the animation of the block between tile positions. 

        //Do nothing if it's flagged not to move
        if (!moving)
            return;

        //Do nothing if it has no path 
        if (!Path.FoundPath)
            return;

        //Do nothing if the path is only one or zero nodes in length
        if (Path.Nodes.Count <= 1)
            return;

        //Do nothing if the next path node is (for some unlikely reason) not a Tile or is null
        Tile next = Path.Nodes[1] as Tile;
        if (next == null)
            return;

        //Do nothing if the next path node already has a block on it.
        if (next.block != null)
            return;


        //Record the current position and store as integers in prevX and prevY
        prevX = Current.IntX;
        prevY = Current.IntY;

        //Position the block exactly on the current tile (even if it was already)
        transform.position = new Vector3(prevX + 0.5f, 0, prevY + 0.5f);

        //Remove the reference to this block from the current tile. Freeing the tile up for other blocks to move onto it.
        Current.block = null;

        //Set the next node along the path as the current one for this block.
        Current = next;

        //Set the block for that node to this one.
        Current.block = this;

        //Remove the first node in the path.
        Path.Nodes.RemoveAt(0);

        //Declare the block moved
        moved = true;
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
