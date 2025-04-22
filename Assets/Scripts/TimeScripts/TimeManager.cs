using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//Adam
public class TimeManager : MonoBehaviour
{
    // Singleton instance
    public static TimeManager Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private float secondsPerMinute = 1f;
    private int gameHour = 8;
    private int gameMinute = 0;
    private int gameDay = 1; // Added day counter
    private float timer = 0f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (timeDisplay == null)
        {
            TextMeshProUGUI[] textComponents = FindObjectsOfType<TextMeshProUGUI>();
            foreach (var text in textComponents)
            {
                if (text.gameObject.name.Contains("TimeDisplay") || text.CompareTag("TimeDisplay"))
                {
                    timeDisplay = text;
                    break;
                }
            }
        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        gameHour = 8;
        gameMinute = 0;
        gameDay = 1; // Initialize day to 1
    }
    // Update is called once per frame
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= secondsPerMinute)
        {
            timer -= secondsPerMinute;
            IncrementTime();
        }
        UpdateTimeDisplay();
    }
    private void IncrementTime()
    {
        gameMinute++;
        if (gameMinute >= 60)
        {
            gameMinute = 0;
            gameHour++;
            if (gameHour >= 24)
            {
                gameHour = 0;
                gameDay++; // Increment day when hour resets
            }
        }
    }
    private void UpdateTimeDisplay()
    {
        if (timeDisplay == null) return;
        string period = gameHour >= 12 ? "PM" : "AM";
        int displayHour = gameHour % 12;
        if (displayHour == 0) //convert to AM/PM vers
        {
            displayHour = 12;
        }
        string formattedTime = $"DAY {gameDay} at {displayHour:D2}:{gameMinute:D2} {period}";
        timeDisplay.text = formattedTime;
    }
    public void SetTimeDisplay(TextMeshProUGUI display)
    {
        timeDisplay = display;
    }
    //Public getter methods
    public int GetHour()
    {
        return gameHour;
    }
    public int GetMinute()
    {
        return gameMinute;
    }
    public int GetDay()
    {
        return gameDay;
    }
    public string GetTimeString()
    {
        string period = gameHour >= 12 ? "PM" : "AM";
        int displayHour = gameHour % 12;
        if (displayHour == 0) displayHour = 12;
        return $"DAY {gameDay} at {displayHour:D2}:{gameMinute:D2} {period}";
    }
}