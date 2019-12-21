using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField]
    private float minimumLife, maximumLife;
    private bool dead;
    private float life;

    public void Awake()
    {
        life = Random.Range(minimumLife, maximumLife);
    }
    public void Update()
    {
        life -= Time.deltaTime;
        if (life < 0 && !dead)
        {
            dead = true;
            Destroy(gameObject);
        }
    }
}
