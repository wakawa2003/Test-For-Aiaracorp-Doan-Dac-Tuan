using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using Sirenix.OdinInspector;
namespace TuanTool.Popup
{
    public class PopupManager : MonoBehaviour

    {
        public Popup Currentpopup;



        public List<Popup> popups = new List<Popup>();

        #region Singleton
        private static PopupManager ins;
        public static PopupManager Ins
        {
            get
            {
                if (ins == null)
                {
                    var a = FindObjectOfType<PopupManager>();
                    a?.Awake();
                    if (a == null)
                    {
                        Debug.Log("Creat new PopupManager!!!");
                        GameObject popup = new GameObject(nameof(PopupManager));
                        popup.AddComponent<PopupManager>().Awake();
                    }
                }
                return ins;
            }
            set => ins = value;
        }
        #endregion

        private void Awake()
        {

            if (ins == null)
                ins = this;
            else
            {
                if (ins != this)
                    Destroy(gameObject);
                return;
            }
        }



        public void Process()

        {

            if (popups.Count > 0)

            {

                if (!popups[0].IsShowing)

                {

                    popups[0].Show(null);

                    Currentpopup = popups[0];

                }

            }

            else

                Currentpopup = null;



        }



        public void AddPopup(Popup popup)

        {

            if (!popups.Contains(popup))

            {

                popups.Add(popup);

                Process();

            }

        }



        public void RemovePopup(Popup popup)

        {

            if (popups.Contains(popup))

            {

                popups.Remove(popup);

                Process();

            }

        }

    }
}