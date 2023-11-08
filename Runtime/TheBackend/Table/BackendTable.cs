using System.Collections.Generic;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;
using LitJson;

namespace IdleGameModule.TheBackend
{
    public class BackendTable : Singleton<BackendTable>
    {
        private string[] tableNames;
        private readonly Dictionary<string, string> _myInDateDictionary = new Dictionary<string, string>();
        private readonly List<TransactionValue> _transactionWriteList = new List<TransactionValue>();

        #region Table Control
        
        /// <summary>
        /// 테이블들을 로드하고 초기화 해줌
        /// </summary>
        /// <param name="modelDic"></param>
        public async UniTask LoadTables(Dictionary<string, BaseModel> modelDic)
        {
            var emptyTableList = new List<string>();

            _myInDateDictionary.Clear();

            // 1. 테이블 이름들 가져옴
            tableNames = await GetTableNames();

            // 2. 데이터를 로드해본 후 테이블이 비어 있다면 Insert, 채워져 있다면 그대로 진행
            foreach (var tableName in tableNames)
            {
                var tableJson = await LoadMyTable(tableName);

                if (tableJson == null)
                {
                    emptyTableList.Add(tableName);
                    continue;
                }

                if (modelDic.ContainsKey(tableName))
                    modelDic[tableName].AcceptFromJson(tableJson);
            }

            foreach (var emptyTable in emptyTableList)
            {
                await InsertMyTable(emptyTable, modelDic[emptyTable]);
            }
        }

        /// <summary>
        /// 테이블 업데이트
        /// </summary>
        /// <param name="tableName">테이블 이름</param>
        /// <param name="param">param 정보</param>
        /// <returns></returns>
        public UniTask UpdateMyTable(string tableName, Param param)
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.GameData.UpdateV2, tableName, _myInDateDictionary[tableName], Backend.UserInDate, param, bro =>
            {
                if (!bro.CheckSuccess(completion, "failed Update Table"))
                    return;

                completion.TrySetResult();
            });

            return completion.Task;
        }
        
        #endregion
        
        #region Transaction

        /// <summary>
        /// 업데이트 트랜잭션 추가
        /// </summary>
        /// <param name="tableName">테이블 이름</param>
        /// <param name="param">관련 param</param>
        public void AddTransactionUpdate(string tableName, Param param)
        {
            if (FindTableInListAndAppend(_transactionWriteList, tableName, param) == false)
                _transactionWriteList.Add(TransactionValue.SetUpdateV2(tableName, _myInDateDictionary[tableName], Backend.UserInDate, param));
        }

        /// <summary>
        /// 트랜잭션을 모두 보냄
        /// 주의: 트랜잭션 list가 10개 이상 넘기면 안됨
        /// </summary>
        /// <returns></returns>
        public UniTask RunTransactionWrite()
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.GameData.TransactionWriteV2, _transactionWriteList, bro =>
            {
                if (!bro.CheckSuccess(completion, "Transaction Error"))
                    return;

                completion.TrySetResult();
            });

            return completion.Task;
        }

        private bool FindTableInListAndAppend(List<TransactionValue> transactionValues, string tableName, Param param)
        {
            var findIndex = transactionValues.FindIndex(tv => tv.table == tableName);

            if (findIndex < 0)
                return false;

            var curParam = transactionValues[findIndex].param;

            foreach (var key in param.GetKeyList())
            {
                if (curParam.ContainsKey(key))
                    curParam[key] = param[key];
                else
                    curParam.Add(key, param[key]);
            }

            return true;
        }

        #endregion

        /// <summary>
        /// 테이블의 InDate를 반환
        /// </summary>
        /// <param name="tableName">테이블 이름</param>
        /// <returns></returns>
        public string GetTableInDate(string tableName)
        {
            return _myInDateDictionary[tableName];
        }

        /// <summary>
        /// 테이블 이름들을 가져옴
        /// </summary>
        /// <returns></returns>
        private UniTask<string[]> GetTableNames()
        {
            var completion = new UniTaskCompletionSource<string[]>();

            SendQueue.Enqueue(Backend.GameData.GetTableList, bro =>
            {
                if (!bro.CheckSuccess(completion, "failed Load Table List"))
                    return;

                var tableJsonList = bro.GetReturnValuetoJSON()["tables"];
                var tableList = new List<string>();

                for (var i = 0; i < tableJsonList.Count; ++i)
                {
                    var curJson = tableJsonList[i];

                    if (curJson["isChecked"].ToString() == "false")
                        continue;

                    tableList.Add(curJson["tableName"].ToString());
                }

                completion.TrySetResult(tableList.ToArray());
            });

            return completion.Task;
        }

        /// <summary>
        /// 내 테이블을 정보를 로드
        /// </summary>
        /// <returns>테이블이 존재하는가</returns>
        private UniTask<JsonData> LoadMyTable(string tableName)
        {
            var completion = new UniTaskCompletionSource<JsonData>();

            SendQueue.Enqueue(Backend.GameData.GetMyData, tableName, new Where(), 1, bro =>
            {
                if (!bro.CheckSuccess(completion, "failed Load Table load"))
                    return;

                if (bro.Rows().Count == 0)
                    completion.TrySetResult(null);
                else
                    completion.TrySetResult(bro.FlattenRows()[0]);
            });

            return completion.Task;
        }

        /// <summary>
        /// 테이블을 서버에 덮어씌움
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private UniTask InsertMyTable(string tableName, BaseModel model)
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.GameData.Insert, tableName, model.GetParam(), bro =>
            {
                if (!bro.CheckSuccess(completion, "failed Insert Table"))
                    return;

                _myInDateDictionary.TryAdd(tableName, bro.GetReturnValuetoJSON()["inDate"].ToString());
                completion.TrySetResult();
            });

            return completion.Task;
        }
    }
}