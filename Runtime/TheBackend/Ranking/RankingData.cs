using System;
using JetBrains.Annotations;
using LitJson;

namespace IdleGameModule.TheBackend
{
    #region Ranking Table

    public class RankingTableData
    {
        public string rankType;
        public RankDateType dateType;
        public string uuid;
        public string tableName; // 랭킹에 사용한 테이블 이름

        // 일회성 랭킹
        public DateTime? rankStartTime;
        public DateTime? rankEndTime;

        // 추가 항목
        [CanBeNull] public string extraDataColumn; // 랭킹 추가 항목으로 사용한 컬럼 명
        [CanBeNull] public string extraDataType;
    }


    /// <summary>
    /// 유저랭킹 테이블 데이터
    /// </summary>
    public class UserRankingTableData : RankingTableData
    {
        public string tableInDate; // 랭킹 갱신에 사용할 테이블 inDate ( Ranking보다 BackendTable이 먼저 초기화 되야함 )
    }

    /// <summary>
    /// 길드 랭킹 테이블 데이터
    /// </summary>
    public class GuildRankingTableData : RankingTableData
    {

    }

    #endregion

    #region Ranking Elements

    public class RankingData
    {
        public double score;
        public string index;
        public string rank;
    }

    /// <summary>
    /// 유저 랭킹관련 데이터
    /// </summary>
    public class UserRankingData : RankingData
    {
        public string gamerInDate;
        public string nickname;

        // 추가 정보
        public string extraData = string.Empty;
    }

    public class GuildRankingData : RankingData
    {
        public string guildInDate;
        public string guildName;
    }

    #endregion

    #region Ranking Rewards

    public class RankingRewardData
    {
        public int startRank;
        public int endRank;
        public string itemId;
        public int rewardItemCount;
    }

    #endregion
}