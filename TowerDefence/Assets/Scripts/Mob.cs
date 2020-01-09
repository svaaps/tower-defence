using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mob : MonoBehaviour
{
    private NavMeshAgent agent;

    private Base targetBase;
    private Block targetBlock;

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

        //Advance the counter timing the interval between repathing.
        setDestinationCounter += Time.deltaTime;


        //If the mob is below -5 (arbitrary value) then it is destroyed. Used to remove mobs that are knocked out of the game area. 
        if (transform.position.y < -5)
        {
            OnDeath();
            Destroy(gameObject);
            return;
        }

        //Advance the counter timing the interval between attacks.
        attackCounter += Time.deltaTime;


        //IF the mob has a target block
        if (targetBlock)
        {

            //Find the square distance from the mob to the target block.
            float squareDistance = Map.SquareDistance(targetBlock.transform.position, transform.position);

            //Square the attack range
            float range = attackRange * attackRange;

            //IF the block is within the attack range
            if (squareDistance < range)
            {
                //Stop moving (disable the NavMeshAgent)
                agent.enabled = false;

                //If the attack counter is beyond the attack interval, i.e. if the mob is ready to attack
                if (attackCounter >= attackInterval)
                {
                    //Reset the attack counter
                    attackCounter = 0;

                    //Add damage to the target block
                    targetBlock.AddDamage(attackDamage);

                    //Trigger an OnAttack call
                    OnAttack();
                }

                //FINALLY RETURN, i.e. don't execute the code below which finds targets
                return;
            }
        }

        //ELSE IF the mob has a target base
        else if (targetBase)
        {
            //Find the square distance from the mob to the target base.
            float squareDistance = Map.SquareDistance(targetBase.transform.position, transform.position);

            //Square the attack range
            float range = attackRange * attackRange;

            //IF the base is within attack range
            if (squareDistance < range)
            {
                //Stop moving
                agent.enabled = false;

                //If the attack counter is beyond the attack interval
                if (attackCounter >= attackInterval)
                {
                    //Reset the attack counter
                    attackCounter = 0;

                    //Add damage to the target base
                    targetBase.AddDamage(attackDamage);

                    //Trigger and OnAttack call
                    OnAttack();
                }

                //FINALLY RETURN
                return;
            }
        }


        //Get the current linear velocity of the mob by getting the magnitude of the 3D velocity vector from the Rigidbody
        float velocity = rb.velocity.magnitude;

        //IF the mob is not being controlled by the NavMeshAgent (i.e. was probably being thrown through the air or falling) and is now moving slower than a defined small minimum (as good as stopped)
        if (!agent.enabled && velocity < minimumRigidbodyVelocity)
        {
            //Reenable the NavMeshAgent
            agent.enabled = true;
        }

        //IF the NavMeshAgent is enabled
        if (agent.enabled)
        {
            //IF the counter is beyond a defined interval
            if (setDestinationCounter >= updatePathInterval)
            {
                //Get new targets.

                //First check for blocks.
                //IF Map.NearestBlock(position, range, out block) returns true, a block was found within range of the position passed in. The NEAREST block it found is passed out via the targetBlock variable.
                if (Map.Instance.NearestBlock(transform.position, blockAggroRange, out targetBlock))
                {
                    //IF the NavMeshAgent has no path/destination currently OR the distance between the CURRENT destination and the NEW destination is greater than a minimum
                    //NB this check is done so that the NavMeshAgent doesn't do unnecessary repathing. It only repaths when either A. It doesn't have a path, or B. its current path is too far off from where it should be going.
                    if (!agent.hasPath || Map.SquareDistance(targetBlock.transform.position, agent.destination) > updatePathMinimumDistanceSquared)
                    {
                        //Reset the set destination counter
                        setDestinationCounter = 0;

                        //Set a new destination for the NavMeshAgent
                        agent.SetDestination(targetBlock.transform.position);
                    }
                }

                //Otherwise if no block was found...
                //IF Map.NearestStructure(position, out base) returns true, a Structure of type Base (because the out parameter targetBase is of type Base and the method uses type generics - I can explain this more if you want)
                //a Base was found. The NEAREST base it found is passed out via the targetBase variable.
                else if (Map.Instance.NearestStructure(transform.position, out targetBase))
                {
                    //IF the NavMeshAgent has no path/destination currently OR the distance between the CURRENT destination and the NEW destination is greater than a minimum
                    if (!agent.hasPath || Map.SquareDistance(targetBase.transform.position, agent.destination) > updatePathMinimumDistanceSquared)
                    {
                        //Reset the set destination counter
                        setDestinationCounter = 0;

                        //Set a new destination for the NavMeshAgent
                        agent.SetDestination(targetBase.transform.position);
                    }
                }
            }
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
