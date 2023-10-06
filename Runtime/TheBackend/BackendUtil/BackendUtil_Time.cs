using System;
using System.Diagnostics;
using System.Threading;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;
using Debug = UnityEngine.Debug;

namespace IdleGameModule.TheBackend
{
    /// <summary>
    /// 뒤끝 유틸함수 관련 기능들 모음
    /// </summary>
    public partial class BackendUtil : Singleton<BackendUtil>
    {
        private const int _normalServerTimeInterval = 600;
        private const int _errorServerTimeInterval = 60;
        private const int _firstErrorServerTimeInterval = 5;
        
        private readonly Stopwatch _stopwatch = new();

        private DateTime _lastServerTimeUtc = DateTime.MinValue;
        private TimeSpan _focusOutTime;
        private CancellationTokenSource _cancel;
        private int _getServerTimeIntervalSec;
        private bool _isLoadedServerTime;

        public Action<DateTime> OnServerTimeFunc;

        /// <summary>
        /// 서버 utc시간을 갖고 옴 ( N분마다 불러오는 서버시간에 타이머 시간을 더한 값 ) 
        /// </summary>
        /// <returns></returns>
        public DateTime GetServerUtcTime()
        {
            if (_lastServerTimeUtc == DateTime.MinValue)
            {
                Debug.Log("LAST SERVER TIME UTC가 MIN VALUE입니다.. (이런 상황은 없어야함) ");
                return DateTime.UtcNow;
            }

            return _lastServerTimeUtc.AddMilliseconds(_stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 내부적으로 서버 시간을 업데이트 하는 타이머 실행
        /// </summary>
        public void StartInternalServerTime()
        {
            if (_cancel != null)
                return;
            
            _cancel = new CancellationTokenSource();
            RefreshServerTime().Forget();
        }
        
        /// <summary>
        /// 내부적으로 서버 시간을 업데이트 하는 타이머 중지
        /// </summary>
        public void StopInternalServerTime()
        {
            if (_cancel == null)
                return;

            _cancel.Cancel();
            _cancel = null;
        }

        private async UniTask RefreshServerTime()
        {
            while (true)
            {
                try
                {
                    var utcTime = await GetServerTimeInternal();
                    Debug.Log($"Get Server UTC Time: {utcTime}");

                    _isLoadedServerTime = true;
                    _lastServerTimeUtc = utcTime;
                    _stopwatch.Restart();

                    OnServerTimeFunc?.Invoke(utcTime);
                    _getServerTimeIntervalSec = _normalServerTimeInterval;
                }
                catch (Exception _)
                {
                    _getServerTimeIntervalSec = _isLoadedServerTime ? _errorServerTimeInterval : _firstErrorServerTimeInterval;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_getServerTimeIntervalSec), ignoreTimeScale: true, cancellationToken: _cancel.Token);
            }
        }

        /// <summary>
        /// 서버타임 불러오기
        /// </summary>
        /// <returns></returns>
        private UniTask<DateTime> GetServerTimeInternal()
        {
            var completion = new UniTaskCompletionSource<DateTime>();

            SendQueue.Enqueue(Backend.Utils.GetServerTime, bro =>
            {
                if (bro.IsSuccess())
                {
                    var timeStr = bro.GetReturnValuetoJSON()["utcTime"].ToString();
                    var utcTime = DateTime.Parse(timeStr).ToUniversalTime();

                    completion.TrySetResult(utcTime);
                }
                else
                {
                    completion.TrySetException(bro.CreateException("서버 타임 불러오기 실패!"));
                }
            });

            return completion.Task;
        }
    }
}