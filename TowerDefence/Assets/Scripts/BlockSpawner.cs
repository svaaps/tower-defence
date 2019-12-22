﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : Structure
{
    [SerializeField]
    private int spawnInterval;
    
    [SerializeField]
    private bool spawnLoop;

    [SerializeField]
    private Block[] spawnQueue;

    private int spawnCounter;
    private List<Block> queue = new List<Block>();

    private Renderer rend;

    private PathFinding.Path path;

    public void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    public override void OnPlace()
    {
        RecalculatePath();
        FillQueue();
        if (queue.Count > 0)
        {
            SetColor(queue[0].Color * 3);
        }
    }

    public void RecalculatePath()
    {
        Map.Instance.NearestBlockGoal(node, out _, out path);
    }

    public void SetColor(Color color)
    {
        rend.material.SetColor("_EmissionColor", color);
    }

    private void FillQueue()
    {
        foreach (Block block in spawnQueue)
            if (block != null) queue.Add(block);
    }

    public override void Tick()
    {
        spawnCounter++;

        if (Map.Instance.Changed)
            RecalculatePath();

        if (spawnCounter >= spawnInterval)
        {
            spawnCounter = 0;
            SpawnNext();
        }
    }

    public void OnDrawGizmosSelected()
    {
        PathFinding.DrawPath(path);
    }

    private void SpawnNext()
    {
        if (queue.Count > 0 && Map.Instance.AddBlock(queue[0], node.pos.x, node.pos.y, out Block block))
        {
            block.Path = new PathFinding.Path(path);
            queue.RemoveAt(0);
        }
        else
        {
          //  Debug.Log("Failing to add a block " + queue.Count + " " + node.block + " " + node.block.moved + " " + node.block.updated);
        }
        if (queue.Count <= 0)
        {
            if (spawnLoop)
            {
                FillQueue();
            }
            else
            {
                SetColor(Block.Black);
                return;
            }
        }

        if (queue.Count > 0)
        {
            SetColor(queue[0].Color * 3);
        }
    }
}
