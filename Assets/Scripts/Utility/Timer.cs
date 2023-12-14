using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Utility
{
    public class Timer
    {
        //in seconds
        private float period = 100;

        private float speed = 1;

        public Timer(float period)
        {
            this.period = period;
        }

        public float GetSpeed() 
        {
            return speed;
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
    }
}