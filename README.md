# IdleGameModule
방치형 게임에서 자주 사용 되는 라이브러리들을 쉽게 사용할 수 있게 하는 모듈 


# Requirements
- UniTask (v2.4.1)
- TheBackend (v5.11.1)
- iOS Advertising Support (1.0.0)
- BigDouble (BreakInfinity)

# 프로젝트 세팅

### Newtonsoft

- `manifest.json` 파일에 `"com.unity.nuget.newtonsoft-json": "3.2.1"` 추가


### 뒤끝
- SendQueueMgr 스크립트를 포함한 오브젝트를 게임 시작 씬에 추가

# 구현 기능

## 뒤끝

### 로그인

- 현재 게스트 로그인만 지원, 구글/애플 로그인 모듈 추가 이후 페더레이션 기능 추가 예정
- TryLogin() 함수를 이용하여 로그인 진행
- RefreshAccessToken() 함수는 AccessToken을 업데이트 해줌, 12시간 주기로 업데이트가 필요 (24시간 동안 유효한 토큰)

### 차트

- GetAllChartData() 함수를 로딩 중에 불러 적용된 차트 전부 로드
- 위 함수에서 리턴한 chartData를 적용 시키면 됨

### 뽑기

- LoadAllProbabilityCards() 함수를 로딩 중에 불러 확률표 전부 로드
- 확률표 네이밍은 {GachaName}_{Level} 로 통일, 레벨이 없는 경우 그냥 {GachaName}만 적어도 됨
- 아쉽게도 확률은 가져 오지 못함. 확률은 따로 차트에 넣어서 로드 하거나 로컬에 저장 하는게 좋을듯

### 공지사항

- GetNoticeList() 함수로 모든 공지사항 로드
- isRead로 공지사항 읽은 여부 확인 (이건 로컬에 따로 저장됨)
- 공지사항 읽을때 AddReadList() 함수 호출

### 우편

우편이 업데이트가 되면서 어드민, 랭킹, 쿠폰 보상으로 3분할 되었다. 하지만 거의 동시에 받는 경우가 대부분 일듯함  
따라서 쿠폰만 옵션으로 두고 나머지는 계속 가져오도록 설정

### 랭킹

- InitUserRanking() 함수를 로딩 중에 불러서 초기화
- 콘솔에 등록된 랭킹 이름을 따라감
- UpdateUserScore() 함수를 이용 하여 점수 등록

### 테이블

- LoadTables() 함수를 로딩 중에 불러서 초기화
- Transaction, UpdateMyTable 등 자유롭게 저장 시점을 고려 하여 구현
- 현재는 Transaction 진행 중에 추가 Transaction이 들어올 경우 대비가 되어 있지 않아서 관련하여 해결해야 함

#### BaseModel

모든 데이터 집합의 부모가 되는 클래스

**구현 해야 할 항목**
- 처음 로드 받았을 때 값들을 적용 하는 함수  
- 저장할 때 Param 데이터를 return 하는 함수

### 유틸

#### 제공 기능
- 버전 체크
- Utc 시간 불러오기
- iOS14 ATTracking 체크

#### 참고사항
- 로그인 이후 StartInternalServerTime() 함수를 실행 하여 주기적으로 서버시간을 가져옴

### 로그

단순히 로그 타입과 Param을 보내준다.  
아래 코드를 참고하여 Param을 생성하는 Util을 만들어주면 좀 더 편할듯 

```
public static class LogUtil
{
    public static Param QuestLog(string timeLimitEventType, string questType, int addCandy)
    {
        return new Param
            {
                {nameof(timeLimitEventType), timeLimitEventType},
                {nameof(questType), questType},
                {nameof(addCandy), addCandy.ToString()},
            };
        }
    }
}
```

### 미구현 기능 ( 추후 업데이트 )
- 푸쉬
- 뒤끝 채팅
- 길드
- 길드 랭킹
- 서버 선택
- 소셜

---------------------------

# 제작중

## 로그인 (Google, Apple)

### 설치 방법

## 통계 (Adjust)

## 인앱 결제 (IAP)

## 광고 (Ads)

## 유틸 함수 (Utils)