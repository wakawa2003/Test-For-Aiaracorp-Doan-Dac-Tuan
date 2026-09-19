using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TuanTool
{
    public class SimulationLowendDevices : MonoBehaviour
    {
#if UNITY_EDITOR
        public int countProcess = 1000;
        public Vector2 rangeRandom = new Vector2(0.8f, 1.2f);
        // Update is called once per frame
        void Update()
        {
            PeriodProcess();
        }

      
        private void FixedUpdate()
        {
            PeriodProcess();
        }
        private void LateUpdate()
        {
            PeriodProcess();
        }
        void SingleProcess()
        {
            for (int i = 0; i < 100; i++)
            {
                var a = Mathf.Pow(5000, 3000);
            }
        }

        private void PeriodProcess()
        {
            var n = Random.Range(countProcess * rangeRandom.x, countProcess * rangeRandom.y);
            for (int i = 0; i < n; i++)
            {
                SingleProcess();
            }
        }

#endif
    }
}
