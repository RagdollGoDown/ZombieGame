using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility.Tweening
{
    public sealed class TweenUtils
    {
        private TweenUtils() { }

        public static async Task Tween(float duration, CancellationToken cancellationToken,
            System.Action<float> onTween, int stepsMilliseconds = 0, 
            bool unscaledTime = false, AnimationCurve curve = null)
        {
            if (duration <= 0)
                throw new System.ArgumentException("Duration must be greater than 0", nameof(duration));
            if (onTween == null)
                throw new System.ArgumentNullException("OnTween cannot be null", nameof(onTween));
            if (stepsMilliseconds < 0)
                throw new System.ArgumentException("StepsMilliseconds must be greater or equal to 0", nameof(stepsMilliseconds));
            
            float time = 0.0f;
            while (time < duration && !cancellationToken.IsCancellationRequested)
            {
                time += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                onTween.Invoke(curve == null ? time / duration : curve.Evaluate(time/duration));

                if (stepsMilliseconds > 0)
                    await Task.Delay(stepsMilliseconds);
                else
                    await Task.Yield();
            }
        }
    }
}

