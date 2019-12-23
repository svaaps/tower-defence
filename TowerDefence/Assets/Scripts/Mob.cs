using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mob : MonoBehaviour
{
    private NavMeshAgent agent;

    private Block nearestBlock;

    private Rigidbody rb;

    private float setDestinationCounter;

    [SerializeField]
    private float life;
    private bool dead;

    [SerializeField]
    private float minimumRigidbodyVelocity = 2;

    [SerializeField]
    private float updatePathInterval = 0.1f;

    [SerializeField]
    private float updatePathMinimumDistanceSquared = 0.5f;


    private Cubes cubes;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        setDestinationCounter = Random.value;
        cubes = GetComponentInChildren<Cubes>();
    }

    public void Update()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        
        setDestinationCounter += Time.deltaTime;

        if (transform.position.y < -5)
        {
            OnDeath();
            Destroy(gameObject);
            return;
        }
        
        float magnitude = rb.velocity.magnitude;
        if (magnitude < minimumRigidbodyVelocity)
        {
            //rb.velocity = Vector3.zero;
            agent.enabled = true;

            if (setDestinationCounter >= updatePathInterval && Map.Instance.NearestBlock(transform.position, out nearestBlock))
            {
                if (!agent.hasPath || Map.SquareDistance(nearestBlock.transform.position, agent.destination) > updatePathMinimumDistanceSquared)
                {
                    agent.SetDestination(nearestBlock.transform.position);
                    setDestinationCounter = 0;
                }
            }
        }
        else
        {
            agent.enabled = false;
        }
    }

    public void AddDamage(float damage)
    {
        life -= damage;
        if (life <= 0 && !dead)
        {
            OnDeath();
            Destroy(gameObject);
        }
    }

    public void AddForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void OnDeath()
    {
        Game.Instance.RemoveMob(this);
        cubes.Explode();
        AudioManager.Instance.PlayWithRandomPitch("Death", Random.value * .25f);
    }
}
