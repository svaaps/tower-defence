using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : Structure
{
    [SerializeField]
    private int spawnInterval;

    [SerializeField]
    private Block[] spawnQueue;

    private int spawnCounter;
    private List<Block> queue = new List<Block>();

    public void Awake()
    {
        FillQueue();
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
            return;

        if (queue.Count <= 0)
            return;

        if (Map.Instance.AddBlock(queue[0], node.pos.x, node.pos.y))
            queue.RemoveAt(0);
    }
}
