using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class PuzzleCompletionChecker : MonoBehaviour
{
    [Header("Puzzle Pillars")]
    public List<GameObject> pillars = new List<GameObject>();

    [Header("Stone Settings")]
    public GameObject timeManipStone; // Assign your stone GameObject here
    public Sprite[] completionSprites; // 0=empty, 1=25%, 2=50%, 3=75%, 4=100%
    public float fadeDuration = 0.5f;
    public float pulseIntensity = 1.2f;
    public float pulseSpeed = 2f;

    [Header("Completion Events")]
    public bool onAllPillarsCompleted;

    private SpriteRenderer stoneRenderer;
    private int completedPillars = 0;
    private float pulseTimer = 0f;
    private bool isAnimating = false;

    void Start()
    {
        if (timeManipStone != null)
        {
            stoneRenderer = timeManipStone.GetComponent<SpriteRenderer>();
            stoneRenderer.sprite = completionSprites[0];
        }
    }

    void Update()
    {
        CheckPillarCompletion();
        
        if (isAnimating)
        {
            PulseAnimation();
        }
    }

    void CheckPillarCompletion()
    {
        int newCount = 0;
        foreach (GameObject pillar in pillars)
        {
            if (pillar.GetComponent<ObjectSnap>().hasSnappedObject && pillar.GetComponent<PillarInteraction>().hasBeenActivated)
            {
                newCount++;
            }
        }

        if (newCount != completedPillars)
        {
            completedPillars = newCount;
            StartCoroutine(AnimateStoneChange());
            
            if (completedPillars == pillars.Count)
            {
                onAllPillarsCompleted = true;
            }
        }
    }

    System.Collections.IEnumerator AnimateStoneChange()
    {
        isAnimating = true;
        pulseTimer = 0f;
        
        // Initial fade out
        float timer = 0f;
        Color originalColor = stoneRenderer.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;
        
        while (timer < fadeDuration/2)
        {
            stoneRenderer.color = Color.Lerp(originalColor, transparentColor, timer/(fadeDuration/2));
            timer += Time.deltaTime;
            yield return null;
        }
        
        // Change sprite
        stoneRenderer.sprite = completionSprites[Mathf.Clamp(completedPillars, 0, completionSprites.Length-1)];
        
        // Fade back in with pulse
        timer = 0f;
        while (timer < fadeDuration/2)
        {
            stoneRenderer.color = Color.Lerp(transparentColor, originalColor, timer/(fadeDuration/2));
            timer += Time.deltaTime;
            yield return null;
        }
        
        stoneRenderer.color = originalColor;
        isAnimating = false;
    }

    void PulseAnimation()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        float scaleFactor = Mathf.Lerp(1f, pulseIntensity, Mathf.PingPong(pulseTimer, 1f));
        timeManipStone.transform.localScale = Vector3.one * scaleFactor;
    }
}