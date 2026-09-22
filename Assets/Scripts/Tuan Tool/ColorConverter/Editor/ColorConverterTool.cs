using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TuanTool
{
    [CreateAssetMenu(fileName = "ColorConverterTool", menuName = "Tools/ColorConverterTool")]
    public class ColorConverterTool : ScriptableObject
    {

        [Button]
        /// <summary>
        /// Nhận hex (gamma), convert sang linear rồi trả về hex (gamma).
        /// </summary>
        public static string ConvertHexGammaToLinearBack(string hex)
        {
            return ColorConverter.ConvertHexGammaToLinearBack(hex);
        }

        [Button]
        /// <summary>
        /// Chuyển hex (gamma) sang linear Color.
        /// </summary>
        public static Color HexToLinearColor(string hex)
        {
            return ColorConverter.HexToLinearColor(hex);
        }

        [Button]
        /// <summary>
        /// Chuyển linear Color sang hex (gamma).
        /// </summary>
        public static string LinearColorToHexGamma(Color linearColor)
        {
            return ColorConverter.LinearColorToHexGamma(linearColor);
        }

        [Button]
        /// <summary>
        /// Chuyển từ gamma (sRGB, như Photoshop) sang linear (Unity).
        /// </summary>
        public static Color GammaToLinear(Color gammaColor)
        {
            return ColorConverter.GammaToLinear(gammaColor);
        }

        [Button]
        private static float GammaToLinearChannel(float c)
        {
            return ColorConverter.GammaToLinearChannel(c);
        }


        [Button]
        /// <summary>
        /// Chuyển từ linear (Unity) sang gamma (sRGB, Photoshop).
        /// </summary>
        public static Color LinearToGamma(Color linearColor)
        {
            return ColorConverter.LinearToGamma(linearColor);
        }


        [Button]
        private static float LinearToGammaChannel(float c)
        {
            return ColorConverter.LinearToGammaChannel(c);
        }
    }


    public static class ColorConverterToolSelector
    {
        [MenuItem("Tools/Open Color Converter Tool")]
        public static void SelectColorConverterTool()
        {
            // Tìm asset
            string[] guids = AssetDatabase.FindAssets("t:ColorConverterTool");
            if (guids.Length == 0)
            {
                Debug.LogWarning("Không tìm thấy asset ColorConverterTool.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

            // Lấy type nội bộ InspectorWindow
            Type inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            if (inspectorType == null)
            {
                Debug.LogError("Không tìm thấy InspectorWindow type.");
                return;
            }

            // Tạo cửa sổ Inspector mới
            EditorWindow inspectorWindow = ScriptableObject.CreateInstance(inspectorType) as EditorWindow;
            inspectorWindow.Show();
            inspectorWindow.Focus();

            // Gán asset vào Inspector mới
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            // Bật Lock qua reflection
            PropertyInfo isLockedProp = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (isLockedProp != null)
            {
                isLockedProp.SetValue(inspectorWindow, true, null);
            }
        }
    }
}
