using System.Collections.Generic;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;
using LitJson;

namespace IdleGameModule.TheBackend
{
    public class ChartCardData
    {
        public string chartName;
        public string chartExplain;
        public int selectedChartFileId;
    }

    public class ChartData
    {
        public string chartName;
        public JsonData jsonData;

        public ChartData(string chartName)
        {
            this.chartName = chartName;
        }
    }

    /// <summary>
    /// 뒤끝에 저장되어 있는 차트를 가져오는 클래스 
    /// </summary>
    public class BackendChart : Singleton<BackendChart>
    {
        /// <summary>
        /// 뒤끝에 적용된 모든 차트를 로드함
        /// 로컬에 저장된 차트가 있다면 로컬 차트를 로드
        /// </summary>
        /// <returns></returns>
        public async UniTask<Dictionary<string, ChartData>> GetAllChartData()
        {
            var chartDic = new Dictionary<string, ChartData>();
            var chartCardList = await GetChartCardList();

            foreach (var chartCard in chartCardList)
            {
                var chartData = await GetLoadAndSaveChart(chartCard);

                chartDic.Add(chartCard.chartName, chartData);
            }

            return chartDic;
        }

        /// <summary>
        /// 차트 리스트를 불러온다.
        /// </summary>
        /// <returns></returns>
        private UniTask<List<ChartCardData>> GetChartCardList()
        {
            var completion = new UniTaskCompletionSource<List<ChartCardData>>();

            SendQueue.Enqueue(Backend.Chart.GetChartListV2, bro =>
            {
                if (!bro.IsSuccess())
                {
                    completion.TrySetException(bro.CreateException("Failed Chart Load"));
                    return;
                }

                completion.TrySetResult(ParseToChartData(bro));
            });

            return completion.Task;
        }

        /// <summary>
        /// 차트를 불러오고 로컬레 저장한다
        /// </summary>
        /// <param name="cardData"></param>
        /// <returns></returns>
        private UniTask<ChartData> GetLoadAndSaveChart(ChartCardData cardData)
        {
            var localChartData = Backend.Chart.GetLocalChartData(cardData.selectedChartFileId.ToString());
            var completion = new UniTaskCompletionSource<ChartData>();
            var chartData = new ChartData(cardData.chartName);

            if (string.IsNullOrEmpty(localChartData))
            {
                // 서버에서 받아오기
                SendQueue.Enqueue(Backend.Chart.GetOneChartAndSaveV2, cardData.selectedChartFileId.ToString(), bro =>
                {
                    if (!bro.CheckSuccess(completion, $"Failed download chart..{cardData.chartName}"))
                        return;

                    chartData.jsonData = bro.FlattenRows();
                    completion.TrySetResult(chartData);
                });
            }
            else
            {
                chartData.jsonData = BackendReturnObject.Flatten(JsonMapper.ToObject(localChartData)["rows"]);
                completion.TrySetResult(chartData);
            }

            return completion.Task;
        }

        /// <summary>
        /// 뒤끝 데이터를 차트로 변환
        /// </summary>
        /// <param name="bro"></param>
        /// <returns></returns>
        private List<ChartCardData> ParseToChartData(BackendReturnObject bro)
        {
            var jsonData = bro.FlattenRows();
            var chartDataList = new List<ChartCardData>();

            for (var i = 0; i < jsonData.Count; ++i)
            {
                var curJson = jsonData[i];

                chartDataList.Add(new ChartCardData
                {
                    chartName = curJson["chartName"].ToString(),
                    chartExplain = curJson["chartExplain"].ToString(),
                    selectedChartFileId = int.Parse(curJson["selectedChartFileId"].ToString())
                });
            }

            return chartDataList;
        }
    }
}