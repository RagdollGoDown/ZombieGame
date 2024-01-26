using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using Utility.Observable;
using System;

namespace Utility
{ 
    /// <summary>
    /// This struct is used to store information about a damageable object that has been reaped.
    /// It should only be used by the Reaper class.
    /// </summary>
    public struct ReapedDamageableObject
        {
            private readonly DamageableObject killed;
            private readonly Damage killingBlow;
            private readonly float timeOfDeath;
            private readonly UnityEngine.Object killer;

            public ReapedDamageableObject(DamageableObject killed, float timeOfDeath)
            {
                this.killed = killed;
                this.timeOfDeath = timeOfDeath;
                killer = killed.GetLastDamageDealer();
                killingBlow = killed.GetLastDamageDone();
            }

            public DamageableObject GetKilled()
            {
                return killed;
            }

            public float GetTimeOfDeath()
            {
                return timeOfDeath;
            }

            public UnityEngine.Object GetKiller()
            {
                return killer;
            }

            public Damage GetKillingBlow()
            {
                return killingBlow;
            }
        }

    /// <summary>
    /// Is meant to collect dead damageable objects and store them in a list.
    /// It's goal was to keep a record of the enemies killed by the player.
    /// </summary>
    public class Reaper
    {
        private readonly List<ReapedDamageableObject> reapedObjects;

        public Reaper(){
            reapedObjects = new();
        }

        public void Reap(DamageableObject killed)
        {
            reapedObjects.Add(new ReapedDamageableObject(killed,Time.time));
        }

        /*private async void CheckHighlight(Object killer)
        {
            int killsNeeded = numberOfKillsToPossiblyGetHighlight;
            float maxAcceptableTime = Time.time - maxTimeBetweenKillsToGetHighlight;
            for (int i = reapedObjects.Count - 2;
                i > reapedObjects.Count - numberOfReapedCheckedForHighlight - 1 && i > 0; i--)
            {
                if (reapedObjects[i].GetTimeOfDeath() > maxAcceptableTime)
                {
                    if (killer.Equals(reapedObjects[i].GetKiller()))
                    {
                        killsNeeded--;

                        if (killsNeeded <= 0)
                        {
                            Highlight(killer);
                            break;
                        }
                    }
                }
                else
                {
                    //too much time between the kills to get a highlight
                    break;
                }

                await Task.Yield();
            }
        }

        private void Highlight(Object killer)
        {
            Debug.Log("highlight : " + killer.name);
        }*/

        //---------------------------getters

        public ReadOnlyCollection<ReapedDamageableObject> GetReapedObjects()
        {
            return reapedObjects.AsReadOnly();
        }
    }
}
