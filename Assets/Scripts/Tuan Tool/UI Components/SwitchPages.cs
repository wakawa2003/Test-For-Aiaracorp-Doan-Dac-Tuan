using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening; // Yêu cầu cài đặt DOTween
using UnityEngine.UI;
namespace TuanTool
{

    /// <summary>
    /// Component này dùng để quản lý việc chuyển đổi giữa các trang UI (Switch Pages) với hiệu ứng trượt mượt mà.
    /// </summary>
    [Tooltip("Component này dùng để quản lý việc chuyển đổi giữa các trang UI (Switch Pages) với hiệu ứng trượt mượt mà.")]
    [AddComponentMenu("UI/Switch Pages")]
    public class SwitchPages : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private float animDuration = 0.4f;
        [SerializeField] private Ease animEase = Ease.OutQuint;

        [Title("Pages Setup")]
        [SerializeField] private RectTransform[] pages;
        [SerializeField] private Selectable[] togglesButton;

        [SerializeField] private int currentPageIndex = 0;

        Sequence sequenceMove;

        public int CurrentPageIndex { get => currentPageIndex; private set => currentPageIndex = value; }

        private void Start()
        {
            // Khởi tạo: Chỉ bật trang đầu tiên, các trang khác tắt đi
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].gameObject.SetActive(i == CurrentPageIndex);
                if (i == CurrentPageIndex) pages[i].anchoredPosition = Vector2.zero;
            }
        }

        [Button(ButtonSizes.Large)]
        public void ShowPage(int indexPage)
        {
            // Kiểm tra lỗi hoặc nếu nhấn vào đúng trang đang hiển thị
            if (indexPage < 0 || indexPage >= pages.Length || indexPage == CurrentPageIndex) return;
            if (sequenceMove != null && sequenceMove.IsActive() && sequenceMove.IsPlaying()) return;

            RectTransform oldPage = pages[CurrentPageIndex];
            RectTransform newPage = pages[indexPage];

            // Hướng: Nếu index mới lớn hơn -> Trang mới trượt từ PHẢI qua TRÁI
            bool isNext = indexPage > CurrentPageIndex;
            float screenWidth = oldPage.rect.width; // Lấy chiều rộng của chính UI

            // 1. Chuẩn bị trang mới: Bật lên và đặt ở vị trí "chờ" bên ngoài màn hình
            newPage.gameObject.SetActive(true);
            float startX = isNext ? screenWidth : -screenWidth;
            newPage.anchoredPosition = new Vector2(startX, 0);

            sequenceMove = DOTween.Sequence();

            // 2. Chạy Animation cho trang cũ trượt ra ngoài
            float exitX = isNext ? -screenWidth : screenWidth;
            var anim1 = oldPage.DOAnchorPos(new Vector2(exitX, 0), animDuration)
                  .SetEase(animEase)
                  .OnComplete(() =>
                  {
                      oldPage.gameObject.SetActive(false);
                  });

            // 3. Chạy Animation cho trang mới trượt vào tâm (0,0)
            var anim2 = newPage.DOAnchorPos(Vector2.zero, animDuration)
                 .SetEase(animEase);

            sequenceMove.Append(anim1);
            sequenceMove.Join(anim2);
            // Cập nhật index hiện tại
            CurrentPageIndex = indexPage;

        }

        void Update()
        {
            for (int i = 0; i < togglesButton.Length; i++)
            {
                var item = togglesButton[i];
                item.interactable = i != CurrentPageIndex; // Cập nhật trạng thái Toggle tương ứng
            }
        }

        #region Helper Methods (Dùng cho các nút bấm UI đơn lẻ)
        [HorizontalGroup("Controls")]
        [Button]
        public void PreviousPage() => ShowPage(CurrentPageIndex - 1);

        [HorizontalGroup("Controls")]
        [Button]
        public void NextPage() => ShowPage(CurrentPageIndex + 1);


        #endregion
    }
}
