using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeSetUI : MonoBehaviour
{
    [SerializeField] private GameObject timeSetPanel;
    [SerializeField] private TMP_Dropdown hourDropdown;
    [SerializeField] private TMP_Dropdown minuteDropdown;
    [SerializeField] private TMP_Dropdown periodDropdown;

    private void Start()
    {
        InitializeDropdowns();
        timeSetPanel.SetActive(false);
    }

    void InitializeDropdowns()
    {
        // Hours
        hourDropdown.ClearOptions();
        for (int i = 1; i <= 12; i++)
            hourDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString("D2")));

        // Minutes
        minuteDropdown.ClearOptions();
        for (int i = 0; i < 60; i++)
            minuteDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString("D2")));

        // AM/PM
        periodDropdown.ClearOptions();
        periodDropdown.options.Add(new TMP_Dropdown.OptionData("AM"));
        periodDropdown.options.Add(new TMP_Dropdown.OptionData("PM"));
    }

    public void ToggleTimePanel()
    {
        UIManager.Instance.TogglePanel(timeSetPanel);
    }

    public void OnConfirmTime()
    {
        int hour = int.Parse(hourDropdown.options[hourDropdown.value].text);
        int minute = int.Parse(minuteDropdown.options[minuteDropdown.value].text);
        string period = periodDropdown.options[periodDropdown.value].text;

        TimeManager.Instance.SetTime(hour, minute, period);
        ToggleTimePanel();
    }

    public void OnTimeSettingsButtonClick()
    {
        if (!NPCInteraction.IsInteracting)
        {
            ToggleTimePanel();
        }
    }
}