
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace TuanTool
{
    public class TimeScaleController : MonoBehaviour
    {

        [SerializeField] List<SlotTimeScale> slotTimeScales = new List<SlotTimeScale>();


        #region Singleton
        private static TimeScaleController ins;
        public static TimeScaleController Ins
        {
            get
            {
                if (ins == null)
                {
                    var a = FindObjectOfType<TimeScaleController>();
                    a?.Awake();
                }
                return ins;
            }
            set => ins = value;
        }
        #endregion

        private void Awake()
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


        }

        [Button]
        async void testtttt()
        {
            var t1 = UniTask.Defer(async () =>
            {
                await UniTask.WaitForSeconds(1);
                Debug.Log($"complete task1");
            });
            var t2 = UniTask.Defer(async () =>
            {
                await UniTask.WaitForSeconds(2);
                Debug.Log($"complete task2");
            });
            var t3 = UniTask.Defer(async () =>
            {
                await UniTask.WaitForSeconds(6);
                Debug.Log($"complete task3");
            });

            await UnitaskExtension.TimingTasks(destroyCancellationToken, (1, t1), (2, t2), (6, t3));
            Debug.Log($"complete");
        }

        void OnDestroy()
        {
            Time.timeScale = 1;
        }

        public SlotTimeScale CreatSlot(Behaviour source, float timeScale)
        {
            var newSlot = new SlotTimeScale(source, timeScale);
            AddSlot(newSlot);
            return newSlot;
        }

        public void AddSlot(SlotTimeScale timeScale)
        {
            if (!slotTimeScales.Contains(timeScale))
                slotTimeScales.Add(timeScale);
        }

        public void RemoveSlot(SlotTimeScale timeScale)
        {
            if (slotTimeScales.Contains(timeScale))
                slotTimeScales.Remove(timeScale);
        }

        private void Update()
        {
            float t = 1;
            foreach (var item in slotTimeScales)
            {
                t *= item.timeScale;
            }
            Time.timeScale = t;
            //Debug.Log($"time scale: {Time.timeScale}");
        }

        [System.Serializable]
        public class SlotTimeScale
        {
            public string Description;
            public Behaviour source;
            public float timeScale;

            public SlotTimeScale(Behaviour source, float timeScale)
            {
                this.source = source;
                this.timeScale = timeScale;
            }
        }
    }
}