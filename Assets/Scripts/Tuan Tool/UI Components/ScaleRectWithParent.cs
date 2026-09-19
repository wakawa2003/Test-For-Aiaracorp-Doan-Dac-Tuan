using Sirenix.OdinInspector;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace TuanTool
{
    [AddComponentMenu("Layout/Scale Rect With Target")]
    [ExecuteInEditMode, DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
    [DefaultExecutionOrder(99999)]
    public class ScaleRectWithTarget : MonoBehaviour
    {

        [SerializeField] private Vector2 offsetMin = Vector2.zero;
        [SerializeField] private Vector2 offsetMax = Vector2.zero;

        [SerializeField] private RectTransform RectTransform;
        [SerializeField] private EGetTargetType getTargetType = EGetTargetType.SetManual;
        [SerializeField, EnableIf(nameof(getTargetType), EGetTargetType.SetManual)] private RectTransform targetRect; // target cụ thể, gán trong Inspector
        [SerializeField] private bool isSyncPosition = true;
        [SerializeField] private bool isFitSize = true;


        public enum EGetTargetType
        {
            SetManual, GetParentCanvas, GetParentRectTransform
        }

        private void Start()
        {
            GetData();
            ExecuteScale();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            GetData();
            GetCurrentNormal();
            ExecuteScale();
        }
#endif
        /// <summary>
        /// Chỉnh RectTransform A fit với RectTransform B bất kể A đang là con của ai.
        /// </summary>
        public void FitToTarget(RectTransform rectA, RectTransform rectB)
        {
            rectA.transform.position = rectB.transform.position;
        }
        /// <summary>
        /// Chỉnh RectTransform A có cùng size với RectTransform B,
        /// nhưng giữ nguyên vị trí hiện tại của A.
        /// </summary>
        public void FitSizeOnly(RectTransform rectA, RectTransform rectB)
        {
            if (rectA == null || rectB == null) return;

            // Lấy world corners của B
            Vector3[] worldCorners = new Vector3[4];
            rectB.GetWorldCorners(worldCorners);

            // Chuyển về local space của cha A
            Transform parent = rectA.parent;
            Vector2 min = (Vector2)parent.InverseTransformPoint(worldCorners[0]) + offsetMin;
            Vector2 max = (Vector2)parent.InverseTransformPoint(worldCorners[2]) + offsetMax;

            // Tính size theo local space
            Vector2 size = max - min;

            rectA.anchorMin = rectA.anchorMax = Vector2.one / 2f; ; // dùng pivot cố định
            // Gán sizeDelta, giữ nguyên anchoredPosition
            rectA.sizeDelta = size;
        }

        private void LateUpdate()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                GetData();
            }
            ExecuteScale();
        }

        [Button]
        public void GetCurrentNormal()
        {
            GetData();
        }

        private void GetData()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();


            switch (getTargetType)
            {
                case EGetTargetType.GetParentCanvas:
                    targetRect = GetComponentInParent<Canvas>().transform as RectTransform;
                    break;
                case EGetTargetType.GetParentRectTransform:
                    targetRect = transform.parent as RectTransform;
                    break;
            }

        }

        [Button]
        public void ExecuteScale()
        {
            if (isFitSize)
                FitSizeOnly(RectTransform, targetRect);
            if (isSyncPosition)
                FitToTarget(RectTransform, targetRect);
        }
    }
}