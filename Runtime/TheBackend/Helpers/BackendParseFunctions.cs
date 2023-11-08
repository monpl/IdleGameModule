using System;
using System.Collections.Generic;
using System.Globalization;
using BackEnd;
using BreakInfinity;
using LitJson;

namespace IdleGameModule.TheBackend
{
    public static class BackendParseFunctions
    {
        public static string GetInDateInArray(this BackendReturnObject bro, int arrayIdx = 0)
        {
            return bro.GetReturnValuetoJSON()["rows"][arrayIdx]["inDate"]["S"].ToString();
        }

        public static JsonData GetElementsArray(this BackendReturnObject bro)
        {
            var broJson = bro.GetReturnValuetoJSON();
            return broJson["elements"];
        }

        // --------
        // int
        public static int GetInt(this JsonData jsonData, string key, int defaultValue = 0)
        {
            if (jsonData.ContainsKey(key))
            {
                if (jsonData[key].ContainsKey("N"))
                {
                    if (int.TryParse(jsonData[key]["N"].ToString(), out var result))
                        return result;
                }
                else if (jsonData[key].ContainsKey("M"))
                {
                    return GetIntInObject(jsonData, key, defaultValue);
                }
                else if (jsonData[key].ContainsKey("NULL"))
                {
                    return defaultValue;
                }
                else
                {
                    return GetIntWithString(jsonData, key, defaultValue);
                }
            }

            return defaultValue;
        }

        public static int GetInt(this JsonData jsonData, int defaultValue = 0)
        {
            if (jsonData.ContainsKey("N"))
            {
                if (int.TryParse(jsonData["N"].ToString(), out var result))
                    return result;
            }

            if (jsonData.ContainsKey("S"))
            {
                if (int.TryParse(GetString(jsonData), out var result))
                    return result;
            }

            return defaultValue;
        }

        public static int GetIntWithString(this JsonData jsonData, string key, int defaultValue = 0)
        {
            return jsonData.ContainsKey(key) ? int.Parse(jsonData.GetString(key)) : defaultValue;
        }

        public static int GetIntWithStringSafe(this JsonData jsonData, string key, int defaultValue = 0)
        {
            if (jsonData.ContainsKey(key) == false)
                return defaultValue;

            if (int.TryParse(jsonData.GetString(key), out int val) == false)
                return defaultValue;

            return val;
        }

        public static int GetIntInObject(this JsonData jsonData, string key, int defaultValue = 0)
        {
            return jsonData.ContainsKey("M") ? jsonData["M"].GetInt(key, defaultValue) : defaultValue;
        }

        public static int GetIntDirect(this JsonData jsonData, string key, int defaultValue = 0)
        {
            if (int.TryParse(jsonData[key].ToString(), out int val))
                return val;

            return defaultValue;
        }

        // --------
        // float
        public static float GetFloat(this JsonData jsonData, string key, float defaultValue = 0f)
        {
            if (jsonData.ContainsKey(key))
            {
                if (jsonData[key].ContainsKey("N"))
                {
                    if (float.TryParse(jsonData[key]["N"].ToString(), out var result))
                        return result;
                }
                else if (jsonData[key].ContainsKey("M"))
                {
                    return GetFloatInObject(jsonData, key, defaultValue);
                }
                else if (jsonData[key].ContainsKey("NULL"))
                {
                    return defaultValue;
                }
                else
                {
                    return GetFloatWithString(jsonData, key, defaultValue);
                }
            }

            return defaultValue;
        }

        public static float GetFloatWithString(this JsonData jsonData, string key, float defaultValue = 0)
        {
            return jsonData.ContainsKey(key) ? float.Parse(jsonData.GetString(key)) : defaultValue;
        }

        public static float GetFloatInObject(this JsonData jsonData, string key, float defaultValue = 0)
        {
            return jsonData.ContainsKey("M") ? jsonData["M"].GetFloat(key, defaultValue) : defaultValue;
        }

        // --------
        // double

        public static double GetDouble(this JsonData jsonData, string key, double defaultValue = 0d)
        {
            if (jsonData.ContainsKey(key))
            {
                if (jsonData[key].ContainsKey("N"))
                {
                    if (double.TryParse(jsonData[key]["N"].ToString(), out var result))
                        return result;
                }
                else if (jsonData[key].ContainsKey("M"))
                {
                    return GetDoubleInObject(jsonData, key, defaultValue);
                }
                else if (jsonData[key].ContainsKey("NULL"))
                {
                    return defaultValue;
                }
                else
                {
                    return GetDoubleWithString(jsonData, key, defaultValue);
                }
            }

            return defaultValue;
        }

        public static double GetDoubleWithString(this JsonData jsonData, string key, double defaultValue = 0)
        {
            return jsonData.ContainsKey(key) ? double.Parse(jsonData.GetString(key), CultureInfo.InvariantCulture) : defaultValue;
        }

