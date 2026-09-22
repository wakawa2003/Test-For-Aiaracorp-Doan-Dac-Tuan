using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TuanTool
{
    public class AssetReferenceReplacer : EditorWindow
    {
        Object assetA;
        Object assetB;

        Dictionary<string, int> fileStats = new Dictionary<string, int>();
        int totalReplaced = 0;

        [MenuItem("Tools/Thay thế Asset Reference toàn bộ")]
        static void Init()
        {
            GetWindow<AssetReferenceReplacer>("Thay thế Asset toàn bộ");
        }

        void OnGUI()
        {
            GUILayout.Label("Thay thế tất cả reference từ Asset A sang Asset B", EditorStyles.boldLabel);

            assetA = EditorGUILayout.ObjectField("Asset A (cũ)", assetA, typeof(Object), false);
            assetB = EditorGUILayout.ObjectField("Asset B (mới)", assetB, typeof(Object), false);

            if (GUILayout.Button("Thực hiện thay thế"))
            {
                fileStats.Clear();
                totalReplaced = 0;

                try
                {
                    ReplaceInAssets();
                    ReplaceInScenes();
                    Debug.Log($"✅ Tổng số thay thế: {totalReplaced}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Lỗi trong quá trình thay thế: {ex.Message}");
                }
            }

            if (fileStats.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"📊 Thống kê:", EditorStyles.boldLabel);
                foreach (var kvp in fileStats)
                {
                    GUILayout.Label($"{kvp.Key}: {kvp.Value} lần");
                }
            }
        }

        void ReplaceInAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:Object");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".unity")) continue;

                Object[] objs;
                try
                {
                    objs = AssetDatabase.LoadAllAssetsAtPath(path);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"⚠️ Không thể load asset tại {path}: {ex.Message}");
                    continue;
                }

                int replacedInFile = 0;
                bool assetDirty = false;

                foreach (Object obj in objs)
                {
                    if (obj == null) continue;

                    try
                    {
                        SerializedObject so = new SerializedObject(obj);
                        SerializedProperty prop = so.GetIterator();

                        while (prop.NextVisible(true))
                        {
                            if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                                prop.objectReferenceValue == assetA)
                            {
                                prop.objectReferenceValue = assetB;
                                so.ApplyModifiedProperties();
                                EditorUtility.SetDirty(obj);
                                replacedInFile++;
                                totalReplaced++;
                                assetDirty = true;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"⚠️ Lỗi khi xử lý object trong {path}: {ex.Message}");
                        continue;
                    }
                }

                if (replacedInFile > 0)
                    fileStats[path] = replacedInFile;
            }
            AssetDatabase.SaveAssets();
        }

        void ReplaceInScenes()
        {
            string[] scenePaths = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
            foreach (string scenePath in scenePaths)
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneAsset == null) continue;

                Scene scene;
                try
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"⚠️ Không thể mở scene {scenePath}: {ex.Message}");
                    continue;
                }

                int replacedInScene = 0;

                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject go in rootObjects)
                {
                    Component[] components = go.GetComponentsInChildren<Component>(true);
                    foreach (Component comp in components)
                    {
                        if (comp == null) continue;

                        try
                        {
                            SerializedObject so = new SerializedObject(comp);
                            SerializedProperty prop = so.GetIterator();

                            while (prop.NextVisible(true))
                            {
                                if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                                    prop.objectReferenceValue == assetA)
                                {
                                    prop.objectReferenceValue = assetB;
                                    so.ApplyModifiedProperties();
                                    replacedInScene++;
                                    totalReplaced++;
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"⚠️ Lỗi khi xử lý component trong scene {scenePath}: {ex.Message}");
                            continue;
                        }
                    }
                }

                if (replacedInScene > 0)
                {
                    fileStats[scenePath] = replacedInScene;
                    try
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"⚠️ Không thể lưu scene {scenePath}: {ex.Message}");
                    }
                }
            }
        }
    }
}