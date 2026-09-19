using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tuan.UtilitiesEditor
{
    public class CreatFolderEditor
    {
        const string menu = "Create/";

        [MenuItem(menu + "Create folder Prefabs")]
        public static void CreateFolderPrefabs()
        {
            TryCreatFolder("Prefabs");
        }

        [MenuItem(menu + "Create folder Animations")]
        public static void CreateFolderAnimations()
        {
            TryCreatFolder("Animations");
        }

        [MenuItem(menu + "Create folder Scripts")]
        public static void CreateFolderScripts()
        {
            TryCreatFolder("Scripts");
        }

        [MenuItem(menu + "Create folder Materials")]
        public static void CreateFolderMaterials()
        {
            TryCreatFolder("Materials");
        }


        [MenuItem(menu + "Create folder Textures")]
        public static void CreateFolderTextures()
        {
            TryCreatFolder("Textures");
        }

        [MenuItem(menu + "Create folder Textures + Materials")]
        public static void CreateFolderTexturesAndMaterials()
        {
            CreateFolderMaterials();
            CreateFolderTextures();
        }




        private static void TryCreatFolder(string name)
        {
            var currentFolder = GetSelectedPathOrFallback();
            if (!AssetDatabase.IsValidFolder(currentFolder + "/" + name))
                AssetDatabase.CreateFolder(currentFolder, name);
        }

        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }

}