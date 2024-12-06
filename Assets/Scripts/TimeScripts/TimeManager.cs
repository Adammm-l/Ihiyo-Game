using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private float secondsPerMinute = 1f;

    private int gameHour = 8;
    private int gameMinute = 0;
    private float timer = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        gameHour = 8;
        gameMinute = 0;
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

        string formattedTime = $"{displayHour:D2}:{gameMinute:D2} {period}";
        timeDisplay.text = formattedTime;
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

    public string GetTimeString()
    {
        string period = gameHour >= 12 ? "PM" : "AM";
        int displayHour = gameHour % 12;
        if (displayHour == 0) displayHour = 12;
        return $"{displayHour:D2}:{gameMinute:D2} {period}";
    }
}

