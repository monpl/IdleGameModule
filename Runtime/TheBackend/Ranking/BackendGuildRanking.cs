using System;
using System.Collections.Generic;
using System.Globalization;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;

namespace IdleGameModule.TheBackend
{
    public class BackendGuildRanking : Singleton<BackendGuildRanking>
    {
        private Dictionary<string, GuildRankingTableData> guildRankingTableDic;

        /// <summary>
        /// 길드 랭킹 관련 초기화
        /// </summary>
        public async UniTask InitUserRanking()
        {
            guildRankingTableDic = await GetRankTableList();
        }

        /// <summary>
        /// 길드 굿즈 기부를 통한 랭킹 갱신
        /// </summary>
        /// <param name="guildRankingName"></param>
        /// <param name="goodsType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UniTask ContributeAndSubmitScore(string guildRankingName, goodsType goodsType, int value)
        {
            var completion = new UniTaskCompletionSource();

            if (!guildRankingTableDic.ContainsKey(guildRankingName))
            {
                completion.TrySetException(new Exception("RankingName is Not found"));
                return completion.Task;
            }

            var curRankingData = guildRankingTableDic[guildRankingName];

            SendQueue.Enqueue(Backend.URank.Guild.ContributeGuildGoods, curRankingData.uuid, goodsType, value, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Update Guild Contribute Score.."))
                    return;

                completion.TrySetResult();
            });

            return completion.Task;
        }

        /// <summary>
        /// 랭킹 리스트를 조회 ( 최적화를 위해 특정 분마다 불러오는 걸 추천 )
        /// </summary>
        /// <param name="rankingName">불러올 랭킹 리스트</param>
        /// <param name="limit">랭커 수</param>
        /// <param name="offset">랭킹 시작 점</param>
        /// <returns></returns>
        public UniTask<GuildRankingData[]> GetRankingList(string rankingName, int limit, int offset = 0)
        {
            var completion = new UniTaskCompletionSource<GuildRankingData[]>();

            if (!guildRankingTableDic.ContainsKey(rankingName))
            {
                completion.TrySetException(new Exception("Ranking Name is not exist, name: " + rankingName));
                return completion.Task;
            }

            SendQueue.Enqueue(Backend.URank.Guild.GetRankList, guildRankingTableDic[rankingName].uuid, limit, offset, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Guild Ranking List"))
                    return;

                var rankListJson = bro.GetFlattenJSON()["rows"];
                var rankList = new List<GuildRankingData>();

                for (var i = 0; i < rankListJson.Count; ++i)
                    rankList.Add(rankListJson[i].CreateGuildRankingDataFromJson());

                completion.TrySetResult(rankList.ToArray());
            });

            return completion.Task;
        }

        /// <summary>
        /// 내 길드 랭킹 정보를 가져온다.
        /// </summary>
        /// <param name="rankingName">랭킹 이름</param>
        /// <returns></returns>
        public UniTask<GuildRankingData> GetMyGuildRankData(string rankingName)
        {
            var completion = new UniTaskCompletionSource<GuildRankingData>();

            if (!guildRankingTableDic.ContainsKey(rankingName))
            {
                completion.TrySetException(new Exception("Ranking Name is not exist, name: " + rankingName));
                return completion.Task;
            }

            var curRankingTable = guildRankingTableDic[rankingName];

            SendQueue.Enqueue(Backend.URank.Guild.GetMyGuildRank, curRankingTable.uuid, bro =>
            {
                if (!bro.IsSuccess())
                {
                    completion.TrySetException(bro.CreateException("Failed to load my rank"));
                    return;
                }

                var myRankJson = bro.GetFlattenJSON()["rows"][0];
                var myRankData = myRankJson.CreateGuildRankingDataFromJson();

                completion.TrySetResult(myRankData);
            });

            return completion.Task;
        }

        /// <summary>
        /// 보상 리스트를 불러옴
        /// </summary>
        /// <param name="rankingName">랭킹 이름</param>
        /// <returns></returns>
        public UniTask<RankingRewardData[]> GetRankingRewardList(string rankingName)
        {
            var completion = new UniTaskCompletionSource<RankingRewardData[]>();

            if (!guildRankingTableDic.ContainsKey(rankingName))
            {
                completion.TrySetException(new Exception("Ranking Name is not exist, name: " + rankingName));
                return completion.Task;
            }

            SendQueue.Enqueue(Backend.URank.Guild.GetRankRewardList, guildRankingTableDic[rankingName].uuid, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed to load Ranking Reward"))
                    return;

                var rewardJsonList = bro.GetFlattenJSON()["rows"];
                var rankingRewardDataList = new List<RankingRewardData>();

                for (var i = 0; i < rewardJsonList.Count; ++i)
                    rankingRewardDataList.Add(rewardJsonList[i].CreateRankingRewardData());

                completion.TrySetResult(rankingRewardDataList.ToArray());
            });

            return completion.Task;
        }

        /// <summary>
        /// 길드 랭킹 테이블 리스트 가져오는 함수
        /// </summary>
        /// <returns></returns>
        private UniTask<Dictionary<string, GuildRankingTableData>> GetRankTableList()
        {
            var completion = new UniTaskCompletionSource<Dictionary<string, GuildRankingTableData>>();

            SendQueue.Enqueue(Backend.URank.Guild.GetRankTableList, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Initialize Ranking"))
                    return;

                var rankTableJsonList = bro.FlattenRows();
                var rankTableDic = new Dictionary<string, GuildRankingTableData>();

                for (var i = 0; i < rankTableJsonList.Count; ++i)
                {
                    var curJson = rankTableJsonList[i];
                    var rankTableData = new GuildRankingTableData
                    {
                        rankType = curJson["rankType"].ToString(),
                        dateType = (RankDateType)Enum.Parse(typeof(RankDateType), curJson["date"].ToString().FirstCharToUpper()),
                        uuid = curJson["uuid"].ToString(),
                        tableName = curJson["table"].ToString()
                    };

                    if (curJson.ContainsKey("rankStartDateAndTime"))
                    {
                        rankTableData.rankStartTime = DateTime.Parse(curJson["rankStartDateAndTime"].ToString(), CultureInfo.InvariantCulture);
                        rankTableData.rankEndTime = DateTime.Parse(curJson["rankEndDateAndTime"].ToString(), CultureInfo.InvariantCulture);
                    }

                    if (curJson.ContainsKey("extraDataColumn"))
                    {
                        rankTableData.extraDataColumn = curJson["extraDataColumn"].ToString();
                        rankTableData.extraDataType = curJson["extraDataType"].ToString();
                    }

                    rankTableDic.Add(curJson["title"].ToString(), rankTableData);
                }

                completion.TrySetResult(rankTableDic);
            });

            return completion.Task;
        }
    }
}