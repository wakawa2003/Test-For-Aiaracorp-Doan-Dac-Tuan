using System;
using UnityEngine;
using UnityEngine.Localization;

namespace TuanTool
{
    /// <summary>
    /// Dùng (Use()) đến số lần tối đa, 
    ///  đến 00:00 UTC hằng ngày sẽ reset về MaximumUse.
    /// </summary>
    [System.Serializable]
    public class UsageRegeneratorDaily
    {
        private string saveKeyRemaining;
        private string saveKeyLastDepleted;

        public int MaximumUse { get; private set; }
        private Func<DateTime> getCurrentTimeUTC;
        private ISaveLoad saveLoad;

        public UsageRegeneratorDaily(ISaveLoad saveLoadSystem, string saveKeyRemaining, string saveKeyLastDepleted, int maximumUse, Func<DateTime> getCurrentTimeUTC)
        {
            if (saveKeyLastDepleted == saveKeyRemaining)
                Debug.LogError("2 key không được giống nhau!!!");

            this.MaximumUse = maximumUse;
            this.getCurrentTimeUTC = getCurrentTimeUTC;
            this.saveLoad = saveLoadSystem;
            this.saveKeyRemaining = saveKeyRemaining;
            this.saveKeyLastDepleted = saveKeyLastDepleted;

            // init nếu chưa có
            if (!saveLoadSystem.HasKey(this.saveKeyRemaining))
                RemainingUse = maximumUse;
            if (!saveLoadSystem.HasKey(this.saveKeyLastDepleted))
                LastDepletedTime = GetPreviousResetTime();
        }

        public int RemainingUse
        {
            get => saveLoad.GetInt(saveKeyRemaining, MaximumUse);
            private set
            {
                saveLoad.SetInt(saveKeyRemaining, value);
                saveLoad.Save();
            }
        }
        public DateTime LastDepletedTime
        {
            get
            {
                var s = saveLoad.GetString(saveKeyLastDepleted, GetPreviousResetTime().ToFileTimeUtc().ToString());
                if (long.TryParse(s, out var fileTime))
                    return DateTime.FromFileTimeUtc(fileTime);
                Debug.LogError("Lỗi khi parse!!!");
                return GetPreviousResetTime();
            }
            private set
            {
                saveLoad.SetString(saveKeyLastDepleted, value.ToFileTimeUtc().ToString());
                saveLoad.Save();
            }
        }

        public void Refresh()
        {
            DateTime now = getCurrentTimeUTC.Invoke();
            DateTime nextReset = GetNextResetTimeFrom(LastDepletedTime);

            if (now >= nextReset)
            {
                // Tính số ngày đã qua kể từ lần reset cuối
                int daysPassed = (now.Date - LastDepletedTime.Date).Days;
                if (daysPassed > 0)
                {
                    ResetUses();
                    // cập nhật LastDepletedTime về mốc reset gần nhất
                    LastDepletedTime = LastDepletedTime.Date.AddDays(daysPassed);
                }
            }
        }


        public bool CanUse()
        {
            Refresh();
            return RemainingUse > 0;
        }

        public void Use()
        {
            if (!CanUse())
            {
                Debug.Log("Item chưa hồi, không thể dùng.");
                return;
            }

            RemainingUse--;
            if (RemainingUse <= 0)
            {
                Debug.Log("Đã dùng hết, chờ hồi vào 00:00 UTC.");
            }
        }

        public void ResetUses()
        {
            RemainingUse = MaximumUse;
            Debug.Log("Item đã hồi đầy.");
        }

        public string GetCooldownTimeString()
        {
            TimeSpan remain = GetCooldownTimeSpan();
            if (remain.TotalSeconds <= 0)
                return "00:00:00"; // đã hồi xong

            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                (int)remain.TotalHours,
                remain.Minutes,
                remain.Seconds);
        }

        public TimeSpan GetCooldownTimeSpan()
        {
            DateTime nextReset = GetNextResetTimeFrom(LastDepletedTime);
            return nextReset - getCurrentTimeUTC.Invoke();
        }

        /// <summary>
        /// Tính thời điểm reset tiếp theo (00:00 UTC) kể từ một mốc bất kỳ
        /// </summary>
        private DateTime GetNextResetTimeFrom(DateTime fromTime)
        {
            DateTime candidate = fromTime.Date; // 00:00 UTC của ngày đó
            if (fromTime >= candidate)
                return candidate.AddDays(1); // nếu đã qua 00:00 thì reset vào ngày mai
            return candidate;
        }

        /// <summary>
        /// Lấy thời điểm reset gần nhất (00:00 UTC hôm qua hoặc hôm nay)
        /// </summary>
        private DateTime GetPreviousResetTime()
        {
            DateTime now = getCurrentTimeUTC.Invoke();
            DateTime todayReset = now.Date; // 00:00 UTC hôm nay
            if (now >= todayReset)
                return todayReset;
            return todayReset.AddDays(-1);
        }
    }
}