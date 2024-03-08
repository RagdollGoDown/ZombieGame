using System.Threading.Tasks;
using UnityEngine;

namespace Utility.Tweening
{
    public sealed class TweenUtils
    {
        private TweenUtils() { }

        public static async void Tween(float duration, System.Action<float> onTween, bool unscaledTime)
        {
            float time = 0.0f;
            while (time < duration)
            {
                time += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                onTween.Invoke(time / duration);
                 await Task.Yield();
            }
        }
    }
}

