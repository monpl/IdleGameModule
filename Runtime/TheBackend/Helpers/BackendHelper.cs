using System;
using System.Globalization;
using BackEnd;
using Cysharp.Threading.Tasks;
using LitJson;

namespace IdleGameModule.TheBackend
{
    /// <summary>
    /// 뒤끝 모듈 관련 헬퍼 함수들
    /// </summary>
    public static class BackendHelper
    {
        /// <summary>
        /// BackendReturnObject를 통해 얻은 Error를 Exception으로 변환시켜주는 함수
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="bro"></param>
        /// <returns></returns>
        public static Exception CreateException(this BackendReturnObject bro, string msg)
        {
            var ex = new Exception(msg);
            // var ex = new Exception($"{msg}, StatusCode: {bro.GetStatusCode()}, errorCode: {bro.GetErrorCode()}");

            // ex.Data.Add("ErrorCode", "BadUnauthorizedException");
            // ex.Data.Add("StatusCode", "401");
            // ex.Data.Add("msg", "bad accessToken, 잘못된 accessToken 입니다");
            // ----------------------------------
            ex.Data.Add("ErrorCode", bro.GetErrorCode());
            ex.Data.Add("StatusCode", bro.GetStatusCode());
            ex.Data.Add("msg", bro.GetMessage());
            return ex;
        }

        public static void SetResultIfNotNull<T>(this UniTaskCompletionSource<T> completion, T target) where T : class
        {
            if (target != null)
                completion.TrySetResult(target);
            else
                completion.TrySetException(new Exception("Target is null"));
        }

        public static DateTime ToDateTime(this string str, DateTime defaultValue = default(DateTime))
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var retValue))
                return retValue.ToUniversalTime();
            else
                return defaultValue;
        }

        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input[0].ToString().ToUpper() + input[1..]
            };

        public static UserRankingData CreateUserRankingDataFromJson(this JsonData userRankJson)
        {
            return new UserRankingData
            {
                gamerInDate = userRankJson["gamerInDate"].ToString(),
                nickname = userRankJson["nickname"].ToString(),
                score = double.Parse(userRankJson["score"].ToString()),
                index = userRankJson["index"].ToString(),
                rank = userRankJson["rank"].ToString(),
            };
        }

        public static GuildRankingData CreateGuildRankingDataFromJson(this JsonData guildRankJson)
        {
            return new GuildRankingData
            {
                guildInDate = guildRankJson["gamerInDate"].ToString(),
                guildName = guildRankJson["nickname"].ToString(),
                score = double.Parse(guildRankJson["score"].ToString()),
                index = guildRankJson["index"].ToString(),
                rank = guildRankJson["rank"].ToString(),
            };
        }

        public static RankingRewardData CreateRankingRewardData(this JsonData rewardJson)
        {
            return new RankingRewardData
            {
                startRank = int.Parse(rewardJson["startRank"].ToString()),
                endRank = int.Parse(rewardJson["endRank"].ToString()),
                itemId = rewardJson["rewardItems"]["itemId"].ToString(), // 차트에 아이템 차트와 itemId 컬럼이 있어야함
                rewardItemCount = int.Parse(rewardJson["rewardItemCount"].ToString()),
            };
        }

        public static string GetString(this JsonData jsonData, string key, string defaultStr = "")
        {
            return jsonData.ContainsKey(key) ? jsonData[key].ToString() : defaultStr;
        }

        public static bool CheckSuccess(this BackendReturnObject bro, UniTaskCompletionSource source, string exMessage)
        {
            if (bro.IsSuccess())
                return true;

            source.TrySetException(bro.CreateException(exMessage));
            return false;
        }

        public static bool CheckSuccess<T>(this BackendReturnObject bro, UniTaskCompletionSource<T> source, string exMessage)
        {
            if (bro.IsSuccess())
                return true;

            source.TrySetException(bro.CreateException(exMessage));
            return false;
        }
    }
}