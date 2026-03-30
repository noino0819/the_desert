# The SSand 프로젝트 규칙

## 프로젝트 개요
- 장르: 2D 횡스크롤 RPG / 생존
- 엔진: Unity 2D (URP)
- 네임스페이스: TheSSand.{Core, Player, Boss, Dialogue, Quest, UI, Level, Scene, Audio}

## 대본 태그 → Unity 매핑
| 태그 | Unity 구현 |
|------|-----------|
| [SCENE] | SceneManager 로드 시 초기화 |
| [CAM] | CameraController |
| [BGM] | AudioManager.CrossFade() |
| [SFX] | AudioManager.PlaySFX() |
| [ANIM] | Animator.SetTrigger() |
| [TRIGGER] | GameManager / QuestManager |
| [NARR] | NarrationUI |
| [CHOICE] | ChoiceUI (버튼 + 타이머) |
| [IF] | DialogueManager 조건 체크 |
| [FADE] | SceneTransitionManager |

## 핵심 시스템 규칙
1. 요정 대사는 용사(플레이어)에게만 들린다 (Ch.4 연구자 사망 장면만 예외)
2. 선택지 타이머: ⏱ N초 표기, 시간 초과 시 기본값 자동 선택. 씨앗 설치는 15초
3. 1회차: isNewGame2==false, Ch.1→Ch.4 / 2회차: isNewGame2==true, Ch.4→Ch.1
4. seedRefuseCount 10회 시 요정 무표정 → 용사 스스로 심음

## 플래그 (SaveData)
```
int   seedPlantedCount, seedRefuseCount, npcKillCount, goldenIdolCount, gold
bool  mudCookieDelivered, scarfPickedUp, scarfGiven, ch2ChildrenAlive, saidIllComeBack
bool  isNewGame2, ch1Killed~ch4Killed, hasDoubleJump, hasWallJump, hasDash, isDoubleAgent
int   ngSeedRecovered, ngTrueSeedPlanted
```

## 엔딩 판정 (우선순위)
1. npcKillCount >= 2 → Ed.3
2. goldenIdolCount >= 3 && gold >= 1000 → Ed.4
3. mudCookieDelivered && scarfGiven && ch2ChildrenAlive && saidIllComeBack → Ed.2
4. fallback → Ed.1
5. 2회차 별도: ngSeedRecovered==4 && ngTrueSeedPlanted==4 → Ed.5

## NPC 말투
| 캐릭터 | 말투 |
|--------|------|
| 할아버지 | 따뜻하고 느긋. 불편한 질문엔 화제 전환 |
| 요정 | 밝고 경쾌한 척. 씨앗 거절 시 점점 차가워짐 |
| 대장소년 | 짧고 무뚝뚝. 신뢰 쌓이면 조금씩 풀림 |
| 잼잼크래프트 | 빠르고 허풍. 어른 흉내 내는 아이 |
| 솔 | 따뜻하고 적극적. 루나 앞에서만 약해짐 |
| 루나 | 조용하고 사려 깊음. 솔 앞에서만 솔직 |
| 연구자 | 반말. 친한 형 느낌. 진실 발견 후 급격히 조용 |
| 손자 | 조용하고 결의에 찬. 분노와 슬픔 사이 |

## 싱글턴 (DontDestroyOnLoad)
GameManager, SaveManager, AudioManager, SceneTransitionManager, DialogueManager, QuestManager

## 대화 JSON 경로
- 1회차: Resources/Dialogues/{npcId}.json
- 2회차: Resources/Dialogues/ng/{npcId}.json

## 씬 이름 규칙
SCN_TitleMenu, SCN_Prologue, SCN_Ch{N}_Desert, SCN_Ch{N}_Pyramid, SCN_Ch{N}_Oasis, SCN_Ch{N}_Boss, SCN_Ending, SCN_NG_Ch{N}_Oasis, SCN_NG_Ch{N}_Boss
