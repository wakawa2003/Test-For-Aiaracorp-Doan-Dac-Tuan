using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TuanTool
{
    [DisallowMultipleComponent]
    [Tooltip("Tự động cuộn ScrollRect để hiện item được chọn trong Dropdown hoặc Menu")]
    public class DropdownAutoScroll : MonoBehaviour
    {
        public ScrollRect scrollRect; // Gán ScrollRect chứa các item
        public RectTransform container; // container chua item dang chon 

        void Update()
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;

            if (selected != null && selected.transform.IsChildOf(container.transform))
            {
                RectTransform selectedRect = selected.GetComponent<RectTransform>();
                ScrollToSelected(selectedRect);
            }
        }

        void ScrollToSelected(RectTransform target)
        {

            var normalized = Get_Normalized_RectA_In_RectB(target, container);
            scrollRect.verticalNormalizedPosition = normalized.y;


        }

        /// <summary>
        /// Trả về vị trí local của target trong hệ tọa độ của container.
        /// </summary>
        /// <param name="target">RectTransform cần tính vị trí</param>
        /// <param name="container">RectTransform cha hoặc vùng chứa</param>
        /// <returns>Vị trí local (Vector2) trong container</returns>
        public static Vector2 GetLocalPositionInRect(RectTransform target, RectTransform container)
        {
            if (target == null || container == null)
            {
                Debug.LogWarning("RectTransform null!");
                return Vector2.zero;
            }

            // Chuyển vị trí world của target thành screen point
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);

            // Chuyển screen point thành local point trong container
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screenPoint, null, out Vector2 localPoint);

            return localPoint;
        }


        /// <summary>
        /// Trả về Normalize của 4 góc Rect A so với 4 góc của Rect B (ưng dụng trong xác định scrollRect.verticalNormalizedPosition).
        /// </summary>
        /// <param name="rectA">RectTransform cần tính vị trí</param>
        /// <param name="rectB">RectTransform cha hoặc vùng chứa</param>
        /// <returns>Normalize (Vector2) trong container</returns>
        public static Vector2 Get_Normalized_RectA_In_RectB(RectTransform rectA, RectTransform rectB)
        {
            var localPoint = GetLocalPositionInRect(rectA, rectB);
            var x = Mathf.InverseLerp(
                rectB.rect.xMin + rectA.rect.width,
                rectB.rect.xMax - rectA.rect.width - rectA.rect.width,
                localPoint.x);
            var y = Mathf.InverseLerp(
                       rectB.rect.yMin + rectA.rect.height,
                       rectB.rect.yMax - rectA.rect.height - rectA.rect.height,
                       localPoint.y);


            return new Vector2(x, y);
        }

        /// <summary>
        /// Trả về vị trí normalized của 4 góc của rectA trong rectB.
        /// </summary>
        /// <param name="rectA">RectTransform cần tính</param>
        /// <param name="rectB">RectTransform làm gốc</param>
        /// <returns>Mảng 4 Vector2 normalized: [bottomLeft, topLeft, topRight, bottomRight]</returns>
        public static Vector2[] GetNormalizedCornersInRect(RectTransform rectA, RectTransform rectB)
        {
            Vector3[] worldCornersA = new Vector3[4];
            rectA.GetWorldCorners(worldCornersA);

            Vector3[] worldCornersB = new Vector3[4];
            rectB.GetWorldCorners(worldCornersB);

            // Tính kích thước của rectB trong world space
            float widthB = Vector3.Distance(worldCornersB[0], worldCornersB[3]); // bottomLeft → bottomRight
            float heightB = Vector3.Distance(worldCornersB[0], worldCornersB[1]); // bottomLeft → topLeft

            Vector2[] normalizedCorners = new Vector2[4];

            for (int i = 0; i < 4; i++)
            {
                // Tính offset từ góc trái dưới của B
                Vector3 offset = worldCornersA[i] - worldCornersB[0];

                // Chuẩn hóa theo kích thước của B
                float xNorm = offset.x / widthB;
                float yNorm = offset.y / heightB;

                normalizedCorners[i] = new Vector2(xNorm, yNorm);
            }

            return normalizedCorners;
        }

    }
}