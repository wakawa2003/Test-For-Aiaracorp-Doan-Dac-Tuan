#if UNITY_EDITOR 
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace TuanTool.Popup
{
    [InitializeOnLoad]
    class PopupIconEditor
    {
        static Texture2D texturesOn;
        static Texture2D texturesOff;
        static List<int> markedObjects;

        static PopupIconEditor()
        {
            // Init
            //texturesOn = AssetDatabase.LoadAssetAtPath("Assets/Scripts/Popup/Btn on.PNG", typeof(Texture2D)) as Texture2D;
            //texturesOff = AssetDatabase.LoadAssetAtPath("Assets/Scripts/Popup/Btn off.png", typeof(Texture2D)) as Texture2D;

            //texturesOn = new Texture2D(24, 24);
            //texturesOff = new Texture2D(24, 24);
            //for (int y = 0; y < texturesOn.height; y++)
            //{ 
            //    for (int x = 0; x < texturesOn.width; x++)
            //    {

            //        texturesOn.SetPixel(x, y, Color.green);
            //        texturesOff.SetPixel(x, y, Color.red);

            //    }
            //}

            //texturesOn.Apply();
            //texturesOff.Apply();

            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;

        }

        static void HierarchyItemCB(int instanceID, Rect selectionRect)
        {
            //Debug.Log("aaaa");
            // place the icon to the right of the list:
            Rect r = new Rect(selectionRect);
            r.x = r.width + 30;
            r.width = 30;
            //Debug.Log("instanceID: " + instanceID);
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            var popup = go?.GetComponent<Popup>();
            if (popup)
            {
                popup.gameObject.SetActive(true);
                var style = new GUIStyle();
                style.richText = true;
                style.alignment = TextAnchor.MiddleCenter;
                Color prevColor = GUI.color;
                if (popup.IsShowing)
                {

                    GUI.color = Color.green;

                    var guiContent = new GUIContent("On");
                    //guiContent.image = texturesOn;
                    if (GUI.Button(r, guiContent))
                        popup.Hide();
                }
                else
                {
                    GUI.color = Color.red;
                    var guiContent = new GUIContent("Off");
                    //guiContent.image = texturesOff;
                    if (GUI.Button(r, guiContent))
                        popup.RequestShow();
                }
                GUI.color = prevColor;
            }
        }
    }
}
#endif