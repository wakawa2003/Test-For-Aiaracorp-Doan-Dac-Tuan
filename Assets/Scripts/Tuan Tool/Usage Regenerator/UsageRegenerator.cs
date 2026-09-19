using System;
using UnityEngine;

namespace TuanTool.UsageRegenerator
{

    /// <summary>
    /// sau khi dùng (Use()) đến số lần tối đa sẽ chạy thời gian để reset theo resetDuration
    /// chỉ chạy resetduration khi RemainingUse <= 0
    /// </summary>
    [System.Serializable]
    public class UsageRegenerator
    {

        /// <summary>
        /// key con bao nhieu lần dùng
        /// </summary>
        private string saveKeyRemaining;

        /// <summary>
        /// key khi het thoi gian reset
        /// </summary>
        private string saveKeyLastDepleted;

        /// <summary>
        /// khoảng thời gian reset (ví dụ: 24h)
        /// </summary>
        private TimeSpan resetDuration;

        public int MaximumUse { get; private set; }
        Func<DateTime> getCurrentTimeUTC = () => DateTime.UtcNow;
        ISaveLoad saveLoad;

        public UsageRegenerator(ISaveLoad saveLoadSystem, string saveKeyRemaining, string saveKeyLastDepleted, int maximumUse, Func<DateTime> getCurrentTimeUTC, TimeSpan? resetDuration = null)
        {
            if (saveKeyLastDepleted == saveKeyRemaining)
                Debug.LogError($"2 key Khong duoc giong nhau!!!");

            this.MaximumUse = maximumUse;
            this.resetDuration = resetDuration ?? TimeSpan.FromHours(24); // mặc định 24h
            this.getCurrentTimeUTC = getCurrentTimeUTC;
            this.saveLoad = saveLoadSystem;
            this.saveKeyRemaining = saveKeyRemaining;
            this.saveKeyLastDepleted = saveKeyLastDepleted;

            // init nếu chưa có
            if (!saveLoadSystem.HasKey(this.saveKeyRemaining))
                RemainingUse = maximumUse;
            if (!saveLoadSystem.HasKey(this.saveKeyLastDepleted))
                LastDepletedTime = getCurrentTimeUTC.Invoke() - this.resetDuration;
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
                var s = saveLoad.GetString(saveKeyLastDepleted, (getCurrentTimeUTC.Invoke() - resetDuration).ToFileTimeUtc().ToString());
                if (long.TryParse(s, out var fileTime))
                    return DateTime.FromFileTimeUtc(fileTime);
                Debug.LogError($"Loi khi parse!!!");
                return getCurrentTimeUTC.Invoke() - resetDuration;
            }
            private set
            {
                saveLoad.SetString(saveKeyLastDepleted, value.ToFileTimeUtc().ToString());
                saveLoad.Save();
            }
        }

        /// <summary>
        /// Tính toán lại số lần dùng.
        /// </summary>
        public void Refresh()
        {
            if (RemainingUse <= 0)
            {
                if (getCurrentTimeUTC.Invoke() >= LastDepletedTime + resetDuration)
                {
                    ResetUses();
                }
            }
        }

        public bool CanUse()
        {
            if (RemainingUse <= 0)
            {
                if (getCurrentTimeUTC.Invoke() >= LastDepletedTime + resetDuration)
                {
                    ResetUses();
                    return true;
                }
                return false;
            }
            return true;
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
                LastDepletedTime = getCurrentTimeUTC.Invoke();
                Debug.Log("Đã dùng hết, chờ hồi.");
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
            DateTime readyTime = LastDepletedTime + resetDuration;
            TimeSpan remain = readyTime - getCurrentTimeUTC.Invoke();
            return remain;
        }
    }
}