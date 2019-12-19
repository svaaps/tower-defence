using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshObject : MonoBehaviour
{
    public int area;
    public NavMeshBuildSource NavMeshBuildSource()
    {
        return new NavMeshBuildSource
        {
            transform = transform.localToWorldMatrix,
            area = area,
            component = this,
            shape = NavMeshBuildSourceShape.Mesh,
            sourceObject = GetComponent<MeshFilter>().sharedMesh
        };
    }
}
