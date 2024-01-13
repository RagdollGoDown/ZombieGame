using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Objectives
{
    public class WaitObjective : Objective
    {
        private readonly static int DELAY_TIME_MILLISEC = 100;
        private readonly static float DELAY_TIME_SEC = DELAY_TIME_MILLISEC * 0.001f;

        [SerializeField] private float waitTime;
        private float currentWaitTime;

        public override void Begin()
        {
            base.Begin();
            currentWaitTime = 0;

            Wait();
        }

        private async void Wait()
        {
            while (currentWaitTime < waitTime)
            {
                currentWaitTime += DELAY_TIME_SEC;
                await Task.Delay(DELAY_TIME_MILLISEC);
            }

            Complete();
            currentWaitTime = 0;
        }

        public override float GetCompletenessRatio()
        {
            return currentWaitTime / waitTime;
        }

        public void SetWaitTime(float waitTime)
        {
            this.waitTime = waitTime;
        }
    }
}