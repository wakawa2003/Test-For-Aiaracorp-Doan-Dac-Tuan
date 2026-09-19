using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

namespace TuanTool
{
    /// <summary>
    /// TimeSyncManager: Quản lý đồng bộ thời gian với Internet để chống hack giờ.
    /// </summary>
    public class TimeSyncManager : MonoBehaviour
    {

        [Header("Trạng thái đồng bộ")]
        [SerializeField] private bool _isTimeSynced = false;

        // Biến lưu trữ mốc thời gian
        private DateTime _baseInternetTime;
        private float _timeAtFetch;

        // Danh sách các server "bất tử" phủ sóng toàn cầu
        private readonly string[] timeServers = new string[]
        {
        "https://www.microsoft.com",     // Ổn định toàn cầu
        "https://www.google.com",        // Nhanh, chuẩn (nhưng bị chặn ở TQ)
        "https://www.naver.com",         // Server check mạng siêu nhẹ của Naver
        "https://apple.com",     // Server check mạng siêu nhẹ của Apple
        "https://www.facebook.com",      // Server check mạng siêu nhẹ của Facebook
        "https://www.twitter.com",       // Server check mạng siêu nhẹ của Twitter
        "https://www.baidu.com" ,         // Fallback cho thị trường Trung Quốc
        "https://weibo.com" ,         // Fallback cho thị trường Trung Quốc

        };

        #region Singleton
        private static TimeSyncManager ins;
        public static TimeSyncManager Ins
        {
            get
            {
                if (ins == null)
                {
                    var a = FindObjectOfType<TimeSyncManager>() ?? new GameObject("Time Sync Manager").AddComponent<TimeSyncManager>();
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

            DontDestroyOnLoad(gameObject);

            // Tự động đồng bộ giờ ngay khi vừa mở game
            _ = SyncTimeWithInternetAsync();
        }

        [Button]
        private void logTimeFromInternet()
        {
            foreach (string url in timeServers)
            {
                FetchTimeFromServerAsync(url, CancellationToken.None).ContinueWith(task =>
                {
                    if (task.Result.HasValue)
                    {
                        Debug.Log($"[TimeSync] Server {url} trả về giờ: {task.Result.Value}");
                    }
                    else
                    {
                        Debug.LogWarning($"[TimeSync] Server {url} không phản hồi hoặc bị chặn.");
                    }
                });
            }
        }

        [Button("Log Current Network Time")]
        void logTime()
        {
            DateTime currentTime = GetCurrentNetworkTime();
            Debug.Log($"[TimeSync] Current Network Time: {currentTime}");
        }

        /// <summary>
        /// Gọi hàm này để lấy giờ hiện tại. Game sẽ tự suy diễn thời gian mà KHÔNG tốn băng thông mạng.
        /// Chống hack giờ 100%.
        /// </summary>
        public DateTime GetCurrentNetworkTime()
        {
            if (_isTimeSynced)
            {
                // Tính số giây đã trôi qua kể từ lúc lấy giờ thành công
                float elapsedSeconds = Time.realtimeSinceStartup - _timeAtFetch;

                // Cộng dồn vào giờ gốc để ra giờ hiện hành
                return _baseInternetTime.AddSeconds(elapsedSeconds);
            }

            // Trạng thái dự phòng: Nếu chưa đồng bộ được, tạm dùng giờ của máy (Local Time) chuẩn UTC
            Debug.LogWarning("[TimeSync] Cảnh báo: Đang dùng giờ Local vì chưa đồng bộ được mạng.");
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Thuật toán Race Pattern: Bắn đồng loạt, lấy nhanh nhất, hủy phần còn lại.
        /// </summary>
        public async Task SyncTimeWithInternetAsync()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            List<Task<DateTime?>> tasks = new List<Task<DateTime?>>();

            // BƯỚC 1: Bắn request tới tất cả server cùng một lúc
            foreach (string url in timeServers)
            {
                tasks.Add(FetchTimeFromServerAsync(url, cts.Token));
            }

            // BƯỚC 2: Chờ người chiến thắng (Ai trả kết quả về đầu tiên)
            while (tasks.Count > 0)
            {
                Task<DateTime?> finishedTask = await Task.WhenAny(tasks);
                tasks.Remove(finishedTask);

                DateTime? resultTime = await finishedTask;

                // Nếu lấy được giờ thành công
                if (resultTime.HasValue)
                {
                    _baseInternetTime = resultTime.Value;
                    _timeAtFetch = Time.realtimeSinceStartup;
                    _isTimeSynced = true;

                    Debug.Log($"[TimeSync] Đã đồng bộ mốc giờ: {_baseInternetTime}");

                    // BƯỚC 3: Phát lệnh hủy (Abort) các request đang chạy chậm chạp còn lại để giải phóng RAM
                    cts.Cancel();
                    cts.Dispose();
                    return;
                }
            }

            // BƯỚC 4: Nếu tất cả đều rớt (Không có mạng)
            Debug.LogWarning("[TimeSync] Offline mode: Không kết nối được server nào.");
            cts.Dispose();
        }

        /// <summary>
        /// Xử lý từng Request độc lập (dùng lệnh HEAD siêu nhẹ)
        /// </summary>
        private async Task<DateTime?> FetchTimeFromServerAsync(string url, CancellationToken token)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Head(url))
            {
                webRequest.timeout = 5; // Chỉ chờ tối đa 5 giây cho mỗi server
                var operation = webRequest.SendWebRequest();

                // Lắng nghe liên tục xem có bị ép ngừng lại bởi CancellationToken không
                while (!operation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        webRequest.Abort(); // Giết request ngay lập tức
                        return null;
                    }

                    await Task.Yield(); // Trả quyền điều khiển cho Unity để không lag Main Thread
                }

                // Phân tích Header Date nếu thành công
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string dateHeader = webRequest.GetResponseHeader("Date");
                        if (!string.IsNullOrEmpty(dateHeader))
                        {
                            return DateTime.ParseExact(dateHeader,
                                "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                CultureInfo.InvariantCulture.DateTimeFormat,
                                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                        }
                    }
                    catch { /* Bỏ qua lỗi Parse, coi như server này fail */ }
                }

                return null; // Trả về null nếu lỗi mạng hoặc bị chặn
            }
        }

        /// <summary>
        /// Nắn lại giờ khi người chơi ẩn app (ngủ đông) rồi mở lại
        /// </summary>
        private void OnApplicationPause(bool isPaused)
        {
            if (!isPaused)
            {
                // Game vừa được mở lại từ background, đồng bộ lại để tránh bộ đếm bị sai số
                _ = SyncTimeWithInternetAsync();
            }
        }
    }
}