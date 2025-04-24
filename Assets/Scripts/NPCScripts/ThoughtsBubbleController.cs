using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ThoughtsBubbleController : MonoBehaviour
{
    [Header("Thought Settings")]
    [SerializeField] private List<string> thoughtFragments;
    [SerializeField] private GameObject thoughtTextPrefab;
    [SerializeField] private RectTransform thoughtsContainer;

    [Header("Animation Settings")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 2.0f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float stayTime = 1.5f;
    [SerializeField] private float fadeOutTime = 0.7f;

    private List<GameObject> activeThoughts = new List<GameObject>();
    private bool isDisplayingThoughts = false;

    public void StartThoughts()
    {
        isDisplayingThoughts = true;
        StartCoroutine(SpawnThoughtsRoutine());
    }

    public void StopThoughts()
    {
        isDisplayingThoughts = false;
        foreach (GameObject thought in activeThoughts)
        {
            Destroy(thought);
        }
        activeThoughts.Clear();
    }

    private IEnumerator SpawnThoughtsRoutine()
    {
        while (isDisplayingThoughts)
        {
            SpawnThought();
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    private void SpawnThought()
    {
        // Pick a random thought
        string thought = thoughtFragments[Random.Range(0, thoughtFragments.Count)];

        // Create text object at random position within container
        GameObject thoughtObj = Instantiate(thoughtTextPrefab, thoughtsContainer);
        RectTransform rt = thoughtObj.GetComponent<RectTransform>();

        // Random position within container bounds
        float randX = Random.Range(-thoughtsContainer.rect.width / 2 + 50, thoughtsContainer.rect.width / 2 - 50);
        float randY = Random.Range(-thoughtsContainer.rect.height / 2 + 20, thoughtsContainer.rect.height / 2 - 20);
        rt.anchoredPosition = new Vector2(randX, randY);

        // Set the text
        TextMeshProUGUI textComponent = thoughtObj.GetComponent<TextMeshProUGUI>();
        textComponent.text = thought;
        textComponent.alpha = 0f;

        // Manage the thought object
        activeThoughts.Add(thoughtObj);
        StartCoroutine(AnimateThought(thoughtObj, textComponent));
    }

    private IEnumerator AnimateThought(GameObject thoughtObj, TextMeshProUGUI textComponent)
    {
        // Fade in
        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            textComponent.alpha = timer / fadeInTime;
            yield return null;
        }

        // Stay visible
        yield return new WaitForSeconds(stayTime);

        // Fade out
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            textComponent.alpha = 1 - (timer / fadeOutTime);
            yield return null;
        }

        // Remove and destroy
        activeThoughts.Remove(thoughtObj);
        Destroy(thoughtObj);
    }
}