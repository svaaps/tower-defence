using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int pos;
    //[HideInInspector]
    public Node[] neighbours = new Node[4];

    [HideInInspector]
    public Structure structure;

    [HideInInspector]
    public float pathDistance, pathCost, pathCrowFliesDistance;
    [HideInInspector]
    public int pathSteps;
    [HideInInspector]
    public Node pathParent;

    public Mob mob;

    public float Cost => structure == null ? 0 : structure.cost;

    public bool IsImpassable => structure != null && structure.isImpassable;

    public void ClearPathFindingData()
    {
        pathParent = null;
        pathDistance = 0;
        pathCost = 0;
        pathCrowFliesDistance = 0;
        pathSteps = 0;
    }

    public void Init(int x, int y)
    {
        pos = new Vector2Int(x, y);
        transform.localPosition = new Vector3(x, 0, y);
        neighbours = new Node[4];
    }

    public Node NorthNeighbour
    {
        get => neighbours[0];
        set => neighbours[0] = value;
    }

    public Node EastNeighbour
    {
        get => neighbours[1];
        set => neighbours[1] = value;
    }

    public Node SouthNeighbour
    {
        get => neighbours[2];
        set => neighbours[2] = value;
    }

    public Node WestNeighbour
    {
        get => neighbours[3];
        set => neighbours[3] = value;
    }

    public void DetachNeighbours()
    {
        if (NorthNeighbour != null)
        {
            NorthNeighbour.SouthNeighbour = null;
            NorthNeighbour = null;
        }

        if (EastNeighbour != null)
        {
            EastNeighbour.WestNeighbour = null;
            EastNeighbour = null;
        }

        if (SouthNeighbour != null)
        {
            SouthNeighbour.NorthNeighbour = null;
            SouthNeighbour = null;
        }

        if (WestNeighbour != null)
        {
            WestNeighbour.EastNeighbour = null;
            WestNeighbour = null;
        }
    }

    public void Tick()
    {

    }
}
