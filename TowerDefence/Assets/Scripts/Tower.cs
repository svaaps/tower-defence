﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : Structure
{
    [SerializeField]
    private Transform projectileStart;

    [SerializeField]
    private Projectile projectilePrefab;

    [SerializeField]
    private int fireInterval;

    private int fireCounter;

    private Mob target;

    [SerializeField]
    private float range;

    public override void Tick()
    {
        /*
        if (target != null && Map.SquareDistance(transform.position, target.transform.position) > range * range)
            target = null;

        if (target == null)
        {
            target = Game.Instance.ClosestMob(transform.position);
        }

        */

        target = Game.Instance.ClosestMob(transform.position);

        if (target != null)
        {
            fireCounter++;
            if (fireCounter >= fireInterval)
            {
                fireCounter = 0;
                if (target != null)
                    Fire(target.transform.position);
            }
        }
    }

    public void Fire(Vector3 position)
    {
        Projectile projectile = Instantiate(projectilePrefab, projectileStart.position, projectileStart.rotation);
        if (!projectile.Fire(position))
            Destroy(projectile.gameObject);
    }
}
