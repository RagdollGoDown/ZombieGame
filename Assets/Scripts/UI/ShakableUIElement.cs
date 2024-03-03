using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Utility;

public class ShakableUIElement : MonoBehaviour, IPreparable
{
    private static float LERP1 = 1f;

    private bool _isShaking;
    private CancellationTokenSource _shakeCancellationTokenSource;
    private CancellationToken _shakeCancellationToken;

    private float _shakeStrength;
    private float _lengthOfShakeInSeconds;
    private float _elapsedTime;

    private RectTransform _rectTransform;
    private Vector3 _basePosition;

    public void Prepare ()
    {
        _rectTransform = GetComponent<RectTransform>();
        _basePosition = _rectTransform.localPosition;

        _shakeCancellationTokenSource = new CancellationTokenSource();
        _shakeCancellationToken = _shakeCancellationTokenSource.Token;
    }

    private async void ShakeCoroutine()
    {
        _isShaking = true;

        while (_elapsedTime < LERP1 && !_shakeCancellationToken.IsCancellationRequested)
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
