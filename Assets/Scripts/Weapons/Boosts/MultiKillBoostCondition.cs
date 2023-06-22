using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiKillBoostCondition : BoostCondition
{

    [SerializeField] private int killsToCountAsMultiKill;

    private int _currentMultiKills;
    [SerializeField] private int multiKillsNeeded;

    

    protected override void GiveBoost()
    {
        throw new System.NotImplementedException();
    }

    protected override void Reset()
    {
        throw new System.NotImplementedException();
    }
}
