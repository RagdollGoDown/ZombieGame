using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility.Tweening
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CanvasRendererAlphaTween : TweenComponentAsync<CanvasRenderer>
    {
        [SerializeField] private float initialAlpha;
        [SerializeField] private float finalAlpha;

        public override async void Tween()
        {
            //in millisecs
            float time = 0;

            while (time < durationInMilliseconds)
            {
                await Task.Delay(stepMilliseconds);

                Component.SetColor(new(Component.GetColor().r, Component.GetColor().g, Component.GetColor().b,
                        (initialAlpha - finalAlpha) * Curve.Evaluate(time / durationInMilliseconds) + initialAlpha));
                

                time += stepMilliseconds;
            }
        }
    }
}