using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : Structure
{
    [SerializeField]
    private int spawnInterval;

    [SerializeField]
    private bool loop;

    [SerializeField]
    private Transform spawnPosition;

    private int spawnCounter;

    [System.Serializable]
    public class Burst
    {
        public Mob[] prefabs;
    }

    [SerializeField]
    private Burst[] bursts;

    private int burst;

    public override void Tick()
    {
        spawnCounter++;

        if (burst >= bursts.Length)
        {
            if (loop)
                burst = 0;
            else
                return;
        }

        if (spawnCounter >= spawnInterval)
        {
            spawnCounter = 0;
            SpawnNext();
        }
    }

    private void SpawnNext()
    {
        foreach(Mob prefab in bursts[burst].prefabs)
            Game.Instance.AddMob(prefab, spawnPosition.position);
        burst++;
    }
}
