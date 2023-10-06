using System;
using System.Globalization;
using BackEnd;
using Cysharp.Threading.Tasks;

namespace IdleGameModule.TheBackend
{
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
    }
}