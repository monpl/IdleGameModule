using System;
using BackEnd;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace IdleGameModule.TheBackend
{
    public class GameVersion
    {
        public Version version;
        public int updateType;

        public GameVersion(Version version, int updateType)
        {
            this.version = version;
            this.updateType = updateType;
        }
    }

    public partial class BackendUtil
    {
        /// <summary>
        /// 현재 내 Application버전이 게임 가능한 버전인지 체크
        /// </summary>
        /// <returns>게임 버전 플레이 가능 여부</returns>
        public async UniTask<bool> IsAvailableVersion()
        {
            var serverVersion = await GetLatestVersion();
            var appVersion = GetVersionOrNull(Application.version);

            if (serverVersion.version > appVersion)
            {
                // 1: 선택, 2: 강제
                return serverVersion.updateType == 1;
            }

            return true;
        }

        /// <summary>
        /// 현재 서버의 마지막 버전을 가져옴. ( Editor에선 Application버전 고정 )
        /// </summary>
        /// <returns>현재 서버의 게임 버전</returns>
        public UniTask<GameVersion> GetLatestVersion()
        {
            var completion = new UniTaskCompletionSource<GameVersion>();

            SendQueue.Enqueue(Backend.Utils.GetLatestVersion, bro =>
            {
                // 이후 처리
                if (bro.IsSuccess())
                {
                    var json = bro.GetReturnValuetoJSON();
                    var versionStr = json["version"].ToString();
                    var updateType = int.Parse(json["type"].ToString());

                    completion.SetResultIfNotNull(GetGameVersionOrNull(versionStr, updateType));
                }
                else
                {
#if UNITY_EDITOR
                    completion.SetResultIfNotNull(GetGameVersionOrNull(Application.version, 2));
#else
                    completion.TrySetException(bro.CreateException("LatestVersion"));
#endif
                }
            });

            return completion.Task;
        }

        /// <summary>
        /// GameVersion class를 만들어 가져옴
        /// </summary>
        /// <param name="versionStr">버전 string</param>
        /// <param name="updateType">업데이트 타입 (강제, 선택)</param>
        /// <returns></returns>
        private GameVersion GetGameVersionOrNull(string versionStr, int updateType)
        {
            var version = GetVersionOrNull(versionStr);

            return version == null ? null : new GameVersion(version, updateType);
        }

        /// <summary>
        /// string으로 되어있는 버전을 class로 바꿔준다. 잘못된 형식이면 null을 반환
        /// </summary>
        /// <param name="versionStr"></param>
        /// <returns></returns>
        private Version GetVersionOrNull(string versionStr)
        {
            var versionSplit = versionStr.Split('.');

            if (versionSplit.Length < 3)
                return null;

            if (int.TryParse(versionSplit[0], out var major) == false ||
                int.TryParse(versionSplit[1], out var minor) == false ||
                int.TryParse(versionSplit[2], out var patch) == false)
                return null;

            return new Version(major, minor, patch);
        }
    }
}