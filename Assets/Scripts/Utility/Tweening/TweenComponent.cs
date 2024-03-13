using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

        abstract public void Tween();
    }
}

