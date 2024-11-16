using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameQuests
{
    public string questTitle;          //The name of the quest
    public string questDescription;    //Details about the quest
    public bool isCompleted;           //Whether the quest is complete
    public int questReward;             //Reward for completing the quest

    //details about current quest progress
    public string requiredItem;        //The goal of the quest
    public int requiredAmount;       //The quantity needed to complete the quest
    public int currentAmount;       //The current quantity the player has aquired
}

