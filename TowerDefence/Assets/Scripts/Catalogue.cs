using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private static Catalogue instance;
    public static Catalogue Instance => instance;

    private Structure placing;

    public void Awake()
    {
        instance = this;
    }

    public Structure PlaceStructurePrefab { get; set; }

    public void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && Map.Instance.MouseTile != null)
        {
            Map.Instance.PlaceStructure(PlaceStructurePrefab, Map.Instance.MouseTile.pos.x, Map.Instance.MouseTile.pos.y, 0, out placing);
            return;
        }

        if (placing != null && placing.tile != Map.Instance.MouseTile)
        {
            float direction = -Mathf.Atan2(placing.tile.pos.y - Map.Instance.MouseCoord.y, placing.tile.pos.x - Map.Instance.MouseCoord.x) * Mathf.Rad2Deg;

            direction -= 90;
            direction %= 360;
            direction += 360;
            direction += 45;
            direction %= 360;
            direction /= 90;

            int rotation = Mathf.FloorToInt(direction);

            if (placing.Rotation != rotation)
            {
                placing.Rotation = rotation;
                Map.Instance.DeclareMapChanged();
            }

        }

        if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0))
        {
            if (placing != null)
            {
                placing.placed = true;
                placing.OnPlace();
                Map.Instance.DeclareMapChanged();
            }
            placing = null;
        }

        if (Input.GetMouseButton(1))
        {
            float maxDistance = 50;
            LayerMask layerMask = LayerMask.GetMask("Structures");

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Collide))
            {
                Structure structure = hitInfo.collider.GetComponent<Structure>();
                if (!structure)
                    structure = hitInfo.collider.GetComponentInParent<Structure>();
                if (structure)
                {
                    Map.Instance.DeleteStructure(structure.tile.pos.x, structure.tile.pos.y);
                }
            }
        }
    }

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
