using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGoal : Structure
{
    private bool sunk;
    private Block sinkingBlock;
    public override void EarlyTick()
    {
        if (sinkingBlock != null && sunk)
        {
            Destroy(sinkingBlock.gameObject);
            sinkingBlock = null;
            sunk = false;
        }

        if (node.block != null && sinkingBlock == null)
        {
            sinkingBlock = node.block;
            node.block = null;
        }
    }

    public override void InterTick(float t)
    {
        if (sinkingBlock != null)
        {
            sinkingBlock.transform.position = Vector3.Lerp(transform.position, transform.position - new Vector3(0, 1, 0), t);
            sunk = true;
        }
    }
}
