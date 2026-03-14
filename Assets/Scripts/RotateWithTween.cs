using UnityEngine;
using DG.Tweening;

public class RotateWithTween : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 180f;

    private Tween rotationTween;

    private void OnEnable()
    {
        float duration = Mathf.Abs(360f / rotationSpeed);
        rotationTween = transform.DORotate(new Vector3(0f, 0f, -360f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void OnDisable()
    {
        if (rotationTween != null && rotationTween.IsActive())
        {
            rotationTween.Kill();
        }
    }
}
