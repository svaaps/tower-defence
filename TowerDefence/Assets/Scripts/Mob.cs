using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mob : MonoBehaviour
{
    [SerializeField]
    private int life;

    public Map.Path Path { get; set; }
    public int Life { get => life; set => life = value; }
    public Node Target { get; private set; }

    public Vector3 axis, pivot;
    public int x, y, direction;
    public int nextX, nextY;
    public bool moving;
    public float angle;
    public bool moved;

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

    public virtual void DetermineTarget()
    {
        Target = Map.Instance.GetNode(12, 12);//TODO
        angle = 0;

        transform.localRotation = Quaternion.Euler
        (
            SnapAngle(transform.localRotation.eulerAngles.x),
            SnapAngle(transform.localRotation.eulerAngles.y),
            SnapAngle(transform.localRotation.eulerAngles.z)
        );

        

        Node n0 = Map.Instance.GetNode(x, y);

        Path = Map.Instance.PathFind(n0, Target, Map.Instance.pathMaxDistance, Map.Instance.pathMaxTries, CostFunction());

        if (!Path.foundPath || Path.path.Count <= 1)
        {
            moving = false;
            return;
        }

        Node n1 = Path.path[1];

        moving = true;

        if (n1.mob)
        {
            moving = false;
        }

        direction = DirectionFrom(n0, n1);
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


    public virtual Map.CostFunction CostFunction()
    {
        return Map.StandardCostFunction;
    }

    public void Init(int x, int y)
    {
        transform.localPosition = new Vector3(x + 0.5f, 0.5f, y + 0.5f);
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
