using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobPathNode : Node
{
    public Tile parentTile;

    public MobPathNode(float x, float y) : base(x, y)
    {
    }

    public override void OnDrawGizmosSelected()
    {
        Vector3 half = new Vector3(0.5f, 0, 0.5f);

        Vector3 position = new Vector3(x, 0, y);

        Gizmos.DrawSphere(position, 0.1f);
        foreach (Node n in neighbours)
            if (n != null)
                Gizmos.DrawLine(position + half, new Vector3(n.x, 0, n.y) + half);
    }
}
