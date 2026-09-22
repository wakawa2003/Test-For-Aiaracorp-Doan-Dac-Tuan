using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace TuanTool
{
    //ver 1.2
    public static class TMath
    {
        /// <summary>
        ///  Clamp in range b1, b2.
        /// <br>exp: TMath.RemapClamp(1f, 0, 10, 0, 1) => 0.1</br>
        /// <br>exp: TMath.RemapClamp(1f, 10, 0, 0, 1) => 0.9</br>
        /// <br>exp: TMath.RemapClamp(1f, 3, 0, 0, 1) => 0,6666667</br>
        /// </summary>
        /// <param name="s"></param>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static float RemapClamp(float s, float a1, float a2, float b1, float b2)
        {
            return Mathf.Clamp(b1 + (s - a1) * (b2 - b1) / (a2 - a1), b1, b2);
        }

        public static float Remap(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public static Vector3 Bezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
        }

        public static Vector3 Bezier(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Mathf.Pow(1 - t, 3) * p0 + 3 * (1 - t) * (1 - t) * t * p1 + 3 * (1 - t) * t * t * p2 + Mathf.Pow(t, 3) * p3;
        }

        /// <summary>
        /// get giao diem giua tia (ray) va mat phang.
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDirection"></param>
        /// <param name="planePoint"></param>
        /// <param name="planeNormal"></param>
        /// <returns></returns>
        public static Vector3 GetIntersectionPointVsPlane(Vector3 rayOrigin, Vector3 rayDirection, Vector3 planePoint, Vector3 planeNormal)
        {
            // 1. Khởi tạo mặt phẳng từ một điểm trên mặt phẳng và vector pháp tuyến
            Plane plane = new Plane(planeNormal, planePoint);

            // 2. Tạo một tia (Ray) xuất phát từ rayOrigin theo hướng rayDirection
            Ray ray = new Ray(rayOrigin, rayDirection);

            // 3. Sử dụng hàm Raycast của Plane để tìm khoảng cách từ rayOrigin đến điểm giao
            if (plane.Raycast(ray, out float enter))
            {
                // 4. Lấy tọa độ điểm giao dựa trên khoảng cách 'enter'
                return ray.GetPoint(enter);
            }

            // Nếu tia không cắt mặt phẳng (song song hoặc hướng ngược lại)
            return Vector3.zero;
        }
        public static Vector2 GiaoDiem2DuongThang(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            // Line AB represented as a1x + b1y = c1  
            float a1 = B.y - A.y;
            float b1 = A.x - B.x;
            float c1 = a1 * (A.x) + b1 * (A.y);

            // Line CD represented as a2x + b2y = c2  
            float a2 = D.y - C.y;
            float b2 = C.x - D.x;
            float c2 = a2 * (C.x) + b2 * (C.y);

            float determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                Debug.LogError("Hai duong thang song song!!");
                return new Vector2(float.MaxValue, float.MaxValue);
            }
            else
            {
                float x = (b2 * c1 - b1 * c2) / determinant;
                float y = (a1 * c2 - a2 * c1) / determinant;
                return new Vector2(x, y);
            }
        }

        /// <summary>
        /// Binary Search
        /// </summary>
        public static (int i_prev, int i_next, float prev, float next) FindPrevNext(List<float> list, float value)
        {
            int l = 0;
            int r = list.Count - 1;

            while (l <= r)
            {
                int m = l + (r - l) / 2;

                if (list[r] == value)
                {
                    return (r - 1, r, list[r - 1], list[r]);
                }
                if (m + 1 == list.Count)//cuoi danh sach
                {
                    return (r - 1, -1, list[r - 1], 0);
                }
                if (value >= list[m] && value < list[m + 1])
                {
                    return (m, m + 1, list[m], list[m + 1]);
                }

                // If x greater, ignore left half 
                if (list[m] < value)
                    l = m + 1;

                // If x is smaller, ignore right half 
                else
                    r = m - 1;
            }

            //Debug.LogError("Khong tim thay khoang gia tri cua: " + value);
            return (-1, -1, 0, 0);
        }

        /// <summary>
        /// Binary Search
        /// </summary>
        public static (int i_prev, int i_next, float prev, float next) FindPrevNext(float[] list, float value)
        {
            int l = 0;
            int r = list.Length - 1;

            while (l <= r)
            {
                int m = l + (r - l) / 2;

                if (list[r] == value)
                {
                    return (r - 1, r, list[r - 1], list[r]);
                }
                if (m + 1 == list.Length)//cuoi danh sach
                {
                    return (r - 1, -1, list[r - 1], 0);
                }
                if (value >= list[m] && value < list[m + 1])
                {
                    return (m, m + 1, list[m], list[m + 1]);
                }

                // If x greater, ignore left half 
                if (list[m] < value)
                    l = m + 1;

                // If x is smaller, ignore right half 
                else
                    r = m - 1;
            }

            //Debug.LogError("Khong tim thay khoang gia tri cua: " + value);
            return (-1, -1, 0, 0);
        }

        public static float RandomMinMax(Vector2 minMax)
        {
            return UnityEngine.Random.Range(minMax.x, minMax.y);
        }

        public static bool RandomPercent(int percent)
        {
            int t = UnityEngine.Random.Range(1, 101);
            return (t <= percent);
        }

        public static bool RandomPercent(float percent)
        {
            float t = UnityEngine.Random.Range(0f, 100f);
            return (t <= percent);
        }

        public static t RandomSelect<t>(params t[] numbers)
        {
            return numbers[UnityEngine.Random.Range(0, numbers.Length)];

        }

        public static t RandomSelect<t>(List<t> numbers)
        {
            return numbers[UnityEngine.Random.Range(0, numbers.Count)];
        }

        public static t RandomEnum<t>() where t : Enum
        {
            var a = Enum.GetValues(typeof(t));
            return (t)a.GetValue(UnityEngine.Random.Range(0, a.Length));

        }

        /// <summary>
        /// <image url="https://drive.google.com/open?id=124AB4tF_TKG6K_izD8hvDIwoW5vCPfI6" scale="0.5"/>
        /// Huong cua "return" la khong xac dinh tren duong tron.
        /// </summary>
        public static Vector3 RotateVector(Vector3 dir, float angle, float angleQuanhDir)
        {
            float D = Vector3.Magnitude(dir) * Mathf.Tan(angle * Mathf.Deg2Rad);
            Vector3 dirD = dir + GetPerpendicularXY(dir).normalized * D;
            dirD = Quaternion.AngleAxis(angleQuanhDir, dir) * dirD;
            return dirD;
        }

        /// <summary>
        /// Tra ve vuong goc dang vector3 (x,y,0).
        /// </summary>
        public static Vector3 GetPerpendicularXY(Vector3 dir)
        {
            if (dir.x == 0 && dir.y == 0 && dir.z != 0)
                return new Vector3(dir.z, 0, 0);
            return new Vector3(-dir.y, dir.x, 0);
        }

        /// <summary>
        /// Draw direction with Arrow.
        /// Dir cung la length.
        /// </summary>
        public static void DrawDirection(Vector3 origin, Vector3 dir, Color color, float lengthArrow = 0.2f, float angleArrow = 30)
        {
            float length = lengthArrow;
            Vector3 endPos = origin + dir;

            Debug.DrawLine(origin, endPos, color);

            //draw arow
            Debug.DrawLine(endPos, endPos + TMath.RotateVector(-dir, angleArrow, 0).normalized * length, color);
            Debug.DrawLine(endPos, endPos + TMath.RotateVector(-dir, angleArrow, 90).normalized * length, color);
            Debug.DrawLine(endPos, endPos + TMath.RotateVector(-dir, angleArrow, 180).normalized * length, color);
            Debug.DrawLine(endPos, endPos + TMath.RotateVector(-dir, angleArrow, 270).normalized * length, color);


        }


        /// <summary>
        /// Vuong goc.
        /// </summary>
        public static Vector3 PerpendicularXY(this Vector3 vector3)
        {
            return GetPerpendicularXY(vector3);
        }

        /// <summary>
        /// Exp:
        /// count = 100;
        /// i= 0 ;
        /// step= 103 => 3;
        /// step= -103 => 97;
        /// step= -4 => 96;
        /// </summary>
        /// <param name="countList"></param>
        /// <param name="currentIPosition"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static int LoopStepCalculate(int countList, int currentIPosition, int step)
        {
            countList = Mathf.Abs(countList);
            step = step % countList;
            int t = (countList + currentIPosition + step) % countList;
            t = Mathf.Abs(t);
            return t;
        }

        /// <summary>
        /// Convert Rotation From 0 : 360 To -x:+x. 
        /// transform.localEulerAngles is 0:360 type. 
        /// Inspector Rotation is -x:+x type. 
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float Convert_0_360_To_NegativableAngle(float angle)
        {
            return (angle > 180) ? angle - 360 : angle;
        }


        public static AnimationCurve AnimationCurve_Linear01()
        {
            return AnimationCurve.Linear(0.0f, 0.0f, 1, 1);
        }


        /// <summary>
        /// Same with Mathf.Sign() but have return 0.
        /// </summary>
        public static int Sign(int t)
        {
            return System.Math.Sign(t);
        }
        public static int Sign(float t)
        {
            return System.Math.Sign(t);
        }

        /// <summary>
        /// Sum of all weights is unlimited.
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int GetRandomWeightedIndex(List<float> weights)
        {
            if (weights == null || weights.Count == 0) return -1;

            float w;
            float t = 0;
            int i;
            for (i = 0; i < weights.Count; i++)
            {
                w = weights[i];

                if (float.IsPositiveInfinity(w))
                {
                    return i;
                }
                else if (w >= 0f && !float.IsNaN(w))
                {
                    t += weights[i];
                }
            }

            float r = UnityEngine.Random.value;
            float s = 0f;

            for (i = 0; i < weights.Count; i++)
            {
                w = weights[i];
                if (float.IsNaN(w) || w <= 0f) continue;

                s += w / t;
                if (s >= r) return i;
            }

            return -1;
        }

        /// <summary>
        /// Tạo bound mới có cùng tỷ lệ khung hình 3D (X:Y:Z) với boundReference,
        /// nhưng đủ lớn để bao trùm hoàn toàn boundTarget.
        /// </summary>
        /// <param name="boundTarget">Bound cần được bao trùm</param>
        /// <param name="boundReference">Bound làm mẫu tỷ lệ (giữ nguyên tỷ lệ X:Y:Z)</param>
        /// <returns>Bound mới có cùng tỷ lệ và bao trọn boundTarget</returns>
        public static Bounds ExpandBoundsKeepingAspect3D(Bounds boundTarget, Bounds boundReference)
        {
            Vector3 min = boundTarget.min;
            Vector3 max = boundTarget.max;
            Vector3 requiredSize = max - min;

            // Tỷ lệ khung hình 3D từ boundReference
            float aspectXY = boundReference.size.x / boundReference.size.y;
            float aspectXZ = boundReference.size.x / boundReference.size.z;

            // Tính kích thước mới để giữ tỷ lệ và bao boundTarget
            float newWidth = requiredSize.x;
            float newHeight = newWidth / aspectXY;
            float newDepth = newWidth / aspectXZ;

            // Nếu chưa đủ để bao boundTarget theo Y hoặc Z, mở rộng thêm
            if (newHeight < requiredSize.y)
            {
                newHeight = requiredSize.y;
                newWidth = newHeight * aspectXY;
                newDepth = newWidth / aspectXZ;
            }

            if (newDepth < requiredSize.z)
            {
                newDepth = requiredSize.z;
                newWidth = newDepth * aspectXZ;
                newHeight = newWidth / aspectXY;
            }

            Vector3 newSize = new Vector3(newWidth, newHeight, newDepth);
            Vector3 center = (min + max) / 2f;

            return new Bounds(center, newSize);
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
        /// Trả về Normalize của 4 góc Rect A so với 4 góc của Rect B (ứng dụng trong xác định scrollRect.verticalNormalizedPosition).
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