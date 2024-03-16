using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility.Tweening
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CanvasRendererAlphaTween : TweenComponent<CanvasRenderer>
    {
        [SerializeField] private float initialAlpha;
        [SerializeField] private float finalAlpha;

        protected override void OnTween(float value)
        {
            Component.SetColor(new(Component.GetColor().r, Component.GetColor().g, Component.GetColor().b,
                        (finalAlpha - initialAlpha) * Curve.Evaluate(value) + initialAlpha));
        }
    }
}