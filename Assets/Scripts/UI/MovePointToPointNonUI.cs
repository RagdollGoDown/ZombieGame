using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePointToPointNonUI : MonoBehaviour
{
    private static float LERP1 = 1f;

    [SerializeField] private Vector3 point1;
    [SerializeField] private Vector3 point2;
    [SerializeField] private AnimationCurve curve;

    [SerializeField] private float timeTillCompletion;
    private float _elapsedLerpTime;

    private bool _isMoving;

    public void Point1to2()
    {
        if (!_isMoving) StartCoroutine(nameof(Point1to2Numerator));
    }

    private IEnumerator Point1to2Numerator()
    {
        _isMoving = true;
        _elapsedLerpTime = 0;

        while(_elapsedLerpTime < LERP1)
        {
            _elapsedLerpTime += Time.deltaTime/timeTillCompletion;

            transform.localPosition = Vector3.Lerp(point1, point2, curve.Evaluate(_elapsedLerpTime));

            yield return null;
        }

        _isMoving = false;
    }

    public void Point2to1()
    {
        if (!_isMoving) StartCoroutine(nameof(Point2to1Numerator));
    }

    private IEnumerator Point2to1Numerator()
    {
        _isMoving = true;
        _elapsedLerpTime = 0;

        while (_elapsedLerpTime < LERP1)
        {
            _elapsedLerpTime += Time.deltaTime / timeTillCompletion;

            transform.localPosition = Vector3.Lerp(point2, point1, curve.Evaluate(_elapsedLerpTime));

            yield return null;
        }

        _isMoving = false;
    }
}
