using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneLightingManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;

    [Header("Colors")]
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.3f, 0.3f, 0.7f, 1f);
    [SerializeField] private Color eveningColor = new Color(1f, 0.7f, 0.5f, 1f);
    [SerializeField] private Color morningColor = new Color(0.8f, 0.8f, 1f, 1f);

    [Header("Tags")]
    [SerializeField] private List<string> tagsToAffect = new List<string> { };

    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private List<Tilemap> tilemaps = new List<Tilemap>();


    private Dictionary<SpriteRenderer, Color> originalSpriteColors = new Dictionary<SpriteRenderer, Color>();
    private Dictionary<Tilemap, Color> originalTilemapColors = new Dictionary<Tilemap, Color>();

    void Start()
    {
        if (timeManager == null)
            timeManager = FindObjectOfType<TimeManager>();

        foreach (string tag in tagsToAffect)
        {
            try
            {
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
                //Debug.Log($"Found {taggedObjects.Length} objects with tag '{tag}'");

                foreach (GameObject obj in taggedObjects)
                {
                    SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>(true); //sprites
                    foreach (SpriteRenderer renderer in renderers)
                    {
                        spriteRenderers.Add(renderer);
                        originalSpriteColors[renderer] = renderer.color;
                    }
                    Tilemap tilemap = obj.GetComponent<Tilemap>(); //tilemaps
                    if (tilemap != null)
                    {
                        tilemaps.Add(tilemap);
                        originalTilemapColors[tilemap] = tilemap.color;
                    }
                }
            }
            catch (UnityException)
            {
            }
        }
    }

    void Update()
    {
        if (timeManager == null) return;

        float currentHour = timeManager.GetHour() + (timeManager.GetMinute() / 60f);
        Color targetColor = CalculateTimeColor(currentHour);

        foreach (SpriteRenderer renderer in spriteRenderers) //sprites
        {
            Color original = originalSpriteColors[renderer];
            renderer.color = new Color(
                original.r * targetColor.r,
                original.g * targetColor.g,
                original.b * targetColor.b,
                original.a
            );
        }

        foreach (Tilemap tilemap in tilemaps) //tilemaps
        {
            Color original = originalTilemapColors[tilemap];
            tilemap.color = new Color(
                original.r * targetColor.r,
                original.g * targetColor.g,
                original.b * targetColor.b,
                original.a
            );
        }
    }

    private Color CalculateTimeColor(float currentHour)
    {
        if (currentHour < 6) //(0-6)
        {
            float t = currentHour / 6f;
            return Color.Lerp(nightColor, morningColor, t);
        }
        else if (currentHour < 12) //(6-12)
        {
            float t = (currentHour - 6f) / 6f;
            return Color.Lerp(morningColor, dayColor, t);
        }
        else if (currentHour < 18) //(12-18)
        {
            float t = (currentHour - 12f) / 6f;
            return Color.Lerp(dayColor, eveningColor, t);
        }
        else //(18-24)
        {
            float t = (currentHour - 18f) / 6f;
            return Color.Lerp(eveningColor, nightColor, t);
        }
    }
}