using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScore
{
    private static float _timeSurvived;
    private static int _killCount;

    private PlayerScore(){}

    public static void ResetScore()
    {
        _timeSurvived = 0;
        _killCount = 0;
    }

    public static void AddKill()
    {
        _killCount++;
    }

    public static void AddDeltaTimeToTimeSurvived(float deltaTime)
    {
        _timeSurvived += deltaTime;
    }

    public static int GetKills()
    {
        return _killCount;
    }

    public static float GetTime()
    {
        return _timeSurvived;
    }
}
