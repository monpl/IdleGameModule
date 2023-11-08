using System;
using IdleGameModule.Common;

namespace IdleGameModule.TheBackend
{
    /// <summary>
    /// Mono 기능들 (Update..)을 이용하기 위한 유틸 함수
    /// </summary>
    public class BackendMono : MonoSingleton<BackendMono>
    {
        public Action OnUpdateActions { get; set; }
        
        private void Update()
        {
            OnUpdateActions?.Invoke();
        }
    }
}