        public static double GetDoubleInObject(this JsonData jsonData, string key, double defaultValue = 0)
        {
            return jsonData.ContainsKey("M") ? jsonData["M"].GetDouble(key, defaultValue) : defaultValue;
        }

        public static BigDouble GetBigDouble(this JsonData jsonData, string key, BigDouble defaultValue = default)
        {
            if (jsonData.ContainsKey(key))
            {
                if (jsonData[key].ContainsKey("N"))
                {
                    if (BigDouble.TryParse(jsonData[key]["N"].ToString(), out var result))
                        return result;
                }
                else if (jsonData[key].ContainsKey("M"))
                {
                    return GetBigDoubleInObject(jsonData, key, defaultValue);
                }
                else if (jsonData[key].ContainsKey("NULL"))
                {
                    return defaultValue;
                }
                else
                {
                    return GetBigDoubleWithString(jsonData, key, defaultValue);
                }
            }

            return defaultValue;
        }

        public static BigDouble GetBigDoubleWithString(this JsonData jsonData, string key, BigDouble defaultValue = default)
        {
            return jsonData.ContainsKey(key) ? BigDouble.Parse(jsonData.GetString(key)) : defaultValue;
        }

        public static BigDouble GetBigDoubleInObject(this JsonData jsonData, string key, BigDouble defaultValue = default)
        {
            return jsonData.ContainsKey("M") ? jsonData["M"].GetBigDouble(key, defaultValue) : defaultValue;
        }

        // --------
        // string
        public static string GetString(this JsonData jsonData)
        {
            if (jsonData.ContainsKey("S") == false)
                return "";

            return jsonData["S"]?.ToString();
        }

        public static string GetString(this JsonData jsonData, string key, string defaultValue = "")
        {
            if (!jsonData.ContainsKey(key)) return defaultValue;

            if (jsonData[key].ContainsKey("S"))
            {
                return jsonData[key]["S"].ToString();
            }

            return jsonData[key].ContainsKey("M") ? GetStringInObject(jsonData, key, defaultValue) : defaultValue;
        }

        public static string GetStringInDic(this JsonData jsonData, string key, string defaultValue = "")
        {
            return jsonData.ContainsKey(key) ? jsonData[key].ToString() : defaultValue;
        }

        public static string GetStringInObject(this JsonData jsonData, string key, string defaultValue = "")
        {
            return jsonData.ContainsKey("M") ? jsonData["M"].GetString(key, defaultValue) : defaultValue;
        }

        public static string GetStringDirect(this JsonData jsonData, string key)
        {
            return jsonData[key].ToString();
        }

        // -------
        // bool
        public static bool GetBool(this JsonData jsonData, string key, bool defaultValue = false)
        {
            if (!jsonData.ContainsKey(key)) return defaultValue;

            if (jsonData[key].ContainsKey("BOOL"))
            {
                return bool.Parse(jsonData[key]["BOOL"].ToString());
            }

            return jsonData[key].ContainsKey("M") ? GetBoolInObject(jsonData, key, defaultValue) : defaultValue;
        }

        public static bool GetBool(this JsonData jsonData, bool defaultValue = false)
        {
            if (jsonData.ContainsKey("BOOL"))
            {
                if (bool.TryParse(jsonData["BOOL"].ToString(), out var result))
                    return result;
            }

            if (jsonData.ContainsKey("S"))
            {
                if (bool.TryParse(GetString(jsonData), out var result))
                    return result;
            }

            return defaultValue;
        }

        public static bool GetBoolInObject(this JsonData jsonData, string key, bool defaultValue = false)
        {
            return jsonData.ContainsKey("M") ? jsonData["M"].GetBool(key, defaultValue) : defaultValue;
        }

        // --------
        // list
        public static List<JsonData> GetList(this JsonData jsonData, string key)
        {
            if (jsonData.ContainsKey(key) == false)
                return new List<JsonData>();

            var dataList = jsonData[key]["L"];
            var returnList = new List<JsonData>();

            var dataListCount = dataList.Count;

            for (var i = 0; i < dataListCount; ++i)
                returnList.Add(dataList[i]);

            return returnList;
        }

        // --------
        // dictionary
        public static Tuple<JsonData, ICollection<string>> GetDictionary(this JsonData jsonData, string key)
        {
            if (jsonData.ContainsKey(key) == false)
                return new Tuple<JsonData, ICollection<string>>(new JsonData(), new List<string>());

            var retDic = jsonData[key]["M"];
            return new Tuple<JsonData, ICollection<string>>(retDic, retDic.Keys);
        }

        // --------
        // member
        public static JsonData GetMember(this JsonData jsonData, string key)
        {
            return jsonData.ContainsKey(key) ? jsonData[key]["M"] : jsonData;
        }

        public static JsonData GetMember(this JsonData jsonData)
        {
            return jsonData.ContainsKey("M") ? jsonData["M"] : null;
        }
    }
}