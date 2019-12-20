using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private static Game instance;
    public static Game Instance => instance;

    [SerializeField]
    private float tickInterval;

    private float counter;

    [SerializeField]
    private Mob mobPrefab;

    private List<Mob> mobs = new List<Mob>();

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        for (int i = 0; i < 10; i++)
            AddMob(mobPrefab, new Vector3(Random.Range(0, Map.Instance.Size.x), 0, Random.Range(0, Map.Instance.Size.y)));
    }

    public void FixedUpdate()
    {
        counter += Time.deltaTime;
        while(counter >= tickInterval)
        {
            counter -= tickInterval;
            Map.Instance.Tick();
        }
        Map.Instance.InterTick(counter / tickInterval);
    }

    public Mob ClosestMob(Vector3 position)
    {
        Mob closest = null;
        float sqDistance = float.MaxValue;
        foreach (Mob mob in mobs)
        {
            float d = Map.SquareDistance(position, mob.transform.position);
            if (closest == null || sqDistance > d)
            {
                closest = mob;
                sqDistance = d;
            }
        }
        return closest;
    }

    public List<Mob> MobsInRange(Vector3 position, float range)
    {
        List<Mob> inRange = new List<Mob>();

        foreach (Mob mob in mobs)
            if (Map.SquareDistance(position, mob.transform.position) <= range * range)
                inRange.Add(mob);

        return inRange;
    }

    public void AddForce(Vector3 position, float force, float range)
    {
        foreach (Mob mob in MobsInRange(position, range))
        {
            Vector3 delta = mob.transform.position - position;
            float distance = delta.magnitude;
            delta /= distance;
            float rangeMultiplier = 1f - distance / range;
            mob.AddForce(delta * force * rangeMultiplier);
        }
    }

    public void AddMob(Mob prefab, Vector3 position)
    {
        mobs.Add(Instantiate(prefab, position, Quaternion.identity));
    }

    public void RemoveMob(Mob instance)
    {
        mobs.Remove(instance);
    }
}
