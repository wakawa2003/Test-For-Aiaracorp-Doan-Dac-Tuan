using System.Collections.Generic;
using System.Linq;
using R3.Triggers;
using UnityEngine;
using R3;

namespace MyGameNamespace
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private Transform focusPoint;
        [SerializeField] public List<Transform> listFocusPoint;
        [SerializeField] private Transform posSpawnEnemy;
        [SerializeField] private Collider colliderSpawnEnemy;
        [SerializeField] private Transform EnemyPrefabs;
        Transform EnemyCurrent;

        #region Singleton
        private static GameController ins;
        public static GameController Ins
        {
            get
            {
                if (ins == null)
                {
                    var a = FindObjectOfType<GameController>();
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


        void Start()
        {
            colliderSpawnEnemy.OnTriggerEnterAsObservable().Subscribe(delegate
            {
                if (EnemyCurrent == null)
                {
                    var a = Instantiate(EnemyPrefabs, posSpawnEnemy.position, Quaternion.identity);
                    EnemyCurrent = a.transform;
                }
            });
        }

        void Update()
        {
            Vector3 p = Vector3.zero;
            foreach (var item in listFocusPoint)
            {
                p += item.position;
            }
            p = p / listFocusPoint.Count();
            focusPoint.position = p;
        }
    }
}
