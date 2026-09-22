
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TuanTool
{
    public static class ExtensionMethod
    {
        public static void DeleteAllChild(this GameObject obj)
        {
            DeleteAllChild(obj, obj);
        }
        /// <summary>
        /// Checks if a GameObject has been destroyed.
        /// </summary>
        /// <param name="gameObject">GameObject reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }

        public static void DeleteAllChild(this Transform transform, Transform exclusive)
        {

            DeleteAllChild(transform.gameObject, exclusive.gameObject);
        }

        public static void DeleteAllChild(this Transform transform)
        {

            DeleteAllChild(transform.gameObject);
        }


        public static void DeleteAllChild(this Transform transform, List<Transform> exclusive)
        {

            DeleteAllChild(transform.gameObject, exclusive.ConvertAll(_ => _.gameObject).ToList());
        }

        public static void DeleteAllChild(this GameObject obj, GameObject exclusive)
        {
            var childrens = new List<GameObject>();
            foreach (Transform child in obj.transform)
                childrens.Add(child.gameObject);

            foreach (var item in childrens)
            {
                if (item != exclusive)
                    if (Application.isPlaying)
                        Object.Destroy(item.gameObject);
                    else
                        Object.DestroyImmediate(item.gameObject);
            }


        }

        public static void DeleteAllChild(this GameObject obj, List<GameObject> exclusive)
        {

            var childrens = new List<GameObject>();
            foreach (Transform child in obj.transform)
                childrens.Add(child.gameObject);

            foreach (var item in childrens)
            {
                if (!exclusive.Contains(item))
                    if (Application.isPlaying)
                        Object.Destroy(item.gameObject);
                    else
                        Object.DestroyImmediate(item.gameObject);
            }
        }


        public static int FindIndex<T>(this List<T> list, T obj)
        {
            int j = -1;
            for (int i = 0; i < list.Count; i++)
            {
                //if (EqualityComparer<T>.Default.Equals(list[i], obj))
                if (ReferenceEquals(list[i], obj))
                {
                    j = i;
                    break;
                }
            }

            if (j != -1)
                return j;
            else
            {
                Debug.LogError("Khong co phan tu trong mang!");
                return -1;
            }
        }

        // public static System.IObservable<bool> IsAliveAsObservable(this ParticleSystem particle, bool whereBool)
        // {
        //     return particle.ObserveEveryValueChanged(_ => _.IsAlive(true) && particle != null).Where(_ => _ == whereBool);
        // }

    }
}