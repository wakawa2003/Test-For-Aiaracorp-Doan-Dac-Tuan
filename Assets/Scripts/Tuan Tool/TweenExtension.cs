using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
namespace TuanTool
{
    public class TweenExtension : MonoBehaviour
    {
        List<Tween> tweensWhenDestroy = new List<Tween>();
        List<Tween> tweensWhenDisable = new List<Tween>();

        public void AddTweenKillOnDestory(Tween tween)
        {
            tweensWhenDestroy.Add(tween);
        }

        public void AddTweenKillOnDisable(Tween tween)
        {
            tweensWhenDisable.Add(tween);
        }

        private void OnDestroy()
        {
            foreach (var t in tweensWhenDestroy)
            {
                t.Kill();
            }
        }

        private void OnDisable()
        {

            foreach (var t in tweensWhenDisable)
            {
                t.Kill();
            }
        }
    }

    public static class TweenExt
    {


        static TweenExtension getTweenExtension(MonoBehaviour mono)
        {

            return getTweenExtension(mono.transform);
        }

        static TweenExtension getTweenExtension(Transform obj)
        {
            TweenExtension tweenExtension = obj.gameObject.GetComponent<TweenExtension>();
            if (tweenExtension == null)
                tweenExtension = obj.gameObject.AddComponent<TweenExtension>();
            return tweenExtension;
        }

        public static Tween KillOnCancellationToken(this Tween tween, CancellationToken destroyCancellationToken)
        {
            destroyCancellationToken.Register(() => tween?.Kill(false));
            return tween;
        }
        public static Sequence KillOnCancellationToken(this Sequence seq, CancellationToken destroyCancellationToken)
        {
            destroyCancellationToken.Register(() => seq?.Kill(false));
            return seq;
        }
        public static Tween KillOnDestroy(this Tween tween, MonoBehaviour mono)
        {
            getTweenExtension(mono).AddTweenKillOnDestory(tween);
            return tween;
        }

        public static Tween KillOnDisable(this Tween tween, MonoBehaviour mono)
        {
            getTweenExtension(mono).AddTweenKillOnDisable(tween);
            return tween;
        }

        public static Tween KillOnDestroy(this Tween tween, Transform mono)
        {
            getTweenExtension(mono).AddTweenKillOnDestory(tween);
            return tween;
        }

        public static Tween KillOnDisable(this Tween tween, Transform mono)
        {
            getTweenExtension(mono).AddTweenKillOnDisable(tween);
            return tween;
        }


        public static Tween KillOnDestroy(this Tween tween, GameObject mono)
        {
            getTweenExtension(mono.transform).AddTweenKillOnDestory(tween);
            return tween;
        }

        public static Tween KillOnDisable(this Tween tween, GameObject mono)
        {
            getTweenExtension(mono.transform).AddTweenKillOnDisable(tween);
            return tween;
        }
    }
}