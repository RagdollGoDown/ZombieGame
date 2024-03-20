using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility.Tweening
{
    [RequireComponent(typeof(Light))]
    public class LightIntensityTween : TweenComponent<Light>
    {
        [SerializeField] private float initialIntensity;
        [SerializeField] private float finalIntensity;

        protected override void OnTween(float value)
        {
            Component.intensity = (finalIntensity - initialIntensity) * Curve.Evaluate(value) + initialIntensity;
        }
    }
}