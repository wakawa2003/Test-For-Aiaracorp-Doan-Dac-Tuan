
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public class PlaySceneEditor
{
#if UNITY_EDITOR
    private const string PREVIOUS_SCENE_KEY = "PreviousScenePath";
    private const string ROOT_SCENE_PATH = "Assets/Scenes/Root.unity";


    [MenuItem("Developer Tools/Play From Start #p")] // Shortcut: Shift + p
    private static void PlayFromStart()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 1. Lưu lại đường dẫn Scene hiện tại vào bộ nhớ Editor
            string currentScene = EditorSceneManager.GetActiveScene().path;
            EditorPrefs.SetString(PREVIOUS_SCENE_KEY, currentScene);

            // 2. Mở Scene Root và chạy
            EditorSceneManager.OpenScene(ROOT_SCENE_PATH);
            EditorApplication.isPlaying = true;
        }
    }

    // Hàm này sẽ tự động chạy mỗi khi trạng thái Editor thay đổi (Play/Stop)
    [InitializeOnLoadMethod]
    private static void MonitorPlayModeState()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Khi người dùng bấm Stop (quay lại chế độ Edit)
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Kiểm tra xem trước đó có lưu Scene nào không
            if (EditorPrefs.HasKey(PREVIOUS_SCENE_KEY))
            {
                string previousScene = EditorPrefs.GetString(PREVIOUS_SCENE_KEY);

                // Nếu Scene cũ khác Scene hiện tại (Root) thì mới mở lại
                if (!string.IsNullOrEmpty(previousScene) && previousScene != EditorSceneManager.GetActiveScene().path)
                {
                    EditorSceneManager.OpenScene(previousScene);
                }

                // Xóa key sau khi đã quay lại để tránh nhảy Scene lung tung lần sau
                EditorPrefs.DeleteKey(PREVIOUS_SCENE_KEY);
            }
        }
    }
#endif
}