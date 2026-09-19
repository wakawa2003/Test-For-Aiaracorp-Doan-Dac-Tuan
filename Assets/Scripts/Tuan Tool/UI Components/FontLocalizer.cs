using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;


namespace TuanTool
{
    /// <summary>
    /// Localize cho Font, cần tạo font asset database trong localize asset database.
    /// </summary>
    [DisallowMultipleComponent]
    public class FontLocalizer : MonoBehaviour
    {
        private TMP_Text textComponent;
        public LocalizedAsset<TMP_FontAsset> localizedFont;

        void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
            if (textComponent == null)
                Debug.LogError($"khong tim thay tmp_text");
            localizedFont.AssetChanged += UpdateFont;
            localizedFont.LoadAssetAsync();

        }

        private void OnDestroy()
        {
            localizedFont.AssetChanged -= UpdateFont;
        }

        void UpdateFont(TMP_FontAsset font)
        {
            textComponent.font = font;
        }
    }
}