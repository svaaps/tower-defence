using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialHueShifter : MonoBehaviour
{
    public float speed;
    public Material[] materials;
    public string nameID;

    [ColorUsage(true, true)]
    public Color initial;

    private float h, s, v;

    public void Awake()
    {
        Color.RGBToHSV(initial, out h, out s, out v);
    }

    public void Update()
    {
        h += Time.deltaTime * speed;
        h %= 1;
        foreach(Material material in materials)
            if (material != null)
                material.SetColor(nameID, Color.HSVToRGB(h, s, v));
    }
}
