using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScore
{
    private static float _timeSurvived;
    private static int _killCount;

    private static int _multiKillsTotal;
    private static int _headShotsTotal;

    private PlayerScore(){}

    //--------------------------------------------------setters

    public static void ResetScore()
    {
        _timeSurvived = 0;
        _killCount = 0;
        _headShotsTotal = 0;
    }

    public static void AddHeadShot()
    {
        _headShotsTotal++;
    }

    public static int GetHeadShots()
    {
        return _headShotsTotal;
    }

    public static void AddKill()
    {
        _killCount++;
    }

    public static int GetKills()
    {
        return _killCount;
    }

    public static void AddDeltaTimeToTimeSurvived(float deltaTime)
    {
        _timeSurvived += deltaTime;
    }

    public static float GetTime()
    {
        return _timeSurvived;
    }
}
