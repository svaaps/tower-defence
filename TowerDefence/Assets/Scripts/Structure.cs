using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Structure : MonoBehaviour
{
    public float life;
    public float cost;
    public bool canBeBuiltOver;
    public bool isRemovable;
    public bool isObstacle;
    public bool isImpassable;
    public bool isInvincible;
    public bool northExitBlocked;
    public bool eastExitBlocked;
    public bool southExitBlocked;
    public bool westExitBlocked;
    public int height = 1;
    private bool dead;
    [HideInInspector]
    public bool placed;
    [HideInInspector]
    public Texture2D thumbnail;
    [HideInInspector]
    public Tile tile;
    private int rotation;

    public bool ExitBlocked(int direction)
    {
        direction /= 2;
        direction -= rotation;
        direction %= 4;
        direction += 4;
        direction %= 4;

        if (direction == 0)
            return northExitBlocked;

        if (direction == 1)
            return eastExitBlocked;

        if (direction == 2)
            return southExitBlocked;

        if (direction == 3)
            return westExitBlocked;

        return false;
    }
    public int Rotation
    {
        get => rotation;
        set
        {
            rotation = value;
            transform.localRotation = Quaternion.Euler(0, 90 * value, 0);
        }
    }

    public virtual void OnPlace() { }
    public virtual void OnDeath() { }
    public virtual void BuildModeUpdate(bool mapChanged) { }
    public virtual void EarlyTick() { }
    public virtual void Tick() { }
    public virtual void LateTick() { }
    public virtual void InterTick(float t) { }

    public void AddDamage(float amount)
    {
        if (isInvincible)
            return;

        if (dead)
            return;

        life -= amount;
    
        if (life <= 0)
        {
            dead = true;
            OnDeath();
            Map.Instance.DeleteStructure(tile.IntX, tile.IntY, true);
        }
    }
}
