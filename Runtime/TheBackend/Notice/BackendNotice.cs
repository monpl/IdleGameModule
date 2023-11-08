using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using IdleGameModule.Common;
using Newtonsoft.Json;
using UnityEngine;

namespace IdleGameModule.TheBackend
{
    public class NoticeData
    {
        public string title;
        public string content;
        public DateTime postingDate;
        public bool isPublic;
        public string uuid;
        public string inDate;
        public string imageKey;
        public string linkUrl;
        public bool isRead;
    }

    public class BackendNotice : Singleton<BackendNotice>
    {
        private readonly string _seenNoticeUuidListKey = "SEEN_NOTICE_LIST";

        /// <summary>
        /// 공지사항 리스트를 전부 가져옴
        /// </summary>
        /// <returns></returns>
        public UniTask<NoticeData[]> GetNoticeList()
        {
            var completion = new UniTaskCompletionSource<NoticeData[]>();

            SendQueue.Enqueue(Backend.Notice.NoticeList, bro =>
            {
                if (!bro.CheckSuccess(completion, "Notice Load Error"))
                    return;

                var json = bro.Rows();
                var ret = new NoticeData[json.Count];
                var readNoticeList = GetReadNoticeList();

                for (var i = 0; i < json.Count; ++i)
                {
                    var curJson = json[i];
                    var isPublic = curJson.GetString("isPublic") == "y";
                    var uuid = curJson.GetString("uuid");

                    ret[i] = new NoticeData
                    {
                        title = curJson.GetString("title").Replace("\\n", "\n"),
                        content = curJson.GetString("content").Replace("\\n", "\n"),
                        postingDate = DateTime.Parse(curJson.GetString("postingDate")),
                        isPublic = isPublic,
                        imageKey = curJson.GetString("imageKey"),
                        inDate = curJson.GetString("inDate"),
                        uuid = uuid,
                        linkUrl = curJson.GetString("linkUrl"),
                        isRead = readNoticeList.Contains(uuid),
                    };
                }

                RemoveDeletedReadNoticeList(readNoticeList, ret);

                completion.TrySetResult(ret);
            });

            return completion.Task;
        }

        /// <summary>
        /// 공지사항을 읽을때 호출 ( 읽은 리스트에 추가 )
        /// </summary>
        /// <param name="uuid">공지사항의 uuid</param>
        public void AddReadList(string uuid)
        {
            var list = GetReadNoticeList();

            if (!list.Contains(uuid))
                list.Add(uuid);

            SetReadNoticeList(list);
        }

        private List<string> GetReadNoticeList()
        {
            return JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString(_seenNoticeUuidListKey)) ?? new List<string>();
        }

        private void SetReadNoticeList(List<string> readNoticeUuidList)
        {
            PlayerPrefs.SetString(_seenNoticeUuidListKey, JsonConvert.SerializeObject(readNoticeUuidList));
        }

        /// <summary>
        /// 이미 읽은 list의 uuid중 현재 불러온 notice에 없는 애들을 삭제
        /// </summary>
        /// <param name="readNoticeList"></param>
        /// <param name="noticeDataArr"></param>
        private void RemoveDeletedReadNoticeList(List<string> readNoticeList, NoticeData[] noticeDataArr)
        {
            readNoticeList.RemoveAll(uuid => noticeDataArr.Count(obj => obj.uuid == uuid) == 0);

            SetReadNoticeList(readNoticeList);
        }
    }
}