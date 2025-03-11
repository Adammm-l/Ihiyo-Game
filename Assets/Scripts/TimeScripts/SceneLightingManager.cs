using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneLightingManager : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.3f, 0.3f, 0.7f, 1f);
    [SerializeField] private Color eveningColor = new Color(1f, 0.7f, 0.5f, 1f);
    [SerializeField] private Color morningColor = new Color(0.8f, 0.8f, 1f, 1f);

    [Header("Tags")]
    [SerializeField] private List<string> tagsToAffect = new List<string> { };

    [Header("Persistent Objects")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool affectPlayer = true;

    // Track scene objects and persistent objects separately
    private List<SpriteRenderer> sceneRenderers = new List<SpriteRenderer>();
    private List<Tilemap> sceneTilemaps = new List<Tilemap>();
    private List<SpriteRenderer> persistentRenderers = new List<SpriteRenderer>();

    private Dictionary<SpriteRenderer, Color> originalSpriteColors = new Dictionary<SpriteRenderer, Color>();
    private Dictionary<Tilemap, Color> originalTilemapColors = new Dictionary<Tilemap, Color>();

    // Static reference to track if player lighting has been initialized
    private static bool playerInitialized = false;

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Reset scene-specific lighting
        ResetSceneLighting();

        // Setup new lighting after a frame
        StartCoroutine(CollectObjectsNextFrame());
    }

    private IEnumerator CollectObjectsNextFrame()
    {
        yield return new WaitForEndOfFrame();
        CollectRenderersAndTilemaps();
        UpdateLighting();
    }

    void Start()
    {
        CollectRenderersAndTilemaps();
        UpdateLighting();
    }

    // Reset scene-specific lighting when changing scenes
    private void ResetSceneLighting()
    {
        foreach (SpriteRenderer renderer in sceneRenderers)
        {
            if (renderer != null && originalSpriteColors.ContainsKey(renderer))
            {
                renderer.color = originalSpriteColors[renderer];
            }
        }

        foreach (Tilemap tilemap in sceneTilemaps)
        {
            if (tilemap != null && originalTilemapColors.ContainsKey(tilemap))
            {
                tilemap.color = originalTilemapColors[tilemap];
            }
        }

        sceneRenderers.Clear();
        sceneTilemaps.Clear();
    }

    public void CollectRenderersAndTilemaps()
    {
        // Only clear scene objects, maintain persistent ones
        sceneRenderers.Clear();
        sceneTilemaps.Clear();

        // Handle player separately if it's the first time
        if (!playerInitialized && affectPlayer)
        {
            CollectPlayerObjects();
            playerInitialized = true;
        }

        // Collect scene objects with tags
        foreach (string tag in tagsToAffect)
        {
            try
            {
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

                foreach (GameObject obj in taggedObjects)
                {
                    // Skip objects that are DontDestroyOnLoad (like player)
                    if (IsInDontDestroyOnLoad(obj))
                        continue;

                    SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (SpriteRenderer renderer in renderers)
                    {
                        sceneRenderers.Add(renderer);
                        if (!originalSpriteColors.ContainsKey(renderer))
                        {
                            originalSpriteColors[renderer] = renderer.color;
                        }
                    }

                    Tilemap tilemap = obj.GetComponent<Tilemap>();
                    if (tilemap != null)
                    {
                        sceneTilemaps.Add(tilemap);
                        if (!originalTilemapColors.ContainsKey(tilemap))
                        {
                            originalTilemapColors[tilemap] = tilemap.color;
                        }
                    }
                }
            }
            catch (UnityException) { }
        }
    }

    private void CollectPlayerObjects()
    {
        // Reset persistent renderers
        persistentRenderers.Clear();

        try
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer renderer in renderers)
                {
                    persistentRenderers.Add(renderer);
                    if (!originalSpriteColors.ContainsKey(renderer))
                    {
                        originalSpriteColors[renderer] = renderer.color;
                    }
                }
            }
        }
        catch (UnityException) { }
    }

    // Check if object is in DontDestroyOnLoad scene
    private bool IsInDontDestroyOnLoad(GameObject obj)
    {
        // Objects in "DontDestroyOnLoad" scene have scene index -1
        return obj.scene.buildIndex == -1;
    }

    void Update()
    {
        UpdateLighting();
    }

    private void UpdateLighting()
    {
        if (TimeManager.Instance == null) return;

        float currentHour = TimeManager.Instance.GetHour() + (TimeManager.Instance.GetMinute() / 60f);
        Color targetColor = CalculateTimeColor(currentHour);

        // Update scene sprite renderers
        foreach (SpriteRenderer renderer in sceneRenderers)
        {
            if (renderer == null) continue;

            Color original = originalSpriteColors[renderer];
            renderer.color = new Color(
                original.r * targetColor.r,
                original.g * targetColor.g,
                original.b * targetColor.b,
                original.a
            );
        }

        // Update scene tilemaps
        foreach (Tilemap tilemap in sceneTilemaps)
        {
            if (tilemap == null) continue;

            Color original = originalTilemapColors[tilemap];
            tilemap.color = new Color(
                original.r * targetColor.r,
                original.g * targetColor.g,
                original.b * targetColor.b,
                original.a
            );
        }

        // Update player renderers
        if (affectPlayer)
        {
            foreach (SpriteRenderer renderer in persistentRenderers)
            {
                if (renderer == null) continue;

                Color original = originalSpriteColors[renderer];
                renderer.color = new Color(
                    original.r * targetColor.r,
                    original.g * targetColor.g,
                    original.b * targetColor.b,
                    original.a
                );
            }
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

    // Public method to manually reset player lighting
    public void ResetPlayerLighting()
    {
        playerInitialized = false;

        foreach (SpriteRenderer renderer in persistentRenderers)
        {
            if (renderer != null && originalSpriteColors.ContainsKey(renderer))
            {
                renderer.color = originalSpriteColors[renderer];
            }
        }

        persistentRenderers.Clear();
        CollectPlayerObjects();
    }
}