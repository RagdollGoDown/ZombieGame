using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{ 
    public class Reaper
    {
        private struct ReapedDamageableObject
        {
            private readonly DamageableObject killed;
            private readonly Damage killingBlow;
            private readonly float timeOfDeath;
            private readonly Object killer;

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

            public Object GetKiller()
            {
                return killer;
            }

            public Damage GetKillingBlow()
            {
                return killingBlow;
            }
        }

        private List<ReapedDamageableObject> reapedObjects;

        private float probabilityOfHighlight = 1 / 2;
        private int numberOfKillsToPossiblyGetHighlight = 3;
        private float maxTimeBetweenKillsToGetHighlight = 0.5f;
        private int numberOfReapedCheckedForHighlight = 10;

        public Reaper(
            float probabilityOfHighlight = 1/2,
            int numberOfKillsToPossiblyGetHighlight = 3,
            float maxTimeBetweenKillsToGetHighlight = 0.5f,
            int numberOfReapedCheckedForHighlight = 10
            )
        {
            this.probabilityOfHighlight = probabilityOfHighlight;
            this.numberOfKillsToPossiblyGetHighlight = numberOfKillsToPossiblyGetHighlight;
            this.maxTimeBetweenKillsToGetHighlight = maxTimeBetweenKillsToGetHighlight;
            this.numberOfReapedCheckedForHighlight = numberOfReapedCheckedForHighlight;

            reapedObjects = new();
        }

        public void Reap(DamageableObject killed)
        {
            reapedObjects.Add(new ReapedDamageableObject(killed,Time.time));

            CheckHighlight(killed.GetLastDamageDealer());
        }

        private async void CheckHighlight(Object killer)
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
        }
    }
}
