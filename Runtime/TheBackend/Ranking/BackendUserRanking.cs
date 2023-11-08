using System;
using System.Collections.Generic;
using System.Globalization;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;

namespace IdleGameModule.TheBackend
{
    public enum RankDateType
    {
        Day, // 일간
        Week, // 주간
        Month, // 월간
        Infinity, // 누적
        Custom // 일회성
    }

    public class BackendUserRanking : Singleton<BackendUserRanking>
    {
        private Dictionary<string, UserRankingTableData> userRankingTableDic;

        /// <summary>
        /// 유저 랭킹 관련 초기화
        /// </summary>
        public async UniTask InitUserRanking()
        {
            userRankingTableDic = await GetRankTableList();
        }


        /// <summary>
        /// 유저 점수를 업데이트 함
        /// </summary>
        /// <param name="rankingName">랭킹 이름</param>
        /// <param name="rankingParam">업데이트할 랭킹 param</param>
        /// <returns></returns>
        public UniTask UpdateUserScore(string rankingName, Param rankingParam)
        {
            var completion = new UniTaskCompletionSource();

            if (!userRankingTableDic.ContainsKey(rankingName))
            {
                completion.TrySetException(new Exception("RankingName is Not found"));
                return completion.Task;
            }

            var curRankingData = userRankingTableDic[rankingName];

            SendQueue.Enqueue(Backend.URank.User.UpdateUserScore, curRankingData.uuid, curRankingData.tableName, curRankingData.tableInDate, rankingParam, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Update User Score.."))
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
        public UniTask<UserRankingData[]> GetRankingList(string rankingName, int limit, int offset = 0)
        {
            var completion = new UniTaskCompletionSource<UserRankingData[]>();

            if (!userRankingTableDic.ContainsKey(rankingName))
            {
                completion.TrySetException(new Exception("Ranking Name is not exist, name: " + rankingName));
                return completion.Task;
            }

            SendQueue.Enqueue(Backend.URank.User.GetRankList, userRankingTableDic[rankingName].uuid, limit, offset, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed User Ranking List"))
                    return;

                var rankListJson = bro.GetFlattenJSON()["rows"];
                var rankList = new List<UserRankingData>();

                for (var i = 0; i < rankListJson.Count; ++i)
                    rankList.Add(rankListJson[i].CreateUserRankingDataFromJson());

                completion.TrySetResult(rankList.ToArray());
            });

            return completion.Task;
        }

        /// <summary>
        /// 내 랭킹 정보를 가져온다.
        /// </summary>
        /// <param name="rankingName">랭킹 이름</param>
        /// <returns></returns>
        public UniTask<UserRankingData> GetMyRankData(string rankingName)
        {
            var completion = new UniTaskCompletionSource<UserRankingData>();

            if (!userRankingTableDic.ContainsKey(rankingName))
            {
                completion.TrySetException(new Exception("Ranking Name is not exist, name: " + rankingName));
                return completion.Task;
            }

            var curRankingTable = userRankingTableDic[rankingName];

            SendQueue.Enqueue(Backend.URank.User.GetMyRank, curRankingTable.uuid, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed to load my rank"))
                    return;

                var myRankJson = bro.GetFlattenJSON()["rows"][0];
                var myRankData = myRankJson.CreateUserRankingDataFromJson();

                if (!string.IsNullOrEmpty(curRankingTable.extraDataColumn))
                {
                    myRankData.extraData = myRankJson[curRankingTable.extraDataColumn].ToString();
                }

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

            if (!userRankingTableDic.ContainsKey(rankingName))
            {
                completion.TrySetException(new Exception("Ranking Name is not exist, name: " + rankingName));
                return completion.Task;
            }

            SendQueue.Enqueue(Backend.URank.User.GetRankRewardList, userRankingTableDic[rankingName].uuid, bro =>
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
        /// 유저랭킹 테이블 정보를 가져온다.
        /// </summary>
        /// <returns></returns>
        private UniTask<Dictionary<string, UserRankingTableData>> GetRankTableList()
        {
            var completion = new UniTaskCompletionSource<Dictionary<string, UserRankingTableData>>();

            SendQueue.Enqueue(Backend.URank.User.GetRankTableList, bro =>
            {
                if (!bro.CheckSuccess(completion, "Failed Initialize Ranking"))
                    return;

                var rankTableJsonList = bro.FlattenRows();
                var rankTableDic = new Dictionary<string, UserRankingTableData>();

                for (var i = 0; i < rankTableJsonList.Count; ++i)
                {
                    var curJson = rankTableJsonList[i];
                    var tableName = curJson["table"].ToString();
                    var rankTableData = new UserRankingTableData
                    {
                        rankType = curJson["rankType"].ToString(),
                        dateType = (RankDateType)Enum.Parse(typeof(RankDateType), curJson["date"].ToString().FirstCharToUpper()),
                        uuid = curJson["uuid"].ToString(),
                        tableName = tableName,
                        tableInDate = BackendTable.Instance.GetTableInDate(tableName), 
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