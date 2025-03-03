// Script Written by Eri aka Edwin Sotres

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController_Dynamic : MonoBehaviour
{

    [Header("UI References")]
    public RectTransform mapParent;
    public GameObject areaPrefab;
    public RectTransform playerIcon;

    [Header("Colors")]
    public Color defaultColor = Color.gray; // Areas of the map that we aren't in
    public Color curAreaColor = new Color(0.5f, 0f, 0.5f); // Areas of the map that we are in

    [Header("Map Settings")]
    public GameObject MapBounds; // Parent of the area Colliders
    public PolygonCollider2D startArea; // Starting Area
    public float mapScale = 10f; // Adjust Mapsize UI

    private PolygonCollider2D[] mapsAreas; // Children of the MapBounds
    private Dictionary<string, RectTransform> uiAreas = new Dictionary<string, RectTransform>(); // Map each PC2 to its corresponding Rect Transform

    public static MapController_Dynamic Instance { get; private set; } // Access this script from other scripts easily
    private static bool mapExists; // All instances of this Player references the exact same variable

    void Start() {

        if(!mapExists) { // If the player doesn't exist, then mark them as Don't Destroy on Load, handling duplicates

            mapExists = true;
            DontDestroyOnLoad(transform.gameObject);
        }

        else { // Eliminate Duplicate Objects
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Making sure only instance in a scene exists
        }

        mapsAreas = MapBounds.GetComponentsInChildren<PolygonCollider2D>();
        DontDestroyOnLoad(transform.gameObject);
    }

    // Generate Map
    public void GenerateMap(PolygonCollider2D newCurArea = null)
    {
        PolygonCollider2D currentArea = newCurArea != null ? newCurArea : startArea; // Set initial area if there's nothing to reference
        ClearMap();

        foreach (PolygonCollider2D area in mapsAreas)
        {
            CreateAreaUI(area, area == currentArea);
        }

        // Move Player Icon
        Debug.Log($"Total areas in dictionary: {uiAreas.Count}");

        movePlayerIcon(currentArea.name);
    }

    // Clear Map
    private void ClearMap()
    {
        foreach (Transform child in mapParent)
        {
            Destroy(child.gameObject);
        }

        uiAreas.Clear();
    }

    private void CreateAreaUI(PolygonCollider2D area, bool isCurrent)
    {
        // Instantiate prefab for image
        Debug.Log($"Creating UI for area: {area.name}, Bounds: {area.bounds}");
        GameObject areaImage = Instantiate(areaPrefab, mapParent);
        RectTransform rectTransform = areaImage.GetComponent<RectTransform>();

        // Get Bounds
        Bounds bounds = area.bounds;

        // Scale UI image fit map and bounds
        rectTransform.sizeDelta = new Vector2(bounds.size.x * mapScale, bounds.size.y * mapScale);
        rectTransform.anchoredPosition = bounds.center * mapScale;

        // Set Color based on Current
        areaImage.GetComponent<Image>().color = isCurrent ? curAreaColor : defaultColor;

        // Add to Dictionary
        uiAreas[area.name] = rectTransform;
    }

    // Update Current Area
    public void UpdateCurrentArea(string newCurrentArea)
    {
        // Update the Color
        Debug.Log($"Updating current area to: {newCurrentArea}");
        foreach (KeyValuePair<string, RectTransform> area in uiAreas)
        {
            area.Value.GetComponent<Image>().color = area.Key == newCurrentArea ? curAreaColor : defaultColor;
        }

        movePlayerIcon(newCurrentArea);
    }

    // Move Player Icon
    private void movePlayerIcon(string newCurrentArea)
    {
        if (uiAreas.TryGetValue(newCurrentArea, out RectTransform areaUI))
        {
            // If the current area is found, set the icon's position to the center of the area
            playerIcon.anchoredPosition = areaUI.anchoredPosition;
        }
    }
}
