using System.Threading.Tasks;
using UnityEngine;
using Utility;

public class ShakableUIElement : MonoBehaviour, Preparable
{
    private static float LERP1 = 1f;

    private bool _isShaking;

    private float _shakeStrength;
    private float _lengthOfShakeInSeconds;
    private float _elapsedTime;

    private RectTransform _rectTransform;
    private Vector3 _basePosition;

    public void Prepare ()
    {
        _rectTransform = GetComponent<RectTransform>();
        _basePosition = _rectTransform.localPosition;
    }

    private async void ShakeCoroutine()
    {
        _isShaking = true;

        while (_elapsedTime < LERP1)
        {
            _elapsedTime += Time.deltaTime / _lengthOfShakeInSeconds;
            _rectTransform.localPosition = _basePosition +
                new Vector3(Random.value*2 -1,Random.value*2-1) * (1 - _elapsedTime) * _shakeStrength;

            await Task.Yield();
        }

        _isShaking = false;
    }

    public void Shake()
    {
        if (!_isShaking) StartCoroutine(nameof(ShakeCoroutine));

        _elapsedTime = 0;
    }

    public void SetLengthAndStrength(float length, float strength)
    {
        _lengthOfShakeInSeconds = length;
        _shakeStrength = strength;
    }
}
