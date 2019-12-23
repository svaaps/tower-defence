using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mob : MonoBehaviour
{
    private NavMeshAgent agent;

    private Base nearestBase;
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

    [SerializeField]
    private float attackRange;

    [SerializeField]
    private float attackInterval;

    [SerializeField]
    private float attackDamage;

    private float attackCounter;

    [SerializeField]
    private float blockAggroRange;


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
        attackCounter += Time.deltaTime;

        if (nearestBlock)
        {
            float squareDistance = Map.SquareDistance(nearestBlock.transform.position, transform.position);
            float range = attackRange * attackRange;

            if (squareDistance < range)
            {
                agent.enabled = false;

                if (attackCounter >= attackInterval)
                {
                    attackCounter = 0;
                    nearestBlock.AddDamage(attackDamage);
                    OnAttack();
                }

                return;
            }
        }

        else if (nearestBase)
        {
            float squareDistance = Map.SquareDistance(nearestBase.transform.position, transform.position);
            float range = attackRange * attackRange;

            if (squareDistance < range)
            {
                agent.enabled = false;

                if (attackCounter >= attackInterval)
                {
                    attackCounter = 0;
                    nearestBase.AddDamage(attackDamage);
                    OnAttack();
                }

                return;
            }
        }

        float magnitude = rb.velocity.magnitude;
        if (magnitude < minimumRigidbodyVelocity)
        {
            //rb.velocity = Vector3.zero;
            agent.enabled = true;

            if (setDestinationCounter >= updatePathInterval)
            {
                if (Map.Instance.NearestBlock(transform.position, blockAggroRange, out nearestBlock))
                {
                    if (!agent.hasPath || Map.SquareDistance(nearestBlock.transform.position, agent.destination) > updatePathMinimumDistanceSquared)
                    {
                        agent.SetDestination(nearestBlock.transform.position);
                        setDestinationCounter = 0;
                    }
                }
                else if (Map.Instance.NearestStructure(transform.position, out nearestBase))
                {
                    if (!agent.hasPath || Map.SquareDistance(nearestBase.transform.position, agent.destination) > updatePathMinimumDistanceSquared)
                    {
                        agent.SetDestination(nearestBase.transform.position);
                        setDestinationCounter = 0;
                    }
                }
            }
        }
        else
        {
           // agent.enabled = false;
        }
    }

    public virtual void OnAttack()
    {

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
