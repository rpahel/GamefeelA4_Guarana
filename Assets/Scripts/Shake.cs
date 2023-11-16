using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Shake : MonoBehaviour
{
    [SerializeField]
    private float _duration = 1f;
    [SerializeField]
    private AnimationCurve _curve;

    private Vector3 _startPos;
    private Coroutine _shakeCoroutine = null;

    public void Start()
    {
        _startPos = transform.position;
    }

    [Button]
    public void StartShake()
    {
        if(_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
        }

        transform.position = _startPos;
        StartCoroutine(Shaking());
    }

    private IEnumerator Shaking()
    {
        {
            float elapsedTime = 0f;

            while (elapsedTime < _duration)
            {
                elapsedTime += Time.deltaTime;
                float strenght = _curve.Evaluate(elapsedTime / _duration);
                transform.position = _startPos + (Vector3)Random.insideUnitCircle * strenght;
                yield return null;
            }

            transform.position = _startPos;
        }
    }

}
