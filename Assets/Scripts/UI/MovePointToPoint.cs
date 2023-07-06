using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MovePointToPoint : MonoBehaviour
{
    private static float LERP1 = 1f;

    [SerializeField] private Vector3 point1;
    [SerializeField] private Vector3 point2;

    private RectTransform _rectTransform;

    [SerializeField] private AnimationCurve curve;

    [SerializeField] private float timeTillCompletion;
    private float _elapsedLerpTime;

    private bool _isMoving;

    public void Point1to2()
    {
        if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();

        if (!_isMoving) StartCoroutine(nameof(Point1to2Numerator));
    }

    private IEnumerator Point1to2Numerator()
    {
        _isMoving = true;

        while(_elapsedLerpTime < LERP1)
        {
            _elapsedLerpTime += Time.deltaTime/timeTillCompletion;

            _rectTransform.anchoredPosition3D = Vector3.Lerp(point1, point2, curve.Evaluate(_elapsedLerpTime));

            yield return null;
        }

        _isMoving = false;
    }

    public void Point2to1()
    {
        if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();

        if (!_isMoving) StartCoroutine(nameof(Point1to2Numerator));
    }

    private IEnumerator Point2to1Numerator()
    {
        _isMoving = true;

        while (_elapsedLerpTime < LERP1)
        {
            _elapsedLerpTime += Time.deltaTime / timeTillCompletion;

            _rectTransform.anchoredPosition3D = Vector3.Lerp(point2, point1, curve.Evaluate(_elapsedLerpTime));

            yield return null;
        }

        _isMoving = false;
    }
}
