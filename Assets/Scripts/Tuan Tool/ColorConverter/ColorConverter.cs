using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TuanTool
{
    public class ColorConverter : MonoBehaviour
    {
        /// <summary>
        /// Nhận hex (gamma), convert sang linear rồi trả về hex (gamma).
        /// </summary>
        public static string ConvertHexGammaToLinearBack(string hex)
        {
            // Parse hex sang Color (gamma)
            Color gammaColor;
            if (!ColorUtility.TryParseHtmlString("#" + hex, out gammaColor))
            {
                Debug.LogError("Hex không hợp lệ: " + hex);
                return hex;
            }

            // Convert sang linear
            Color linearColor = gammaColor.linear;

            // Xuất lại hex string
            return ColorUtility.ToHtmlStringRGB(linearColor);
        }

        /// <summary>
        /// Chuyển hex (gamma) sang linear Color.
        /// </summary>
        public static Color HexToLinearColor(string hex)
        {
            Color gammaColor;
            if (!ColorUtility.TryParseHtmlString("#" + hex, out gammaColor))
            {
                Debug.LogError("Hex không hợp lệ: " + hex);
                return Color.black;
            }
            return gammaColor.linear;
        }

        /// <summary>
        /// Chuyển linear Color sang hex (gamma).
        /// </summary>
        public static string LinearColorToHexGamma(Color linearColor)
        {
            Color gammaColor = linearColor.gamma;
            return ColorUtility.ToHtmlStringRGB(gammaColor);
        }


        /// <summary>
        /// Chuyển từ gamma (sRGB, như Photoshop) sang linear (Unity).
        /// </summary>
        public static Color GammaToLinear(Color gammaColor)
        {
            return new Color(
                GammaToLinearChannel(gammaColor.r),
                GammaToLinearChannel(gammaColor.g),
                GammaToLinearChannel(gammaColor.b),
                gammaColor.a
            );
        }

        public static float GammaToLinearChannel(float c)
        {
            // Công thức chuẩn sRGB -> Linear
            return (c <= 0.04045f)
                ? c / 12.92f
                : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);
        }

        /// <summary>
        /// Chuyển từ linear (Unity) sang gamma (sRGB, Photoshop).
        /// </summary>
        public static Color LinearToGamma(Color linearColor)
        {
            return new Color(
                LinearToGammaChannel(linearColor.r),
                LinearToGammaChannel(linearColor.g),
                LinearToGammaChannel(linearColor.b),
                linearColor.a
            );
        }

        public static float LinearToGammaChannel(float c)
        {
            return (c <= 0.0031308f)
                ? 12.92f * c
                : 1.055f * Mathf.Pow(c, 1f / 2.4f) - 0.055f;
        }
    }

}
