using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : Node
{
    [HideInInspector]
    public Structure structure;
    [HideInInspector]
    public Block block;

    public override float Cost() => structure == null ? 0 : structure.cost;
    public override bool IsImpassable() => structure != null && structure.isImpassable;
    public override bool ExitBlocked(int direction) => structure ? structure.ExitBlocked(direction) : false;
}
