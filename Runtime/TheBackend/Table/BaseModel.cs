using BackEnd;
using LitJson;

namespace IdleGameModule.TheBackend
{
    /// <summary>
    /// 테이블의 기본 클래스
    /// </summary>
    public abstract class BaseModel
    {
        /// <summary>
        /// 로드된 jsonData를 클래스에 적용함
        /// </summary>
        /// <param name="jsonData"></param>
        public abstract void AcceptFromJson(JsonData jsonData);

        /// <summary>
        /// 해당 클래스의 Param정보를 가져옴
        /// </summary>
        /// <returns></returns>
        public abstract Param GetParam();
        
        /// <summary>
        /// paramKey를 입력하여 원하는 Param만 반환
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public Param GetParam(params string[] keys)
        {
            var allParam = GetParam();
            var ret = new Param();

            foreach (var key in keys)
            {
                if (!allParam.ContainsKey(key))
                    continue;

                ret.Add(key, allParam[key]);
            }

            return ret;
        }

        protected string GetElement(JsonData json, string id) => json[id].ToString();
    }
}