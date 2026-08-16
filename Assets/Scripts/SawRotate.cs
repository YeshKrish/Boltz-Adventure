using DG.Tweening;
using UnityEngine;

public class SawRotate : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Seconds for one full revolution. Lower is faster.")]
    private float _secondsPerRevolution = 0.9f;

    private Tween _spin;

    private void OnEnable()
    {
        _spin = transform
            .DOLocalRotate(new Vector3(0f, 360f, 0f), _secondsPerRevolution, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        _spin?.Kill();
        _spin = null;
    }
}
