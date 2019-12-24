using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int pos;
    [HideInInspector]
    public Node[] neighbours;
    [HideInInspector]
    public float pathDistance, pathCost, pathCrowFliesDistance;
    [HideInInspector]
    public int pathSteps, pathTurns, pathEndDirection;
    [HideInInspector]
    public Node pathParent;

    public virtual float Cost() => 0;
    public virtual bool IsImpassable() => false;
    public virtual bool ExitBlocked(int direction) => false;

    public void ClearPathFindingData()
    {
        pathParent = null;
        pathDistance = 0;
        pathCost = 0;
        pathCrowFliesDistance = 0;
        pathSteps = 0;
        pathTurns = 0;
        pathEndDirection = 0;
    }

    public void Init(int x, int y)
    {
        pos = new Vector2Int(x, y);
        transform.localPosition = new Vector3(x, 0, y);
        neighbours = new Node[8];
    }

    public Node NorthNeighbour
    {
        get => neighbours[0];
        set => neighbours[0] = value;
    }

    public Node NorthEastNeighbour
    {
        get => neighbours[1];
        set => neighbours[1] = value;
    }

    public Node EastNeighbour
    {
        get => neighbours[2];
        set => neighbours[2] = value;
    }
    public Node SouthEastNeighbour
    {
        get => neighbours[3];
        set => neighbours[3] = value;
    }
    public Node SouthNeighbour
    {
        get => neighbours[4];
        set => neighbours[4] = value;
    }
    public Node SouthWestNeighbour
    {
        get => neighbours[5];
        set => neighbours[5] = value;
    }

    public Node WestNeighbour
    {
        get => neighbours[6];
        set => neighbours[6] = value;
    }
    public Node NorthWestNeighbour
    {
        get => neighbours[7];
        set => neighbours[7] = value;
    }

    public void DetachNeighbours()
    {
        if (NorthNeighbour != null)
        {
            NorthNeighbour.SouthNeighbour = null;
            NorthNeighbour = null;
        }

        if (NorthEastNeighbour != null)
        {
            NorthEastNeighbour.SouthWestNeighbour = null;
            NorthEastNeighbour = null;
        }

        if (EastNeighbour != null)
        {
            EastNeighbour.WestNeighbour = null;
            EastNeighbour = null;
        }

        if (SouthEastNeighbour != null)
        {
            SouthEastNeighbour.NorthWestNeighbour = null;
            SouthEastNeighbour = null;
        }

        if (SouthNeighbour != null)
        {
            SouthNeighbour.NorthNeighbour = null;
            SouthNeighbour = null;
        }

        if (SouthWestNeighbour != null)
        {
            SouthWestNeighbour.NorthEastNeighbour = null;
            SouthWestNeighbour = null;
        }

        if (WestNeighbour != null)
        {
            WestNeighbour.EastNeighbour = null;
            WestNeighbour = null;
        }

        if (NorthWestNeighbour != null)
        {
            NorthWestNeighbour.SouthEastNeighbour = null;
            NorthWestNeighbour = null;
        }
    }

    public void Tick()
    {

    }

    public virtual void OnDrawGizmosSelected()
    {
        Vector3 half = new Vector3(0.5f, 0, 0.5f);

        Gizmos.DrawSphere(transform.position, 0.1f);
        foreach (Node n in neighbours)
            if (n) Gizmos.DrawLine(transform.position + half, n.transform.position + half);
    }
}
