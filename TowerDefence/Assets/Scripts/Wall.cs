using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public bool vertical, facesOutward;
    public Vector2Int pos;
    public float cost;

    public bool canBeBuiltOver;
    public bool isRemovable;
    public bool isObstacle;
}
