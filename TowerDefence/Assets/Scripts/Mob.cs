using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mob : MonoBehaviour
{
    private NavMeshAgent agent;

    private Block nearestBlock;

    private Rigidbody rb;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Update()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (transform.position.y < -5)
        {
            OnDeath();
            Destroy(gameObject);
            return;
        }

        float magnitude = rb.velocity.magnitude;
        if (magnitude < 1)
        {
            rb.velocity = Vector3.zero;
            agent.enabled = true;
            if (Map.Instance.NearestBlock(transform.position, out nearestBlock))
                agent.SetDestination(nearestBlock.transform.position);
            else
                agent.ResetPath();
        }
        else
        {
            agent.enabled = false;
        }
    }

    public void AddForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
        agent.enabled = false;
    }

    public void OnDeath()
    {
        Game.Instance.RemoveMob(this);
    }
}
