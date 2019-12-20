using System.Collections;
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


    public void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        FillQueue();
        if (queue.Count > 0)
        {
            SetColor(queue[0].Color * 3);
        }
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

        if (spawnCounter >= spawnInterval)
        {
            spawnCounter = 0;
            SpawnNext();
        }
    }

    private void SpawnNext()
    {
        if (node.block != null)
        {
            return;
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

        if (Map.Instance.AddBlock(queue[0], node.pos.x, node.pos.y))
        {
            queue.RemoveAt(0);
        }
        if (queue.Count > 0)
        {
            SetColor(queue[0].Color * 3);
        }
    }
}
