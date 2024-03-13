using System.Threading.Tasks;
using UnityEngine;

namespace Utility.Tweening
{
    public sealed class TweenUtils
    {
        private TweenUtils() { }

        public static async void Tween(float duration, System.Action<float> onTween, 
            bool unscaledTime = false, AnimationCurve curve = null)
        {
            float time = 0.0f;
            while (time < duration)
            {
                time += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                onTween.Invoke(curve == null ? time / duration : curve.Evaluate(time/duration));
                 await Task.Yield();
            }
        }

        public static async void Tween(float duration, System.Action<float> onTween,int stepSizeMilliSeconds,
            bool unscaledTime = false, AnimationCurve curve = null)
        {
            float time = 0.0f;
            while (time < duration)
            {
                time += stepSizeMilliSeconds * 1000;

                onTween.Invoke(curve == null ? time / duration : curve.Evaluate(time / duration));
                await Task.Delay(stepSizeMilliSeconds);
            }
        }
    }
}

