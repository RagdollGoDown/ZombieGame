using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShakableUIElement : MonoBehaviour
{
    private static float LERP1 = 1f;

    private bool _isShaking;

    private float _shakeStrength;
    private float _lengthOfShakeInSeconds;
    private float _elapsedTime;

    private RectTransform _rectTransform;
    private Vector3 _basePosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _basePosition = _rectTransform.localPosition;
    }

    private IEnumerator ShakeCoroutine()
    {
        _isShaking = true;

        while (_elapsedTime < LERP1)
        {
            _elapsedTime += Time.deltaTime / _lengthOfShakeInSeconds;
            _rectTransform.localPosition = _basePosition +
                new Vector3(Random.value*2 -1,Random.value*2-1) * (1 - _elapsedTime) * _shakeStrength;

            yield return null;
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
