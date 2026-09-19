//#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;


namespace Tuan.UtilitiesEditor
{
    public class GameAssetProcessor : AssetPostprocessor
    {
        bool isDirty = false;
        private void OnPostprocessModel(GameObject gameObject)
        {
            return;
            isDirty = false;
            if (gameObject.name.StartsWith("~") || assetPath.Contains("common-lib"))//bo qua textures bat dau = "~"
                //if (texture.name.StartsWith("~"))//bo qua textures bat dau = "~"
                return;
            var a = assetImporter as ModelImporter;
            if (a.meshCompression != ModelImporterMeshCompression.High)
            {
                a.meshCompression = ModelImporterMeshCompression.High;
                isDirty = true;
            }
            if (isDirty)
                EditorCoroutineUtility.StartCoroutine(IESave(), a);
            IEnumerator IESave()
            {
                yield return null;
                Debug.Log($"import model: {a.assetPath}");
                assetImporter.SaveAndReimport();
                AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(assetPath));
            }

        }

        [MenuItem("Tools/Reimport Textures")]
        static async void ReimportTextures()
        {
            var startTime = System.DateTime.Now;
            AssetDatabase.StartAssetEditing();
            int countCompleted = 0;
            try
            {
                // Lấy đối tượng đang được chọn trong Project
                List<Task> listTask = new List<Task>();
                Object[] selectedObjects = Selection.objects;
                if (selectedObjects.Length > 0)
                {
                    foreach (Object obj in selectedObjects)
                    {
                        // Lấy đường dẫn tới asset
                        string path = AssetDatabase.GetAssetPath(obj);

                        // Kiểm tra xem asset có phải là Texture không
                        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (textureImporter != null)
                        {
                            Debug.Log($"Texture Importer settings for {obj.name}:");
                            Debug.Log($"Path: {path}");
                            Debug.Log($"Texture Type: {textureImporter.textureType}");
                            Debug.Log($"Wrap Mode: {textureImporter.wrapMode}");
                            Debug.Log($"Filter Mode: {textureImporter.filterMode}");
                            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                            listTask.Add(_exe(texture, textureImporter));

                        }
                        else
                        {
                            Debug.LogWarning($"The asset '{obj.name}' at path '{path}' is not a texture!");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No assets are selected!");
                }
                await Task.WhenAll(listTask);

                async Task _exe(Texture2D texture, AssetImporter assetImporter)
                {
                    await Execute(texture, assetImporter);
                    countCompleted++;

                }

            }
            finally
            {

                AssetDatabase.StopAssetEditing();
                Debug.Log($"Hoàn thành import: {(System.DateTime.Now - startTime).TotalSeconds}");
            }
        }

        //private void OnPostprocessTexture(Texture2D texture)
        //{
        //    //Execute(texture, assetImporter);

        //}
        static async Task Execute(Texture2D texture, AssetImporter assetImporter)
        {

            //return;
            //Debug.Log($"sovle texxxx");
            var a = assetImporter as TextureImporter;

            //Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetImporter.assetPath);
            var path = assetImporter.assetPath;

            //return;
            //if (texture.name.StartsWith("~") || a.assetPath.Contains("common-lib"))//bo qua textures bat dau = "~"
            //if (texture.name.StartsWith("~"))//bo qua textures bat dau = "~"
            //return;


            //if (!powerOfTwo(texture.width) || !powerOfTwo(texture.height))
            //{
            //    var path = a.assetPath.Split('/').ToList();
            //    //if (path[0] != "Packages")
            //    //    if (!path.Any(_ => _.Contains("Editor")) && !path.Contains("GUI"))
            //    Debug.LogError($"Texture {a.assetPath} is not Power Of Two!");
            //    // Cấu hình riêng cho Android
            //    a.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            //    {
            //        name = "Android",               // Nền tảng
            //        maxTextureSize = a.maxTextureSize,
            //        overridden = false,              // Sử dụng cài đặt riêng
            //        format = TextureImporterFormat.Automatic, // Định dạng nén
            //        compressionQuality = 100
            //    });
            //}
            //else
            //{
            //    a.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            //    {
            //        maxTextureSize = a.maxTextureSize,
            //        name = "Android",               // Nền tảng
            //        overridden = true,              // Sử dụng cài đặt riêng
            //        format = TextureImporterFormat.DXT5Crunched, // Định dạng nén mhỏ nhất cho hình ảnh pot.
            //        compressionQuality = 100
            //    });
            //}
            //a.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            //{
            //    name = "Android",               // Nền tảng
            //    overridden = false,              // Sử dụng cài đặt riêng
            //});
            //a.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            //{
            //    //maxTextureSize = a.maxTextureSize,
            //    name = "Android",               // Nền tảng
            //    overridden = true,              // Sử dụng cài đặt riêng
            //    format = TextureImporterFormat.ASTC_12x12, // Định dạng nén mhỏ nhất cho hình ảnh pot.
            //    compressionQuality = 100
            //});

            var mipmapEnabled = false;
            var textureCompression = TextureImporterCompression.CompressedLQ;
            var alphaIsTransparency = true;
            var compressionQuality = 100;//khong nen de la 50% => bi banding textures
            var crunchedCompression = true;//can phai xep o cuoi

            if (a.mipmapEnabled != mipmapEnabled)
            {

                a.mipmapEnabled = mipmapEnabled;
            }


            if (a.textureCompression != textureCompression)
            {
                a.textureCompression = textureCompression;
            }


            if (a.alphaIsTransparency != alphaIsTransparency)
            {
                a.alphaIsTransparency = alphaIsTransparency;
            }

            if (a.compressionQuality != compressionQuality)
            {
                a.compressionQuality = compressionQuality;
            }

            if (a.crunchedCompression != crunchedCompression)
            {
                a.crunchedCompression = crunchedCompression;
            }

            Debug.Log($"import texture: {a.assetPath}");

            EditorCoroutineUtility.StartCoroutineOwnerless(IEImport());

            bool isCOmplte = false;
            while (!isCOmplte)
            {
                await Task.Delay(100);
            }
            IEnumerator IEImport()
            {
                for (int i = 0; i < 10; i++)
                    yield return null;
                assetImporter.SaveAndReimport();
                for (int i = 0; i < 10; i++)
                    yield return null;
                isCOmplte = true;
            }
        }
        static bool powerOfTwo(int n)
        {
            if (n == 0) { return false; }
            while (n != 1)
            {
                n = n / 2;
                if (n % 2 != 0 && n != 1) { return false; }
            }
            return true;
        }

    }
}
//#endif