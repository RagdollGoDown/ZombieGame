using System.Collections;
using UnityEngine;

public abstract class DissolveAnimation : MonoBehaviour
{
    private static float timeBetweenFrames = 0.04f;
    [SerializeField] private float totalAnimationTimes;
    [SerializeField] private float startCutoff;
    [SerializeField] private float stopCutoff;

    private Material dissolveMaterial;

    private float _lerpPosition;

    private IEnumerator Dissolve()
    {
        while(_lerpPosition < 1)
        {
            Debug.Log(_lerpPosition);
            dissolveMaterial.SetFloat("_NoiseCutoff", Mathf.Lerp(startCutoff, stopCutoff, _lerpPosition));
            _lerpPosition += timeBetweenFrames / totalAnimationTimes;

            yield return new WaitForSeconds(timeBetweenFrames);
        }

        dissolveMaterial.SetFloat("_NoiseCutoff", stopCutoff);
    }

    protected void StartAnimation(Material mat)
    {
        dissolveMaterial = mat;
        StartCoroutine("Dissolve");
    }
}
