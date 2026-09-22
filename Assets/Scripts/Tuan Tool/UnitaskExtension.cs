using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace TuanTool
{
    public class UnitaskExtension
    {
        async void testTimingTasks()
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

            await UnitaskExtension.TimingTasks(default, (1, t1), (2, t2), (6, t3));
            Debug.Log($"complete");
        }


        /// <summary>
        /// Ensure multyple tasks completed in same time.
        /// Tasks need Defered.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="timeTaksDefered"></param>
        /// Duration in seconds and Defered Unitask (use  UniTask.Defer())
        /// <returns></returns>
        public static async UniTask TimingTasks(CancellationToken cancellationToken = default, params (float, UniTask)[] timeTaksDefered)
        {
            var maxTime = timeTaksDefered.Max(_ => _.Item1);
            List<UniTask> uniTasks = new List<UniTask>();
            foreach (var item in timeTaksDefered)
            {
                var a = UniTask.Create(async (cancellationToken) =>
                    {
                        await UniTask.WaitForSeconds(maxTime - item.Item1);
                        await item.Item2;
                    }, cancellationToken);
                uniTasks.Add(a);
            }
            await UniTask.WhenAll(uniTasks);
        }
    }
}
