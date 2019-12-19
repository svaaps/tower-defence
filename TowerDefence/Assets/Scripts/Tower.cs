using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : Structure
{
    [SerializeField]
    private Transform projectileStart;

    [SerializeField]
    private Projectile projectilePrefab;

    [SerializeField]
    private float fireRate;

    private float fireCounter;

    private Mob target;

    [SerializeField]
    private float range;

    public void Update()
    {
        if (target != null && Map.SquareDistance(transform.position, target.transform.position) > range * range)
            target = null;


        if (target == null)
            target = Map.Instance.ClosestMob(transform.position);

        if (target != null)
        {
            //Rotate to face the target.
        }

        if (fireRate <= 0)
            return;

        float fireInterval = 1f / fireRate;
        fireCounter += Time.deltaTime;

        while(fireCounter >= fireInterval && target != null)
        {
            fireCounter -= fireInterval;
            if (target != null)
                Fire(target.transform.position);
        }
    }

    public void Fire(Vector3 position)
    {
        Projectile projectile = Instantiate(projectilePrefab, projectileStart.position, projectileStart.rotation);
        projectile.Fire(position);
    }
}
