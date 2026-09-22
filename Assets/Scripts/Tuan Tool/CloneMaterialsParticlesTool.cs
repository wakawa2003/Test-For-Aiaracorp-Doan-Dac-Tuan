
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace TuanTool
{
    public class CloneMaterialsParticlesTool : MonoBehaviour
    {
        [SerializeField] Transform particlesRoot;
#if UNITY_EDITOR
        [SerializeField][FolderPath] string folderMaterial;

        [Button]
        void CloneParticleMaterial()
        {

            ParticleSystem[] particles = particlesRoot.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < particles.Count(); i++)
            {
                var item = particles[i];
                var r = item.GetComponent<ParticleSystemRenderer>();
                Material newmat = new Material(r.sharedMaterial);
                var path = $"{folderMaterial}/{newmat.name}.mat";
                AssetDatabase.CreateAsset(newmat, path);
                AssetDatabase.SaveAssets();

                EditorCoroutineUtility.StartCoroutine(IEFix(), this);

                //fix khong gan dc refernce vao object
                IEnumerator IEFix()
                {
                    while (r.sharedMaterial != AssetDatabase.LoadMainAssetAtPath(path) as Material)
                    {
                        var asset = AssetDatabase.LoadMainAssetAtPath(path) as Material;
                        //r.materials = new Material[] { asset };\
                        r.sharedMaterial = asset;

                        EditorUtility.SetDirty(r);

                        var objs = Selection.objects.ToList();
                        objs.Add(asset);
                        Selection.objects = objs.ToArray();

                        yield return null;
                    }
                }
            }

        }
#endif

    }
}