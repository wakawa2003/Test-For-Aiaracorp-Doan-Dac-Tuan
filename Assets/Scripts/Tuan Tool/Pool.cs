
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace TuanTool
{
    public class Pool : MonoBehaviour, IPoolMannager
    {

        [System.Serializable]
        public class PoolSlot
        {
            public bool AutoInit = true;
            public string Name;
            [OnValueChanged(nameof(OnObjectChange))]
            public GameObject Object;
            [HideIf(nameof(checkHideAmount))] public int Amount = 10;
            [HideInInspector] public int hintIndex = 0;
            public List<GameObject> _poolQueue = new List<GameObject>();
            [SerializeField] public Transform Container;
            void OnObjectChange()
            {
                Name = Object.name;
            }
            bool checkHideAmount()
            {
                return !AutoInit;
            }
        }

        [SerializeField] Transform containerGlobal;
        [SerializeField] private List<PoolSlot> _poolSlots = new List<PoolSlot>();

        [SerializeField] private Dictionary<string, PoolSlot> _poolDict = new Dictionary<string, PoolSlot>();

        bool isInit = false;

        public List<PoolSlot> PoolSlots { get => _poolSlots; set => _poolSlots = value; }
        public Dictionary<string, PoolSlot> PoolDict { get => _poolDict; set => _poolDict = value; }

        public void Init()
        {
            if (!isInit)
            {
                for (int i = 0; i < PoolSlots.Count; i++)
                {
                    for (int j = 0; j < PoolSlots[i].Amount; j++)
                    {
                        var poolSlot = PoolSlots[i];
                        if (poolSlot.AutoInit)
                            AddNewGameObjectForSlot(poolSlot);
                        else
                        {
                            //init
                            foreach (var item in poolSlot._poolQueue)
                            {
                                InitForNewGameObject(item);
                            }
                        }
                    }
                    PoolDict.Add(PoolSlots[i].Name, PoolSlots[i]);
                }
                isInit = true;
            }
        }

        private GameObject AddNewGameObjectForSlot(PoolSlot poolSlot)
        {
            Transform parent = poolSlot.Container == null ? containerGlobal : poolSlot.Container;
            GameObject newGameObject = Instantiate(poolSlot.Object, parent);

            InitForNewGameObject(newGameObject);
            poolSlot._poolQueue.Add(newGameObject);
            InCreasePoolIndex(poolSlot);
            return newGameObject;
        }

        private void InitForNewGameObject(GameObject newGameObject)
        {
            //Debug.Log($"init ipoool");
            foreach (var item in newGameObject.GetComponentsInChildren<IPool>(true))
            {
                item.PoolMannagerOwner = this;
            }
            newGameObject.SetActive(false);
        }

        private void Start()
        {
            Init();

        }

        //[Button]
        //void TestSpawn(string s = "Sphere")
        //{
        //    var a = Instantiate(s, transform.position, Quaternion.identity);
        //}

        public GameObject Instantiate(string name, Vector3 pos, Quaternion quaternion)
        {
            var a = Instantiate(name, g =>
            {
                g.transform.SetPositionAndRotation(pos, quaternion);
            });
            return a;
        }

        public GameObject Instantiate(string name, Action<GameObject> onBeforeCallSpawnPool = null)
        {
            Init();
            if (!PoolDict.ContainsKey(name))
            {
                Debug.LogError("Pool ko co Key: " + name);
                return null;
            }
            var poolSLot = PoolDict[name];
            //GameObject gameObject1 = _poolDict[name]._poolQueue.Peek();
            GameObject gameObject1 = getNextObj(poolSLot);

            if (gameObject1 == null)
            {
                gameObject1 = AddNewGameObjectForSlot(PoolDict[name]);
                Debug.LogWarning($"Can't find: {name} inactive in pool => spawn new object!!");
            }


            gameObject1.SetActive(true);
            onBeforeCallSpawnPool?.Invoke(gameObject1);
            foreach (var item in gameObject1.GetComponentsInChildren<IPool>(true))
            {
                item.OnPoolSpawn();
            }
            //gameObject1.SendMessage("OnPoolSpawn", SendMessageOptions.DontRequireReceiver);

            return gameObject1;

            static GameObject getNextObj(PoolSlot poolSLot)
            {
                for (int i = 0; i < poolSLot._poolQueue.Count; i++)
                {
                    InCreasePoolIndex(poolSLot);
                    var obj = poolSLot._poolQueue[poolSLot.hintIndex];
                    if (!obj.activeSelf)
                        return obj;
                }

                return null;
            }
        }

        private static void InCreasePoolIndex(PoolSlot poolSLot)
        {
            poolSLot.hintIndex += 1;
            poolSLot.hintIndex = poolSLot.hintIndex % (poolSLot._poolQueue.Count);
        }

        public void UnSpawn(GameObject go)
        {
            Init();
            foreach (var item in go.GetComponentsInChildren<IPool>(true))
            {
                item.OnPoolUnSpawn();
            }
            //go.SendMessage("OnPoolUnSpawn", SendMessageOptions.DontRequireReceiver);
            go.SetActive(false);
        }

    }

    public interface IPoolMannager
    {
        GameObject Instantiate(string name, Vector3 pos, Quaternion quaternion);
        public void UnSpawn(GameObject go);
    }

    public interface IPool
    {
        GameObject gameObject { get; }
        IPoolMannager PoolMannagerOwner { get; set; }
        //public void SetPoolManager(IPoolMannager poolMannager) { PoolMannagerOwner = poolMannager; Debug.Log($"set pool ne!!"); }
        void UnSpawn() { PoolMannagerOwner.UnSpawn(gameObject); }
        void OnPoolSpawn();
        void OnPoolUnSpawn();
    }

}