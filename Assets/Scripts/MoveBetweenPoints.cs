using UnityEngine;
using DG.Tweening;

public class MoveBetweenPoints : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float duration = 1f;
    [SerializeField] private AnimationCurve motionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private bool loop = true;
    [SerializeField] private Transform rotateTargetA;
    [SerializeField] private Transform rotateTargetB;
    [SerializeField] private float rotationAmount = -360f;

    private Tween moveTween;
    private Tween rotateTweenA;
    private Tween rotateTweenB;

    private void OnEnable()
    {
        if (pointA == null || pointB == null)
        {
            return;
        }

        transform.position = pointA.position;
        moveTween = transform.DOMove(pointB.position, duration)
            .SetEase(motionCurve);

        if (rotateTargetA != null)
        {
            rotateTweenA = rotateTargetA.DORotate(new Vector3(0f, 0f, rotationAmount), duration, RotateMode.FastBeyond360)
                .SetEase(motionCurve);
        }

        if (rotateTargetB != null)
        {
            rotateTweenB = rotateTargetB.DORotate(new Vector3(0f, 0f, rotationAmount), duration, RotateMode.FastBeyond360)
                .SetEase(motionCurve);
        }

        if (loop)
        {
            moveTween.SetLoops(-1, LoopType.Yoyo);
            rotateTweenA?.SetLoops(-1, LoopType.Yoyo);
            rotateTweenB?.SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnDisable()
    {
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
        }

        if (rotateTweenA != null && rotateTweenA.IsActive())
        {
            rotateTweenA.Kill();
        }

        if (rotateTweenB != null && rotateTweenB.IsActive())
        {
            rotateTweenB.Kill();
        }
    }
}
