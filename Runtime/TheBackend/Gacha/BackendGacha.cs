using System;
using System.Collections.Generic;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;
using LitJson;
using UnityEngine;

namespace IdleGameModule.TheBackend
{
    public class BackendGacha : Singleton<BackendGacha>
    {
        private Dictionary<string, List<string>> _gachaCardIdListDic;
        
        /// <summary>
        /// 적용된 모든 확률표를 로드 ( 뽑기 확률표 이름은 언더바를 꼭 넣어야함 )
        /// </summary>
        /// <returns></returns>
        public UniTask LoadAllProbabilityCards()
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.Probability.GetProbabilityCardListV2, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Load Probability Cards"))
                    return;

                AcceptToGachaCardList(bro.FlattenRows());
            });

            return completion.Task;
        }
        
        /// <summary>
        /// 뽑기를 진행
        /// </summary>
        /// <param name="gachaName">뽑기 이름</param>
        /// <param name="gachaCount">뽑기 횟수 ( 최대 100 )</param>
        /// <param name="gachaLevel">뽑기 레벨</param>
        /// <returns></returns>
        public UniTask<JsonData> TryGacha(string gachaName, int gachaCount, int gachaLevel = 1)
        {
            var completion = new UniTaskCompletionSource<JsonData>();

            if (!_gachaCardIdListDic.ContainsKey(gachaName))
            {
                completion.TrySetException(new Exception("GachaName is None: " + gachaName));
                return completion.Task;
            }

            var gachaCardIdList = _gachaCardIdListDic[gachaName];
            var gachaCardId = gachaCardIdList[Mathf.Clamp(gachaLevel - 1, 0, gachaCardIdList.Count - 1)];
            gachaCount = Mathf.Clamp(gachaCount, 0, 100);
            
            SendQueue.Enqueue(Backend.Probability.GetProbabilitys, gachaCardId, gachaCount, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Try Gacha"))
                    return;

                var elements = bro.GetFlattenJSON()["elements"];

                completion.TrySetResult(elements);
            });
            
            return completion.Task;
        }
        
        private void AcceptToGachaCardList(JsonData jsonData)
        {
            _gachaCardIdListDic = new Dictionary<string, List<string>>();

            for (var i = 0; i < jsonData.Count; ++i)
            {
                var curJson = jsonData[i];
                var cardFullName = curJson["probabilityName"].ToString();
                var fileId = curJson["selectedProbabilityFileId"].ToString();
                var nameSplit = cardFullName.Split('_');
                var cardName = nameSplit[0];

                if (!_gachaCardIdListDic.ContainsKey(cardName))
                    _gachaCardIdListDic.Add(cardName, new List<string>());

                var curIdList = _gachaCardIdListDic[cardName];

                // 레벨이 없는 경우
                if (nameSplit.Length < 2)
                {
                    curIdList.Add(fileId);
                    continue;
                }

                // 레벨 체크
                var level = int.Parse(nameSplit[1]);

                while (curIdList.Count < level)
                    curIdList.Add(string.Empty);

                curIdList[level - 1] = fileId;
            }
        }
    }
}