using System;
using System.Collections.Generic;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;
using LitJson;

namespace IdleGameModule.TheBackend
{
    public class PostData
    {
        public PostType postType;
        public string title; // 타이틀
        public string content; // 내용
        public DateTime expirationDate; // 만료 날짜
        public DateTime sentDate; // 우편 보낸 날짜
        public string inDate;
        public PostItemData[] postItems;
    }

    public class PostItemData
    {
        public string itemId;
        public int count;
    }

    public class BackendPost : Singleton<BackendPost>
    {
        /// <summary>
        /// Admin, Rank, Coupon 타입의 우편을 가져온다. (User는 제외)
        /// </summary>
        /// <returns></returns>
        public async UniTask<Dictionary<PostType, PostData[]>> GetAllPosts(int limit, bool includeCoupon = false)
        {
            var postDic = new Dictionary<PostType, PostData[]>();

            var adminPosts = await GetPosts(PostType.Admin, limit);
            var rankPosts = await GetPosts(PostType.Rank, limit);

            postDic.Add(PostType.Admin, adminPosts);
            postDic.Add(PostType.Rank, rankPosts);

            if (includeCoupon)
            {
                var couponPosts = await GetPosts(PostType.Coupon, limit);
                postDic.Add(PostType.Coupon, couponPosts);
            }

            return postDic;
        }

        /// <summary>
        /// 모든 타입의 우편을 받는다. 
        /// </summary>
        /// <param name="includeCoupon"></param>
        public async UniTask ReceiveAllPostTypes(bool includeCoupon = false)
        {
            await ReceiveAll(PostType.Admin);
            await ReceiveAll(PostType.Rank);

            if (includeCoupon)
                await ReceiveAll(PostType.Coupon);
        }

        /// <summary>
        /// 해당 PostType에 맞는 우편을 가져온다.
        /// </summary>
        /// <param name="postType">우편 타입</param>
        /// <param name="limit">우편 개수</param>
        /// <returns></returns>
        public UniTask<PostData[]> GetPosts(PostType postType, int limit)
        {
            var completion = new UniTaskCompletionSource<PostData[]>();

            SendQueue.Enqueue(Backend.UPost.GetPostList, postType, limit, bro =>
            {
                if (!bro.CheckSuccess(completion, "Get Post Error"))
                    return;

                completion.TrySetResult(JsonToPostDataArray(postType, bro.GetReturnValuetoJSON()));
            });

            return completion.Task;
        }

        /// <summary>
        /// PostType에 해당하는 우편 1개를 받는다.
        /// </summary>
        /// <param name="postType"></param>
        /// <param name="inDate"></param>
        /// <returns></returns>
        public UniTask ReceiveOne(PostType postType, string inDate)
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.UPost.ReceivePostItem, postType, inDate, bro =>
            {
                if (!bro.CheckSuccess(completion, "Receive One Error"))
                    return;

                completion.TrySetResult();
            });

            return completion.Task;
        }

        /// <summary>
        /// PostType에 해당하는 우편을 전부 받는다.
        /// </summary>
        /// <param name="postType"></param>
        /// <returns></returns>
        public UniTask ReceiveAll(PostType postType)
        {
            var completion = new UniTaskCompletionSource();

            SendQueue.Enqueue(Backend.UPost.ReceivePostItemAll, postType, bro =>
            {
                if (!bro.IsSuccess())
                {
                    // 더 이상 수령할 우편이 없는 경우는 그냥 패스
                    if (bro.GetStatusCode() == "404" && bro.GetErrorCode() == "NotFoundException")
                        completion.TrySetResult();
                    else
                        completion.TrySetException(bro.CreateException("Receive All Error"));

                    return;
                }

                completion.TrySetResult();
            });

            return completion.Task;
        }

        private PostData[] JsonToPostDataArray(PostType postType, JsonData jsonData)
        {
            var postDataList = new List<PostData>();
            var postListJson = jsonData["postList"];

            for (var i = 0; i < postListJson.Count; ++i)
            {
                var curJson = postListJson[i];
                var newPost = new PostData
                {
                    postType = postType,
                    title = curJson.GetStringDirect("title"),
                    content = curJson.GetStringDirect("content"),
                    expirationDate = curJson.GetStringDirect("expirationDate").ToDateTime(),
                    sentDate = curJson.GetStringDirect("sentDate").ToDateTime(),
                    inDate = curJson.GetStringDirect("inDate"),
                };

                // Items
                var items = curJson["items"];
                newPost.postItems = new PostItemData[items.Count];
                for (var j = 0; j < items.Count; ++j)
                {
                    var curItem = items[j];

                    newPost.postItems[j] = new PostItemData()
                    {
                        itemId = curItem["item"]["itemId"].ToString(),
                        count = curItem.GetIntDirect("itemCount"),
                    };
                }

                postDataList.Add(newPost);
            }

            return postDataList.ToArray();
        }
    }
}