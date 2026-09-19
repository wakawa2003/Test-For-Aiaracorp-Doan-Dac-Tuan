
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TuanTool
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ButtonSyncWithText : Button
    {
        public CanvasGroup m_CanvasGroup;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            Color tintColor;
            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = colors.normalColor;
                    break;
                case SelectionState.Highlighted:
                    tintColor = colors.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    tintColor = colors.pressedColor;
                    break;
                case SelectionState.Selected:
                    tintColor = colors.selectedColor;
                    break;
                case SelectionState.Disabled:
                    tintColor = colors.disabledColor;
                    break;
                default:
                    tintColor = Color.black;
                    break;
            }
            if (transition == Transition.ColorTint)
                if (m_CanvasGroup != null)
                {
                    float a = tintColor.a * tintColor.r * tintColor.g * tintColor.b;
                    m_CanvasGroup.DOFade(a, instant ? 0f : colors.fadeDuration).SetUpdate(true).KillOnDestroy(m_CanvasGroup.transform);
                }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Color tintColor = colors.normalColor;
            if (interactable)
            {
                if (m_CanvasGroup != null)
                    tintColor = colors.normalColor;
            }
            else
            {
                if (m_CanvasGroup != null)
                    tintColor = colors.disabledColor;
            }
            if (m_CanvasGroup == null) m_CanvasGroup = GetComponentInChildren<CanvasGroup>();
            float a = tintColor.a * tintColor.r * tintColor.g * tintColor.b;
            m_CanvasGroup.alpha = a * colors.colorMultiplier;
        }

        protected override void Reset()
        {
            base.Reset();
            if (!m_CanvasGroup) m_CanvasGroup = GetComponentInChildren<CanvasGroup>();
        }
#endif
    }
}