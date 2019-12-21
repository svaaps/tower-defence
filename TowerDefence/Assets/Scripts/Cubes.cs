using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cubes : MonoBehaviour
{
    [SerializeField]
    private GameObject cubePrefab;

    [SerializeField]
    private Vector3 cubeScale;


    [SerializeField]
    private int length;

    [SerializeField]
    private float explosionForce;

    [SerializeField]
    private float explosionRadius;

    private bool exploded;

    [SerializeField]
    private bool explodeOnCollision;

    

    [ContextMenu("Generate Cubes")]
    public void GenerateCubes()
    {
        Vector3 cubeScale = Vector3.one / length;

        for (int z = 0; z < length; z++)
            for (int y = 0; y < length; y++)
                for (int x = 0; x < length; x++)
                {
                    GameObject cube = Instantiate(cubePrefab, transform, false);
                    cube.transform.localPosition = new Vector3
                    (
                        -0.5f + (x + 0.5f) * cubeScale.x,
                        -0.5f + (y + 0.5f) * cubeScale.y,
                        -0.5f + (z + 0.5f) * cubeScale.z
                    );
                    cube.transform.localScale = new Vector3(cubeScale.x * this.cubeScale.x, cubeScale.y * this.cubeScale.y, cubeScale.z * this.cubeScale.z);
                    cube.transform.parent = null;
                    Rigidbody rb = cube.GetComponent<Rigidbody>();
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    Vector3 force = cube.transform.position - transform.position;
                    float distance = force.magnitude;
                    
                    if (distance > 0)
                    {
                        force /= distance;
                        force *= explosionForce;
                        force *= 1 - Mathf.Clamp(distance / explosionRadius, 0, 1);
                    }
                    else
                    {
                        force = Vector3.up * explosionForce;
                    }
                    rb.AddForce(force, ForceMode.Impulse);
                    //rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (explodeOnCollision)
        {
            Explode();
            Destroy(gameObject);
        }
    }

    public void Explode()
    {
        if (exploded)
            return;
        exploded = true;
        GenerateCubes();
    }
}
