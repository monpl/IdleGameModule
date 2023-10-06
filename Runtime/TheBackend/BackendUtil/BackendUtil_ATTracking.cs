using System;
using Cysharp.Threading.Tasks;
using Unity.Advertisement.IosSupport;
using UnityEngine;

namespace IdleGameModule.TheBackend
{
    public partial class BackendUtil
    {
        public Action<ATTrackingStatusBinding.AuthorizationTrackingStatus> OnEndATTrackingEvent;
        
        /// <summary>
        /// iOS14 부터 검사하는 ATTracking 팝업 호출함수
        /// </summary>
        public async UniTask CheckAndRequestATTracking()
        {
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Version iosVersion = new Version(UnityEngine.iOS.Device.systemVersion);
                Version minimumVersion = new Version("14.0");

                if (iosVersion >= minimumVersion)
                {
                    Debug.Log("[YC] ATT STATUS: " + ATTrackingStatusBinding.GetAuthorizationTrackingStatus());

                    if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                        ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                        ATTrackingStatusBinding.RequestAuthorizationTracking();

                    await WaitForResponse();
                }
            }
#endif
            
        }

        private async UniTask WaitForResponse()
        {
            float limitTime = 30f;
            float curTime = 0f;

            while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED &&
                   curTime < limitTime)
            {
                curTime += Time.deltaTime;
                await UniTask.Yield();
            }
            
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            OnEndATTrackingEvent?.Invoke(status);
        }
    }
}