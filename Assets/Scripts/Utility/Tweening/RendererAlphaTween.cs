using System.Threading.Tasks;
using UnityEngine;

namespace Utility.Tweening
{
    [RequireComponent(typeof(Renderer))]
    public class RendererAlphaTween : TweenComponentAsync<Renderer>
    {
        [SerializeField] private float initialAlpha;
        [SerializeField] private float finalAlpha;

        public override async void Tween()
        {
            Material[] mats = Component.materials;
           
            //in millisecs
            float time = 0;

            while (time < durationInMilliseconds)
            {
                await Task.Delay(stepMilliseconds);

                foreach (Material mat in mats)
                {
                    mat.color = new(mat.color.r, mat.color.g, mat.color.b,
                        (initialAlpha - finalAlpha) * Curve.Evaluate(time / durationInMilliseconds) + initialAlpha);
                }

                time += stepMilliseconds;
            }
        }
    }
}