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

        protected string GetElement(JsonData json, string id) => json[id].ToString();
    }
}