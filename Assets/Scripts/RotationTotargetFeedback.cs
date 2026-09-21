using DG.Tweening;
using UnityEngine;

namespace MyGameNamespace
{
    public class RotationTotargetFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform rotator;
        [SerializeField] private Transform target;

        [Header("Rotation settings")]
        [SerializeField] private float Duration = 0.2f;

        private Tween rotationTween;

        public void StartRun()
        {
            if (rotator == null || target == null)
            {
                return;
            }

            rotationTween?.Kill();

            Vector3 direction = target.position - rotator.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

            rotationTween = rotator.DORotateQuaternion(targetRotation, Duration).SetEase(Ease.Linear);
        }
    }
}
