using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Structure : MonoBehaviour
{
    public int rotation;
    public float cost;
    public bool canBeBuiltOver;
    public bool isRemovable;
    public bool isObstacle;
    public bool isImpassable;
    public int height = 1;
    public Texture2D thumbnail;
}
