using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using UnityEngine;

namespace TuanTool.Processing
{
    public class ProcessingPopup : Popup.Popup
    {
        private Tween rotateTween;

        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 1f;
        [SerializeField] private Transform iconTransform;

        #region Singleton
        private static ProcessingPopup ins;
        public static ProcessingPopup Ins
        {
            get
            {
                if (ins == null)
                {
                    var a = FindObjectOfType<ProcessingPopup>();
                    a?.Awake();
                }
                return ins;
            }
            set => ins = value;
        }
        #endregion

        protected override void Awake()
        {
            #region Singleton
            if (ins == null)
                ins = this;
            else
            {
                if (ins != this)
                    Destroy(gameObject);
                return;
            }
            #endregion
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }


        protected override void Update()
        {
            base.Update();
            var process = ProcessingController.Ins;

            bool isProcessing = false;
            foreach (var item in process.ProcessStacks.ToArray())
            {
                if (item.State == ProcessingController.ProcessStack.EState.Processing)
                    isProcessing = true;
            }

            if (IsShowing && !isProcessing)
                Hide();
            else if (!IsShowing && isProcessing)
                RequestShow();

        }

        public override void Show(Action onComplete = null)
        {
            base.Show(onComplete);
            rotateTween?.Kill();
            rotateTween = iconTransform.DORotate(new Vector3(0, 0, -360), rotationSpeed, RotateMode.FastBeyond360)
       .SetEase(Ease.Linear)
       .SetLoops(-1, LoopType.Restart)
       .SetLink(Ins.gameObject)
       .SetUpdate(true)
       .OnKill(() => rotateTween = null);
        }
    }
}