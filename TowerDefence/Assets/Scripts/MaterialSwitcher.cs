using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    [SerializeField]
    private Material[] materials;

    [SerializeField]
    private int index;

    private Renderer rend;
    public int Index => index;
    public void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.material = materials[index];
    }

    public void SetMaterial(int index)
    {
        if (index < 0 || index >= materials.Length)
            return;
        rend.material = materials[index];
        this.index = index;
    }
    public Material GetMaterial()
    {
        return materials[index];
    }
}
