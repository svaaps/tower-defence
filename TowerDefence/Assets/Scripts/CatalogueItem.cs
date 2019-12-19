using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueItem : MonoBehaviour
{
    public Structure StructurePrefab { get; private set; }

    [SerializeField]
    private RawImage thumbnail;

    private RectTransform rectTransform;

    public void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    public void Init(Structure structurePrefab, Texture2D thumbnail)
    {
        StructurePrefab = structurePrefab;
        this.thumbnail.texture = thumbnail;
    }

    public void Pressed()
    {
        Catalogue.Instance.PlaceStructurePrefab = StructurePrefab;
    }
}
