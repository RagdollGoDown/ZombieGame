using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScore
{
    private float _timeSurvived;
    private int _killCount;

    public PlayerScore(){
        _timeSurvived = 0;
        _killCount = 0;

    }

    //--------------------------------------------------setters

    public void AddKill()
    {
        _killCount++;
    }

    public int GetKills()
    {
        return _killCount;
    }

    public void AddDeltaTimeToTimeSurvived(float deltaTime)
    {
        _timeSurvived += deltaTime;
    }

    public float GetTime()
    {
        return _timeSurvived;
    }
}
