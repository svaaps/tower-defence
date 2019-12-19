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
    private float minimumRigidbodyVelocity = 2;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        setDestinationCounter = Random.value;
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

            float setDestinationInterval = 1f;
            float minimumDestinationDistanceSq = 0.1f;

            if (setDestinationCounter >= setDestinationInterval && Map.Instance.NearestBlock(transform.position, out nearestBlock))
            {
                if (!agent.hasPath || Map.SquareDistance(nearestBlock.transform.position, agent.destination) > minimumDestinationDistanceSq)
                {
                    agent.SetDestination(nearestBlock.transform.position);
                    setDestinationCounter = 0;
                }
                else
                {
                    agent.ResetPath();
                }
            }
        }
        else
        {
            agent.enabled = false;
        }
    }

    public void AddForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void OnDeath()
    {
        Game.Instance.RemoveMob(this);
    }
}
