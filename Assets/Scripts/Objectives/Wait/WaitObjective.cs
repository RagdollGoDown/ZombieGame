using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using Utility.Observable;

namespace Objectives
{
    public class WaitObjective : Objective
    {
        private readonly static int DELAY_TIME_MILLISEC = 100;
        private readonly static float DELAY_TIME_SEC = DELAY_TIME_MILLISEC * 0.001f;

        private CancellationTokenSource waitCancellationTokenSource;
        private CancellationToken waitCancellationToken;

        [SerializeField] private float waitTime;

        private ObservableObject<float> currentWaitTime;

        public override void Begin()
        {
            base.Begin();
            currentWaitTime = new ObservableObject<float>(0);
            currentWaitTime.onValueChange += (a) => { };

            waitCancellationTokenSource = new CancellationTokenSource();
            waitCancellationToken = waitCancellationTokenSource.Token;
            Wait();
        }

        private async void Wait()
        {
            while (currentWaitTime.GetValue() < waitTime && !waitCancellationToken.IsCancellationRequested)
            {
                currentWaitTime.SetValue(currentWaitTime.GetValue() + DELAY_TIME_SEC * Time.timeScale);
                await Task.Delay(DELAY_TIME_MILLISEC);
            }

            Complete();
            currentWaitTime.SetValue(0);
        }

        public override float GetCompletenessRatio()
        {
            return currentWaitTime.GetValue() / waitTime;
        }

        public void SetWaitTime(float waitTime)
        {
            this.waitTime = waitTime;
        }

        public void ObserveCurrentTime(Action<float> action)
        {
            currentWaitTime.onValueChange += action;
        }

        private void OnDisable()
        {
            waitCancellationTokenSource?.Cancel();
        }
    }
}