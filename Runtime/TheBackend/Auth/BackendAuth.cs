using System;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;
using UnityEngine;

namespace IdleGameModule.TheBackend
{
    public enum LoginType
    {
        Apple,
        Google,
        Guest,
    }
    
    public class BackendAuth : Singleton<BackendAuth>
    {
        // (400) 기기 로컬에 액세스 토큰이 존재하지 않는 경우
        // (401) 다른기기로 로그인하여 refresh 토큰이 만료된 경우
        // (410) 토큰이 만료된 경우
        private readonly int[] _loginEdgeStatusCode = {400, 401, 410};
        
        public async UniTask TryInit()
        {
            if (!Backend.IsInitialized)
            {
                return;
            }
        }

        /// <summary>
        /// 로그인을 시도한다 ( 현재는 Guest만 지원 )
        /// </summary>
        /// <param name="loginType"></param>
        public async UniTask TryLogin(LoginType loginType)
        {
            var statusCode = await TryLoginWithBackendToken();

            if (IsLoginEdgeCase(statusCode))
                await LoginAndGetNewRefreshToken(loginType);
        }

        /// <summary>
        /// 회원탈퇴 진행
        /// </summary>
        /// <param name="graceHours">유예기간, 기본값: 7일(168시간)</param>
        /// <returns></returns>
        public UniTask WithdrawAccount(int graceHours = 168)
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.BMember.WithdrawAccount, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Withdraw Account"))
                    return;

                completion.TrySetResult();
            });

            return completion.Task;
        }

        /// <summary>
        /// 게스트에서 Federation으로 전환
        /// </summary>
        /// <param name="loginType"></param>
        /// <returns></returns>
        public UniTask ChangeGuestToFederation(LoginType loginType)
        {
            var completion = new UniTaskCompletionSource();

            switch (loginType)
            {
                case LoginType.Apple:
                    // TODO: 애플 로그인 진행!
                    break;
                case LoginType.Google:
                    // TODO: 구글 로그인 진행!
                    break;
                default:
                    completion.TrySetException(new Exception($"Wrong LoginType..{loginType}"));
                    break;
            }

            return completion.Task;
        }

        /// <summary>
        /// AccessToken을 Refresh함
        /// </summary>
        /// <returns></returns>
        public UniTask RefreshAccessToken()
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.BMember.RefreshTheBackendToken, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Refresh Access Token"))
                    return;

                completion.TrySetResult();
            });

            return completion.Task;
        }

        /// <summary>
        /// 닉네임 업데이트를 함 ( 없는 경우 생성 )
        /// </summary>
        /// <param name="nickname">업데이트하거나 생성할 닉네임</param>
        /// <returns>[400: 빈 닉네임, 20자 이상, 닉네임 뒤 앞/뒤 공백], [409: 중복된 닉네임]</returns>
        public UniTask UpdateNickname(string nickname)
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.BMember.UpdateNickname, nickname, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Nickname created"))
                    return;

                completion.TrySetResult();
            });

            return completion.Task;
        }

        /// <summary>
        /// BackendToken으로 로그인 시도 
        /// </summary>
        /// <returns>StatusCode</returns>
        private UniTask<int> TryLoginWithBackendToken()
        {
            var completion = new UniTaskCompletionSource<int>();
            
            SendQueue.Enqueue(Backend.BMember.LoginWithTheBackendToken, bro =>
            {
                var statusCode = int.Parse(bro.GetStatusCode());
                var isEdgeCase = statusCode == 410 || statusCode == 400 || statusCode == 401;
                
                if (bro.IsSuccess() || isEdgeCase)
                {
                    completion.TrySetResult(statusCode);
                    return;
                }

                completion.TrySetException(bro.CreateException("Backend token login failed"));
            });

            return completion.Task;
        }

        /// <summary>
        /// 로그인하고 새로운 리프레시 토큰을 받는다
        /// </summary>
        /// <returns></returns>
        private async UniTask LoginAndGetNewRefreshToken(LoginType loginType)
        {
            switch (loginType)
            {
                case LoginType.Guest:
                    await LoginWithGuest();
                    return;
                default: // TODO: 구글, 애플 로그인 구현
                    throw new ArgumentOutOfRangeException(nameof(loginType), loginType, null);
            }
        }

        private UniTask LoginWithGuest()
        {
            var completion = new UniTaskCompletionSource();
            
            SendQueue.Enqueue(Backend.BMember.GuestLogin, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Login With Guest"))
                    return;

                Debug.Log("Login Guest Success..!");
                completion.TrySetResult();
            });

            return completion.Task;
        }
        
        /// <summary>
        /// 로그인의 엣지케이스를 검사
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private bool IsLoginEdgeCase(int statusCode)
        {
            return _loginEdgeStatusCode.Any(edgeStatusCode => edgeStatusCode == statusCode);
        }
    }
}