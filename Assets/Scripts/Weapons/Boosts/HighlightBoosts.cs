using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoostCondition
{
    protected abstract void Reset();
    protected abstract void GiveBoost();

    public virtual void AddMultiKill(int numberOfKills) { }

    public virtual void AddHeadShot() { }

    public virtual void HasReloaded() { }
}

public abstract class Boost
{

}
