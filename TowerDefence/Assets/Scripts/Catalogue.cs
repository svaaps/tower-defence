using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Catalogue : MonoBehaviour
{
    [SerializeField]
    private Structure[] structurePrefabs;

    [SerializeField]
    private CatalogueItem itemPrefab;

    private CatalogueItem[] items;

    [SerializeField]
    private int thumbnailSize;

    [SerializeField]
    private float thumbnailCameraDistance;

    [SerializeField]
    private Color thumbnailBackgroundColor;

    public void Start()
    {
        GenerateItems();
    }

    public void GenerateItems()
    {
        Camera camera = new GameObject().AddComponent<Camera>();
        camera.orthographic = true;
        
        camera.transform.rotation = Quaternion.Euler(45, 45, 0);
        camera.transform.position -= camera.transform.forward * thumbnailCameraDistance;
        camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = thumbnailBackgroundColor;
        camera.cullingMask = LayerMask.GetMask("Thumbnails");
        RenderTexture old = RenderTexture.active;
        camera.targetTexture = RenderTexture.active = new RenderTexture(thumbnailSize, thumbnailSize, 0, RenderTextureFormat.ARGBInt);

        Texture2D GenerateThumbnail(Structure prefab)
        {
            Texture2D thumbnail = new Texture2D(thumbnailSize, thumbnailSize);
            Structure structure = Instantiate(prefab);
            SetLayerRecursively(structure.gameObject, LayerMask.NameToLayer("Thumbnails"));

            camera.transform.position = new Vector3(0.5f, 0, 0.5f);
            camera.transform.position -= camera.transform.forward * thumbnailCameraDistance;
            camera.orthographicSize = structure.height + 0.5f;
            camera.Render();

            thumbnail.ReadPixels(new Rect(0, 0, thumbnailSize, thumbnailSize), 0, 0);
            thumbnail.Apply();

            DestroyImmediate(structure.gameObject);

            return thumbnail;
        }

        items = new CatalogueItem[structurePrefabs.Length];

        for (int i = 0; i < structurePrefabs.Length; i++)
        {
            items[i] = Instantiate(itemPrefab, transform);
            items[i].Init(structurePrefabs[i], GenerateThumbnail(structurePrefabs[i]));
        }

        RenderTexture.active = old;
        camera.targetTexture.Release();
        DestroyImmediate(camera.gameObject);
    }

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }
}
