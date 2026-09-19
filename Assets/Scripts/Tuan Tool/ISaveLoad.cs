using System;
using UnityEngine;

namespace TuanTool
{
    public interface ISaveLoad
    {
        string KeyPrefix { get; set; }

        void SetInt(string key, int value);
        int GetInt(string key, int defaultValue = 0);

        void SetFloat(string key, float value);
        float GetFloat(string key, float defaultValue = 0f);

        void SetBool(string key, bool value);
        bool GetBool(string key, bool defaultValue = false);

        void SetString(string key, string value);
        string GetString(string key, string defaultValue = "");

        void SetObject<T>(string key, T obj);
        T GetObject<T>(string key, T defaultValue = default);

        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
        void Save();
    }


    public class SaveLoadKeys
    {
        public static string LANGUAGE_KEY = "LANGUAGE_SELECTED";
        public static string VOLUME_MUSIC = "VolumeMusic";
        public static string VOLUME_FX = "VolumeFX";
        public static string VIBRATION_KEY = "Vibration";

        public static string USER_ID = "UserId";
        public static string ACCESS_TOKEN = "AccessToken";
        public static string REFRESH_TOKEN = "RefreshToken";
        public static string USERNAME = "Username";
        public static string PASSWORD = "Password";
        public static string IS_NEW_USER = "IsNewUser";
        public static string SAVED_LOGIN = "SavedLogin";
        public static string IS_ADS_REMOVED = "IsAdsRemoved";
        public static string REMAINING_USE_BUY_IAP_ = "RemainingUseBuyIAP_";
        public static string LAST_TIME_OVER_BUY_IAP_ = "LastTimeOverBuyIAP_";
    }
}
