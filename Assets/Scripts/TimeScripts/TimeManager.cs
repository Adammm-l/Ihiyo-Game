using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//Adam
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public bool canIncrementTime;
    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private float secondsPerMinute = 1f;
    private int gameHour = 8;
    private int gameMinute = 0;
    private float timer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            canIncrementTime = true;
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
    }

    // Update is called once per frame
    private void Update()
    {
        if (!canIncrementTime)
        {
            return;
        }
        
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
            }
        }
    }

    private void UpdateTimeDisplay()
    {
        timeDisplay.text = GetFormattedTime();
    }

    public void SetTimeDisplay(TextMeshProUGUI display)
    {
        timeDisplay = display;
    }
    
    public string GetFormattedTime()
    {
        string period = gameHour >= 12 ? "PM" : "AM";
        int displayHour = gameHour % 12;
        if (displayHour == 0) 
        {
            displayHour = 12;
        }
        return $"{displayHour:D2}:{gameMinute:D2} {period}";
    }

    public int GetHour()
    {
        return gameHour;
    }

    public int GetMinute()
    {
        return gameMinute;
    }

    public string GetTimeString()
    {
        string period = gameHour >= 12 ? "PM" : "AM";
        int displayHour = gameHour % 12;
        if (displayHour == 0) displayHour = 12;
        return $"{displayHour:D2}:{gameMinute:D2} {period}";
    }

    public void SetTime(int hour12, int minute, string period)
    {
        
        if (hour12 < 1 || hour12 > 12)
        {
            Debug.LogError("Invalid hour");
            return;
        }
        if (minute < 0 || minute >= 60)
        {
            Debug.LogError("Invalid minute");
            return;
        }

        
        if (period == "PM")
        {
            gameHour = (hour12 == 12) ? 12 : hour12 + 12;
        }
        else // AM
        {
            gameHour = (hour12 == 12) ? 0 : hour12;
        }

        gameMinute = minute;
        UpdateTimeDisplay();
    }
}