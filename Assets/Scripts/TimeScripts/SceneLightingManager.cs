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

    private List<SpriteRenderer> sceneRenderers = new List<SpriteRenderer>();
    private List<Tilemap> sceneTilemaps = new List<Tilemap>();
    private List<SpriteRenderer> persistentRenderers = new List<SpriteRenderer>();

    private Dictionary<SpriteRenderer, Color> originalSpriteColors = new Dictionary<SpriteRenderer, Color>();
    private Dictionary<Tilemap, Color> originalTilemapColors = new Dictionary<Tilemap, Color>();

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
        ResetSceneLighting();
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
        sceneRenderers.Clear();
        sceneTilemaps.Clear();
        if (!playerInitialized && affectPlayer)
        {
            CollectPlayerObjects();
            playerInitialized = true;
        }

        foreach (string tag in tagsToAffect)
        {
            try
            {
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

                foreach (GameObject obj in taggedObjects)
                {
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

    private bool IsInDontDestroyOnLoad(GameObject obj)
    {
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