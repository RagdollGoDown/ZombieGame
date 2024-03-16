using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Utility.Tweening
{
    public abstract class TweenComponent<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private float duration;
        protected float Duration
        {
            get
            {
                return duration;
            }
        }

        [SerializeField] private int stepsMilliseconds;
        protected int StepsMilliseconds
        {
            get
            {
                return stepsMilliseconds;
            }
        }

        [SerializeField] private AnimationCurve curve;

        protected AnimationCurve Curve
        {
            get
            {
                return curve;
            }
        }

        [SerializeField] private bool unscaledTime;
        protected bool UnscaledTime
        {
            get
            {
                return unscaledTime;
            }
        }

        private T component;
        protected T Component
        {
            get
            {
                return component ??= GetComponent<T>();
            }
        }


        [SerializeField] private UnityEvent onStarted;
        protected UnityEvent OnStarted
        {
            get
            {
                return onStarted;
            }
        }

        [SerializeField] private UnityEvent onComplete;
        protected UnityEvent OnComplete
        {
            get
            {
                return onComplete;
            }
        }

        public async void Tween(){
            onStarted.Invoke();
            
            await TweenUtils.Tween(duration, OnTween, stepsMilliseconds: stepsMilliseconds, unscaledTime: unscaledTime, curve: curve);

            onComplete.Invoke();
        }

        abstract protected void OnTween(float value);
    }
}

