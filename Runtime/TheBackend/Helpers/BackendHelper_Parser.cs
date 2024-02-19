using System;
using System.Collections.Generic;
using System.Globalization;
using BackEnd;
using JetBrains.Annotations;
using LitJson;
using UnityEngine;

namespace IdleGameModule.TheBackend
{
    public static partial class BackendHelper
    {
        public static string GetString(this JsonData jsonData, string key, string defaultVal = "")
        {
            if (!jsonData.ContainsKey(key))
            {
                Debug.Log($"[MP] {nameof(GetString)} 도중 Key 값이 잘못되었습니다. ({key})");
                return defaultVal;
            }
            
            return jsonData[key].ToString();
        }

        public static int GetInt(this JsonData jsonData, string key, int defaultVal = 0)
        {
            if (!jsonData.ContainsKey(key))
            {
                Debug.Log($"[MP] {nameof(GetInt)} 도중 Key 값이 잘못되었습니다. ({key})");
                return defaultVal;
            }
            
            return int.TryParse(jsonData[key].ToString(), out var value) ? value : defaultVal;
        }
        
        public static double GetDouble(this JsonData jsonData, string key, double defaultVal = 0)
        {
            if (!jsonData.ContainsKey(key))
            {
                Debug.Log($"[MP] {nameof(GetDouble)} 도중 Key 값이 잘못되었습니다. ({key})");
                return defaultVal;
            }
            
            var strVal = jsonData.GetString(key);
            var isPercent = strVal.Contains("%");

            if (isPercent) strVal = strVal.Split('%')[0];
            
            return double.TryParse(strVal, out var value) ? value * (isPercent ? 0.01f : 1)  : defaultVal;
        }
        
        public static float GetFloat(this JsonData jsonData, string key, float defaultVal = 0)
        {
            if (!jsonData.ContainsKey(key))
            {
                Debug.Log($"[MP] {nameof(GetFloat)} 도중 Key 값이 잘못되었습니다. ({key})");
                return defaultVal;
            }

            var strVal = jsonData.GetString(key);
            var isPercent = strVal.Contains("%");

            if (isPercent) strVal = strVal.Split('%')[0];
            
            return float.TryParse(strVal, out var value) ? value * (isPercent ? 0.01f : 1)  : defaultVal;
        }
        
        public static bool GetBool(this JsonData jsonData, string key, bool defaultVal = false)
        {
            if (!jsonData.ContainsKey(key))
            {
                Debug.Log($"[MP] {nameof(GetBool)} 도중 Key 값이 잘못되었습니다. ({key})");
                return defaultVal;
            }
            
            return bool.TryParse(jsonData[key].ToString(), out var value) ? value : defaultVal;
        }
        
        public static TEnum GetEnum<TEnum>(this JsonData jsonData, string key) where TEnum : struct 
        {
            if (!jsonData.ContainsKey(key))
            {
                return default;
            }

            var isParse = Enum.TryParse<TEnum>(jsonData[key].ToString(), out var value);
            
            return isParse ? value : default;
        }
        
        public static TEnum GetEnum<TEnum>(this JsonData jsonData, string key, TEnum defaultValue) where TEnum : struct 
        {
            if (!jsonData.ContainsKey(key))
            {
                return defaultValue;
            }

            var isParse = Enum.TryParse<TEnum>(jsonData[key].ToString(), out var value);
            
            return isParse ? value : defaultValue;
        }
    }
}